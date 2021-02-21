using System;
using System.IO;
using System.Linq;
using AttendanceTracker.Models;
using AttendanceTracker.Models.IServices;
using AttendanceTracker.Models.Repository;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Microsoft.VisualBasic.FileIO;
using QRCoder;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using AttendanceTracker.Models.Contracts;
using AttendanceTracker.Models.IServices;
using AttendanceTracker.Models.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.FileIO;
using OfficeOpenXml;
using QRCoder;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
namespace AttendanceTracker.Controllers
{
    [Route("api/Attendance")]
    [ApiController]
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;
        private readonly IDataRepository<Classroom> _classroomRepo;
        private readonly IDataRepository<Student> _studentRepo;
        private readonly IDataRepository<Attendance> _attendanceRepo;
        private readonly IDataRepository<Studentclassroom> _studentclassroomRepo;

        public AttendanceController(IAttendanceService attendanceService, IDataRepository<Classroom> classroomRepo,
            IDataRepository<Student> studentRepo,
            IDataRepository<Attendance> attendanceRepo,
            IDataRepository<Studentclassroom> studentclassroomRepo)
        {
            _attendanceService = attendanceService;
            _classroomRepo = classroomRepo;
            _studentRepo = studentRepo;
            _attendanceRepo = attendanceRepo;
            _studentclassroomRepo = studentclassroomRepo;

        }

        // GET
        [HttpGet]
        public IActionResult Index([FromQuery] int classRoomId, [FromQuery] string attendanceDate)
        {
            var attendances = _attendanceService.GetAttendance(attendanceDate, classRoomId);
            return Ok(attendances);
        }

        // POST
        [HttpPost]
        public IActionResult Create([FromBody] Student student)
        {
            var attendance = _attendanceService.CreateAttendance(student.IcNumber);
            return Ok(attendance);
        }

        // GET
        [HttpGet("ExportAttendance")]
        public IActionResult DownloadAttendance([FromQuery] int classRoomId, [FromQuery] string attendanceDate,
            [FromQuery] string format)
        {
            var attendances = _attendanceService.GetAttendance(attendanceDate, classRoomId);

            var classRoom = _classroomRepo.Get(x => x.Id == classRoomId).FirstOrDefault();
            var classRoomName = "";
            if (classRoom != null)
            {
                classRoomName = classRoom.Grade + " " + classRoom.Name;
            }

            var stream = new MemoryStream();

            using (var package = new ExcelPackage(stream))
            {
                var workSheet = package.Workbook.Worksheets.Add("Sheet1");
                workSheet.Cells.LoadFromCollection(attendances, true);
                package.Save();
            }

            stream.Position = 0;
            string excelName = $"Attendance-{classRoomName}-{attendanceDate}.xlsx";

            //return File(stream, "application/octet-stream", excelName);  
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }

        // GET
        [HttpGet("GenerateClassroomCodes/{classRoomId}")]
        public IActionResult GenerateClassroomCodes(int classRoomId)
        {
            var pdf = GenCodes(classRoomId);
            return pdf;
        }

        FileStreamResult GenCodes(int classRoomId)
        {
            var classRooms = _classroomRepo.GetAll().Where(x=>x.Id == classRoomId).ToList();
            FileStreamResult result = null;
            foreach (var holderClassroom in classRooms)
            {
                // var students = _studentRepo.GetAll().Where(x => x.ClassroomId == holderClassroom.Id).ToList();
                var classroomStudentIds = _studentclassroomRepo.GetAll()
                    .Where(x => x.ClassroomId == holderClassroom.Id && x.IsCurrent.HasValue && x.IsCurrent == true)
                    .Select(x => x.StudentId).ToList();
                    
                    
                var students = _studentRepo.GetAll().Where(x => classroomStudentIds.Contains(x.Id)).ToList();
                PointF firstLocation = new PointF(10f, 140f);
                PointF secondLocation = new PointF(10f, 160f);
                PointF thirdLocation = new PointF(10f, 180f);
                PointF qrCodeLocation = new PointF(180f, 15f);
                PointF schoolLogoLocation = new PointF(0f, 15f);
                Bitmap schoolLogoImage =
                    new Bitmap(
                        @"C:\Users\LENOVO\Documents\repos\attendance-tracker\src\assets\images\skpp112_card.jpeg");
                int width = 323;
                int height = 204;
                var cardCanvas = new Bitmap(width, height);

                var bitmapPathList = new List<string>();
                foreach (var student in students)
                {
                    if (student.IcNumber.Length > 0)
                    {
                        QRCodeGenerator qrGenerator = new QRCodeGenerator();
                        QRCodeData qrCodeData = qrGenerator.CreateQrCode(student.IcNumber, QRCodeGenerator.ECCLevel.Q);
                        QRCode qrCode = new QRCode(qrCodeData);
                        Bitmap qrCodeImage = qrCode.GetGraphic(20);

                        var studentClassroom = this.GetStudentClassroom(student);

                        var classroom = _classroomRepo.Get(x => x.Id == studentClassroom.ClassroomId).FirstOrDefault();
                        if (classroom != null)
                        {
                            using (Graphics graphics = Graphics.FromImage(cardCanvas))
                            {
                                // adding background image
                                using (SolidBrush brush = new SolidBrush(Color.FromKnownColor(KnownColor.White)))
                                {
                                    graphics.FillRectangle(brush, 0, 0, width, height);
                                }

                                // adding qr code
                                Bitmap schoolLogo = new Bitmap(schoolLogoImage,
                                    new Size(schoolLogoImage.Width / 7, schoolLogoImage.Height / 7));
                                graphics.DrawImage(schoolLogo, schoolLogoLocation);

                                // adding qr code
                                Bitmap resizedQrCode = new Bitmap(qrCodeImage,
                                    new Size(qrCodeImage.Width / 4, qrCodeImage.Height / 4));
                                graphics.DrawImage(resizedQrCode, qrCodeLocation);

                                // adding font
                                using (Font arialFont = new Font("Calibri", 15))
                                {
                                    string[] words = student.Name.Split(" ");
                                    int wordIterator = 0;
                                    string firstName = "";
                                    string secondName = "";
                                    foreach (var word in words)
                                    {
                                        if (wordIterator <= 2)
                                        {
                                            firstName += word;
                                            firstName += " ";
                                        }
                                        else
                                        {
                                            secondName += word;
                                            secondName += " ";
                                        }

                                        wordIterator += 1;
                                    }

                                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                                    graphics.DrawString(firstName, arialFont, Brushes.Black, firstLocation);
                                    graphics.DrawString(secondName, arialFont, Brushes.Black, secondLocation);
                                    graphics.DrawString(classroom.Grade + " " + classroom.Name, arialFont,
                                        Brushes.Black,
                                        thirdLocation);
                                }
                            }

                            string imageFilePath = @"C:\Users\LENOVO\Downloads\studentimages\" + student.Id + ".bmp";
                            //
                            Console.WriteLine(imageFilePath);
                            cardCanvas.Save(imageFilePath);
                            bitmapPathList.Add(imageFilePath);
                            // CreatePDF(qrCodeImage);
                        }
                    }
                }

                result = CreatePDF(bitmapPathList, holderClassroom.Grade + " "+  holderClassroom.Name);
            }

            return result;
        }

        private Studentclassroom GetStudentClassroom(Student student)
        {
            var currentTime = DateTime.Now;
            return  _studentclassroomRepo.GetAll().Where(x => x.StudentId == student.Id && x.IsCurrent == true).FirstOrDefault();
        }

        public FileStreamResult CreatePDF(List<string> bitmapPathList, string classroomName)
        {
            PdfDocument doc = new PdfDocument();
            PdfPage page = doc.Pages.Add();
            PdfGrid pdfGrid = new PdfGrid();
            pdfGrid.Columns.Add();
            pdfGrid.Columns.Add();
            
          
            for (int i = 0; i < bitmapPathList.Count; i+=2)
            {
                PdfGridRow pdfGridRow = pdfGrid.Rows.Add();
                pdfGridRow.Height = 150;
                
                
                FileStream fs1 = new FileStream(@bitmapPathList[i], FileMode.Open); 
                PdfBitmap pBmp1 = new PdfBitmap(fs1);
             
                pdfGridRow.Cells[0].Style.BackgroundImage = pBmp1;

                if ((i + 1) <= (bitmapPathList.Count - 1))
                {
                    FileStream fs2 = new FileStream(@bitmapPathList[i + 1], FileMode.Open); 
                    PdfBitmap pBmp2 = new PdfBitmap(fs2);
                    pdfGridRow.Cells[1].Style.BackgroundImage = pBmp2;

                }
            }
        
            pdfGrid.Draw(page, new Syncfusion.Drawing.PointF(10, 10));
            // FileStream fileStream = new FileStream( @"genpdf\" + classroomName + ".pdf", FileMode.CreateNew, FileAccess.ReadWrite);
            // MemoryStream stream = new MemoryStream();

            // doc.Save(stream);
            // return File(stream, "application/pdf", classroomName + ".pdf");
            
            MemoryStream stream = new MemoryStream();
  
            doc.Save(stream);
  
            //Set the position as '0'.
            stream.Position = 0;
  
            //Download the PDF document in the browser
            FileStreamResult fileStreamResult = new FileStreamResult(stream, "application/pdf");
  
            fileStreamResult.FileDownloadName = classroomName + ".pdf";
  
            return fileStreamResult;
            
        }

    }
}