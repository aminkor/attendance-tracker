using System;
using System.IO;
using System.Linq;
using AttendanceTracker.Models;
using AttendanceTracker.Models.IServices;
using AttendanceTracker.Models.Repository;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

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
        public AttendanceController(IAttendanceService attendanceService,IDataRepository<Classroom> classroomRepo,
            IDataRepository<Student> studentRepo,
            IDataRepository<Attendance> attendanceRepo)
        {
            _attendanceService = attendanceService;
            _classroomRepo = classroomRepo;
            _studentRepo = studentRepo;
            _attendanceRepo = attendanceRepo;
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
        public IActionResult DownloadAttendance([FromQuery] int classRoomId, [FromQuery] string attendanceDate, [FromQuery] string format)
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
    }
}