using System;
using AttendanceTracker.Models;
using AttendanceTracker.Models.IServices;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceTracker.Controllers
{
    [Route("api/Attendance")]
    [ApiController]
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
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
    }
}