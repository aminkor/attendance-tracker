using System;
using AttendanceTracker.Models;
using AttendanceTracker.Models.IServices;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceTracker.Controllers
{
    [Route("api/Classroom")]
    [ApiController]
    public class ClassroomController : Controller
    {
        private readonly IClassroomService _classroomService;

        public ClassroomController(IClassroomService classroomService)
        {
            _classroomService = classroomService;
        }

        // GET
        [HttpGet]
        public IActionResult Index()
        {
            var attendances = _classroomService.GetClassrooms();
            return Ok(attendances);
        }
        
        // POST
        [HttpPost]
        public IActionResult Create()
        {
            return Ok();
        }
    }
}