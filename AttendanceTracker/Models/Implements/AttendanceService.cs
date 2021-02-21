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
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using OfficeOpenXml;
using QRCoder;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;

namespace AttendanceTracker.Models.Implements
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IDataRepository<Classroom> _classroomRepo;
        private readonly IDataRepository<Student> _studentRepo;
        private readonly IDataRepository<Attendance> _attendanceRepo;
        private readonly IDataRepository<Studentclassroom> _studentclassroomRepo;

        public AttendanceService(IDataRepository<Classroom> classroomRepo,
            IDataRepository<Student> studentRepo,
            IDataRepository<Attendance> attendanceRepo,
            IDataRepository<Studentclassroom> studentclassroomRepo)
        {
            _classroomRepo = classroomRepo;
            _studentRepo = studentRepo;
            _attendanceRepo = attendanceRepo;
            _studentclassroomRepo = studentclassroomRepo;

        }

        public IEnumerable<AttendanceResponse> GetAttendance(string attendanceDate, int classRoomId)
        {
            IEnumerable<AttendanceResponse> attendanceResponses = new AttendanceResponse[] { };
            DateTime? filterDate = null;
            if (attendanceDate == null)
            {
                filterDate = DateTime.Today.Date;
            }
            else
            {
                filterDate = DateTime.Parse(attendanceDate).Date;
            }
            

            if (classRoomId == 0)
            {
                // return all attendances
                var attendances = _attendanceRepo.GetAll().Where(x => x.CreatedAt.Value.Date == filterDate)
                    .OrderByDescending(x => x.CreatedAt).ToList();
                foreach (var att in attendances)
                {
                    attendanceResponses = attendanceResponses.Append(CreateAttendanceResponse(att, false, null));
                }
            }
            else
            {

                Classroom classroom = _classroomRepo.Get(x => x.Id == classRoomId).FirstOrDefault();
                if (classroom != null)
                {
                    // getting student classrooms first, and then filter by classroom and is current, and get student by those ids
                    var classroomStudentIds = _studentclassroomRepo.GetAll()
                        .Where(x => x.ClassroomId == classroom.Id && x.IsCurrent.HasValue && x.IsCurrent == true)
                        .Select(x => x.StudentId).ToList();
                    
                    
                    IEnumerable<Student> students = _studentRepo.GetAll().Where(x => classroomStudentIds.Contains(x.Id)).ToList();
                    foreach (var student in students)
                    {
                        var attendanceChecking = _attendanceRepo.GetAll().Where(x =>
                            x.StudentId == student.Id && x.CreatedAt.Value.Date == filterDate);
                        if (attendanceChecking.Any())
                        {
                            // attended
                            attendanceResponses = attendanceResponses.Append(CreateAttendanceResponse(attendanceChecking.First(), true, student));

                        }
                        else
                        {
                            // did not attend
                            attendanceResponses = attendanceResponses.Append(CreateAttendanceResponse(null, false, student));

                        }
                    }
                }
            }
         

            return attendanceResponses;
        }

        public AttendanceResponse CreateAttendanceResponse(Attendance attendance, bool attended, Student studentObj)
        {
            var attendanceResponse = new AttendanceResponse();

            if (attendance != null )
            {
                var student = _studentRepo.Get(x => x.Id == attendance.StudentId).FirstOrDefault();
                if (student != null)
                {
                    // checking for today attendance
                    attendanceResponse.Success = true;
                    attendanceResponse.AttendanceId = attendance.Id;
                    attendanceResponse.StudentId = student.Id;
                    attendanceResponse.StudentName = student.Name;
                    attendanceResponse.StudentIcNumber = student.IcNumber;
                    
                    var studentClassroomRow = this.GetStudentClassroom(student);
                    if (studentClassroomRow != null)
                    {
                        var studentClassroom = _classroomRepo.Get(x => x.Id == studentClassroomRow.ClassroomId).FirstOrDefault();
                        if (studentClassroom != null)
                        {
                            attendanceResponse.ClassroomId = studentClassroom.Id;
                            attendanceResponse.ClassroomName = studentClassroom.Name;

                        }
                    }
                    attendanceResponse.CheckedInTime = attendance.CreatedAt;
                    TimeSpan end = new TimeSpan(7, 31, 0);
                    if (attendance.CreatedAt != null && attendance.CreatedAt.Value.TimeOfDay < end)
                    {
                        attendanceResponse.Status = "good";
                    }
                    else if (attendance.CreatedAt != null && attendance.CreatedAt.Value.TimeOfDay >= end)
                    {
                        attendanceResponse.Status = "bad";

                    }
                }
            }
            else 
            {
                attendanceResponse.Success = true;
                attendanceResponse.StudentId = studentObj.Id;
                attendanceResponse.StudentName = studentObj.Name;
                attendanceResponse.StudentIcNumber = studentObj.IcNumber;
                
                var studentClassroomRow = this.GetStudentClassroom(studentObj);
                if (studentClassroomRow != null)
                {
                    var studentClassroom = _classroomRepo.Get(x => x.Id == studentClassroomRow.ClassroomId).FirstOrDefault();
                    if (studentClassroom != null)
                    {
                        attendanceResponse.ClassroomId = studentClassroom.Id;
                        attendanceResponse.ClassroomName = studentClassroom.Name;

                    }
                }
                attendanceResponse.Status = "no-attendance";
            }
       

            return attendanceResponse;
        }

        public AttendanceResponse CreateAttendance(string icNumber)
        {
            var attendance = new Attendance();
            var attendanceResponse = new AttendanceResponse();
            var student = _studentRepo.Get((x) => x.IcNumber == icNumber).FirstOrDefault();
            if (student != null)
            {
                // checking for today attendance
                if (!_attendanceRepo
                    .GetAll().Any(x => x.StudentId == student.Id && x.CreatedAt.Value.Date == DateTime.Today.Date))
                {
                    attendance.StudentId = student.Id;
                    attendance.CreatedAt = DateTime.Now;
                    attendance.UpdatedAt = DateTime.Now;
                    attendance = _attendanceRepo.InsertOnCommit(attendance);
                    _attendanceRepo.CommitChanges();
                    attendanceResponse.Success = true;
                    attendanceResponse.AttendanceId = attendance.Id;
                    attendanceResponse.StudentId = student.Id;
                    attendanceResponse.StudentName = student.Name;
                    attendanceResponse.StudentIcNumber = student.IcNumber;
                    
                    var studentClassroomRow = this.GetStudentClassroom(student);
                    if (studentClassroomRow != null)
                    {
                        var studentClassroom = _classroomRepo.Get(x => x.Id == studentClassroomRow.ClassroomId).FirstOrDefault();
                        if (studentClassroom != null)
                        {
                            attendanceResponse.ClassroomId = studentClassroom.Id;
                            attendanceResponse.ClassroomName = studentClassroom.Name;

                        }
                    }
                    attendanceResponse.CheckedInTime = attendance.CreatedAt;
                }
                else
                {
                    attendanceResponse.Success = false;
                    attendanceResponse.Reason = "Already checked in for today";
                }
            }
            else
            {
                attendanceResponse.Success = false;
                attendanceResponse.Reason = "Student with IC Number " + icNumber + " not found in the database";
            }

            return attendanceResponse;
        }

        public void ClassroomSync()
        {
            string[] array = {"D1.xlsx", "D2.xlsx", "D3.xlsx", "D4.xlsx", "D5.xlsx", "D6.xlsx", "KHAS.xlsx"};
            List<string> list3 = new List<string>(array);
            List<Student> studentsToAdd = new List<Student>();
            var iterator = 1;
            foreach (var fileName in list3)
            {
                //Open the workbook (or create it if it doesn't exist)
                var fi = new FileInfo(@"C:\Users\LENOVO\Downloads\skpp112 - 2021\" + fileName);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var p = new ExcelPackage(fi))
                {
                    var workSheet = p.Workbook.Worksheets[0];
                    Console.WriteLine("Sheet 1 Data");
                    var start = workSheet.Dimension.Start;
                    var end = workSheet.Dimension.End;
                    var iteratorClassroomName = "";
                    Classroom holderClsr = null;
                    for (int row = start.Row; row <= end.Row; row++)
                    {
                        // Row by row...
                        var validStudentRow = false;
                        var validStudentRowWithNoIc = false;
                        var emptyIcColumn = false;
                        for (int col = start.Column; col <= end.Column; col++)
                        {
                            // first column
                            if (col == 2)
                            {
                                string cellValue = workSheet.Cells[row, col].Text;

                                // handling class name
                                if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(cellValue, "murid kelas",
                                    CompareOptions.IgnoreCase) >= 0)
                                {
                                    var classroomName = cellValue.Substring(12);
                                    Console.WriteLine("Its a class name" + classroomName);
                                    iteratorClassroomName = classroomName;
                                    var classroom = _classroomRepo.GetAll().Where(x => x.Name == classroomName);
                                    if (classroom.Any())
                                    {
                                        // classroom existed;
                                        holderClsr = classroom.First();
                                        if (iterator <= 6)
                                        {
                                            holderClsr.Grade = iterator.ToString();
                                        }
                                        else
                                        {
                                            holderClsr.Grade = "KHAS";
                                        }

                                        _classroomRepo.UpdateOnCommit(holderClsr);
                                        _classroomRepo.CommitChanges();
                                    }

                                    else
                                    {
                                        // not yet exists classroom
                                        var freshClassroom = new Classroom();
                                        freshClassroom.Name = iteratorClassroomName;
                                        freshClassroom.CreatedAt = DateTime.Now;
                                        freshClassroom.UpdatedAt = DateTime.Now;
                                        if (iterator <= 6)
                                        {
                                            freshClassroom.Grade = iterator.ToString();
                                        }
                                        else
                                        {
                                            freshClassroom.Grade = "KHAS";
                                        }

                                        freshClassroom = _classroomRepo.InsertOnCommit(freshClassroom);
                                        _classroomRepo.CommitChanges();
                                        holderClsr = freshClassroom;
                                    }
                                }
                                else
                                {
                                    if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(cellValue, "No. KP",
                                        CompareOptions.IgnoreCase) < 0)
                                    {
                                        // its not a column name
                                        // its a valid student row?
                                        // need to handle no ic
                                        if (cellValue.Length > 0)
                                        {
                                            // its a student with ic
                                            // Console.WriteLine(cellValue);
                                            // Console.WriteLine("Its a ic number" + cellValue);
                                            validStudentRow = true;
                                        }
                                        else
                                        {
                                            // empty ic column name
                                            emptyIcColumn = true;
                                        }
                                    }
                                }
                            }

                            if (col == 4)
                            {
                                string cellValue = workSheet.Cells[row, col].Text;
                                if (cellValue.Length > 0 && emptyIcColumn == true)
                                {
                                    validStudentRow = true;
                                    validStudentRowWithNoIc = true;
                                }
                            }
                        }

                        if (validStudentRow)
                        {
                            // string icNum = workSheet.Cells[row, 1].Text;
                            // string birthCertificateNum = workSheet.Cells[row, 2].Text;
                            // string studentName = workSheet.Cells[row, 3].Text;
                            // string dob = workSheet.Cells[row, 4].Text;
                            // string paIc = workSheet.Cells[row, 5].Text;
                            // string paName = workSheet.Cells[row, 6].Text;
                            // string maIc = workSheet.Cells[row, 7].Text;
                            // string maName = workSheet.Cells[row, 8].Text;
                            string icNum = workSheet.Cells[row, 2].Text;
                            string birthCertificateNum = workSheet.Cells[row, 3].Text;
                            string studentName = workSheet.Cells[row, 4].Text;
                            // string dob = workSheet.Cells[row, 4].Text;
                            // string paIc = workSheet.Cells[row, 5].Text;
                            // string paName = workSheet.Cells[row, 6].Text;
                            // string maIc = workSheet.Cells[row, 7].Text;
                            // string maName = workSheet.Cells[row, 8].Text;

                            Student existingStudent = _studentRepo.GetAll()
                                .Where(student1 => EF.Functions.Like(student1.IcNumber, $"%{icNum}%") ||  EF.Functions.Like(student1.Name, $"%{studentName}%")).FirstOrDefault();

                            Console.WriteLine(studentName);
                            if (validStudentRowWithNoIc)
                            {
                                Console.WriteLine("Valid Student Row with no IC");
                            }


                            if (existingStudent == null)
                            {
                                Student student = new Student();
                                student.Name = studentName;
                                student.IcNumber = icNum;
                                student.BirthCertificate = birthCertificateNum;
                                // student.DateOfBirth = DateTime.Parse(dob).Date;
                                // student.FatherIcNumber = paIc;
                                // student.FatherName = paName;
                                // student.MotherIcNumber = maIc;
                                // student.MotherName = maName;
                                // student.ClassroomId = holderClsr.Id;
                                student.CreatedAt = DateTime.Now;
                                student.UpdatedAt = DateTime.Now;
                                _studentRepo.InsertOnCommit(student);
                                _studentRepo.CommitChanges();
                                // // creating classroom for student
                                var studentClassroom = this.CreateStudentClassroom(student,holderClsr.Id);
                                if (studentClassroom != null)
                                {
                                    // result.ClassroomId = studentClassroom.ClassroomId;
                                }
                                // studentsToAdd.Add(student);
                            }
                            else
                            {
                                // create classroom here
                                var studentClassroom = this.CreateStudentClassroom(existingStudent, holderClsr.Id);
                                if (studentClassroom != null)
                                {
                                    // result.ClassroomId = studentClassroom.ClassroomId;

                                }
                            }

                      
                        }
                    }
                }

                iterator += 1;
            }
            // Console.WriteLine(studentsToAdd);

        }

        private Studentclassroom CreateStudentClassroom(Student student,int classRoomId)
        {
            this.UnCurrentPastClassroom(student);
            Studentclassroom studentclassroom = new Studentclassroom();
            studentclassroom.StudentId = student.Id;
            // if (updateObj != null)
            // {
            //     studentclassroom.ClassroomId = updateObj.ClassroomId;
            // }
            // else
            // {
            //     studentclassroom.ClassroomId = student.ClassroomId;
            //
            // }
            studentclassroom.ClassroomId = classRoomId;
            var currentTime = DateTime.Now;
            studentclassroom.CreatedAt = currentTime;
            studentclassroom.UpdatedAt = currentTime;
            int year = DateTime.Now.Year;
            DateTime firstDay = new DateTime(year , 1, 1);
            studentclassroom.EffectiveFrom = firstDay;
            studentclassroom.IsCurrent = true;
            studentclassroom = _studentclassroomRepo.InsertOnCommit(studentclassroom);
            _classroomRepo.CommitChanges();
            return studentclassroom;
        }
        
        private void UnCurrentPastClassroom(Student student)
        {
            var pastClassrooms = _studentclassroomRepo.GetAll().Where(x => x.StudentId == student.Id).ToList();
            foreach (var classroom in pastClassrooms)
            {
                classroom.IsCurrent = false;
                _studentclassroomRepo.UpdateOnCommit(classroom);
                
            }
            _studentclassroomRepo.CommitChanges();
        }


        public void GenerateQRCode()
        {
            var classRooms = _classroomRepo.GetAll().ToList();
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
            Bitmap schoolLogoImage = new Bitmap(@"C:\Users\LENOVO\Documents\repos\attendance-tracker\src\assets\images\skpp112_card.jpeg");
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

                    var classroom = _classroomRepo.Get(x => x.Id == student.ClassroomId).FirstOrDefault();
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
                            Bitmap schoolLogo = new Bitmap(schoolLogoImage,new Size(schoolLogoImage.Width/7,schoolLogoImage.Height/7));
                            graphics.DrawImage(schoolLogo,schoolLogoLocation);
                            
                            // adding qr code
                            Bitmap resizedQrCode = new Bitmap(qrCodeImage,new Size(qrCodeImage.Width/4,qrCodeImage.Height/4));
                            graphics.DrawImage(resizedQrCode,qrCodeLocation);
                            
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
                                graphics.DrawString(classroom.Grade + " " + classroom.Name, arialFont, Brushes.Black,
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
            CreatePDF(bitmapPathList, holderClassroom.Grade + " "+  holderClassroom.Name);

            }
    
        }

        private void CreatePDF(List<string> bitmapPathList, string classroomName)
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
            FileStream fileStream = new FileStream( @"genpdf\" + classroomName + ".pdf", FileMode.CreateNew, FileAccess.ReadWrite);
            doc.Save(fileStream);
            doc.Close();
            
        }

        public void PraSync()
        {
            var path = @"C:\Users\LENOVO\Downloads\skpp112 - 2021\PRA 2.csv"; 
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();
                    
                    string icNum = fields[1];
                    if (icNum.Substring(0,1) == "*")
                    {
                        icNum = icNum.Remove(0,1);
                    }
                    // string birthCertificateNum = fields[3];
                    string studentName = fields[0];
                    // string dob = fields[4];
                    // string paIc = fields[39];
                    // string paName = fields[38];
                    // string maIc = fields[29];
                    // string maName = fields[28];
                    string classroomName = fields[2];
                    string formattedClassroomName = "";
                    if (classroomName == "PRA JASMIN")
                    {
                        formattedClassroomName = "JASMIN";
                    }
                    
                    if (classroomName == "PRA IXORA")
                    {
                        formattedClassroomName = "IXORA";
                    }
                    
                    if (classroomName == "PRA ALAMANDA")
                    {
                        formattedClassroomName = "ALAMANDA";
                    }
                    
                    var classroom = _classroomRepo.GetAll().Where(x => x.Name == formattedClassroomName);
                    Classroom studentClassroom;
                    if (classroom.Any())
                    {
                        // classroom existed;
                        // holderClsr = classroom.First();
                        studentClassroom = classroom.FirstOrDefault();
                    }

                    else
                    {
                        // not yet exists classroom
                        var freshClassroom = new Classroom();
                        freshClassroom.Name = formattedClassroomName;
                        freshClassroom.CreatedAt = DateTime.Now;
                        freshClassroom.UpdatedAt = DateTime.Now;

                        studentClassroom = _classroomRepo.InsertOnCommit(freshClassroom);
                        _classroomRepo.CommitChanges();
                    }
                    
                    
                    Student existingStudent = _studentRepo.GetAll()
                        .Where(student1 => EF.Functions.Like(student1.IcNumber, $"%{icNum}%") ||  EF.Functions.Like(student1.Name, $"%{studentName}%")).FirstOrDefault();

                    if (existingStudent == null)
                    {
                        Console.WriteLine(studentName + icNum  + formattedClassroomName);
                        Student student = new Student();
                        student.Name = studentName;
                        student.IcNumber = icNum;
                        // student.BirthCertificate = birthCertificateNum;
                        // student.DateOfBirth = DateTime.Parse(dob).Date;
                        // student.FatherIcNumber = paIc;
                        // student.FatherName = paName;
                        // student.MotherIcNumber = maIc;
                        // student.MotherName = maName;
                        // student.ClassroomId = studentClassroom.Id;
                        var stdClasr = this.CreateStudentClassroom(student,studentClassroom.Id);
                        if (studentClassroom != null)
                        {
                            // result.ClassroomId = studentClassroom.ClassroomId;
                        }
                        student.CreatedAt = DateTime.Now;
                        student.UpdatedAt = DateTime.Now;
                        _studentRepo.InsertOnCommit(student);
                        _studentRepo.CommitChanges();
                    }
                    else
                    {
                        // update classroom
                        var stdClasr = this.CreateStudentClassroom(existingStudent,studentClassroom.Id);
                        if (studentClassroom != null)
                        {
                            // result.ClassroomId = studentClassroom.ClassroomId;
                        }
                    }
              
                }
            }
        }
        
        private Studentclassroom GetStudentClassroom(Student student)
        {
            var currentTime = DateTime.Now;
            return  _studentclassroomRepo.GetAll().Where(x => x.StudentId == student.Id && x.IsCurrent == true).FirstOrDefault();
        }
        
        public void SeedStudentClassrooms()
        {
            var students = _studentRepo.GetAll().ToList();
            foreach (var student in students)
            {
                Studentclassroom studentclassroom = new Studentclassroom();
                studentclassroom.StudentId = student.Id;
                studentclassroom.ClassroomId = student.ClassroomId;
                var currentTime = DateTime.Now.AddYears(-1);
                studentclassroom.CreatedAt = currentTime;
                studentclassroom.UpdatedAt = currentTime;
                int year = DateTime.Now.AddYears(-1).Year;
                DateTime firstDay = new DateTime(year , 1, 1);
                studentclassroom.EffectiveFrom = firstDay;
                studentclassroom.IsCurrent = true;
                studentclassroom = _studentclassroomRepo.InsertOnCommit(studentclassroom);
                _studentclassroomRepo.CommitChanges();
            }
        }


    }
}