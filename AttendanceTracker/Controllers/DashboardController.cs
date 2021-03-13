using System;
using System.Collections.Generic;
using AttendanceTracker.Models;
using AttendanceTracker.Models.Contracts;
using AttendanceTracker.Models.IServices;
using AttendanceTracker.Models.Repository;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceTracker.Controllers
{
    [Route("api/Dashboard")]
    [ApiController]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // GET
        [HttpGet]
        public IActionResult Index([FromQuery] string? attendanceDate)
        {
            DashboardResponse response;
            try
            {
                response = _dashboardService.GetDashboard(attendanceDate);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return Ok(response);
        }
        
        // GET Students Filer
        [HttpGet("StudentsFilter")]
        public IActionResult StudentsFilter([FromQuery] string? attendanceDate, [FromQuery] string? queryType, [FromQuery] string? gradeId, [FromQuery] int? classroomId)
        {
            List<StudentResponse> response;
            try
            {
                response = _dashboardService.StudentsFilter(attendanceDate, queryType, gradeId, classroomId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return Ok(response);
        }
    }
}