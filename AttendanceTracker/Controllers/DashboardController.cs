using System;
using AttendanceTracker.Models;
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
        public IActionResult Index([FromQuery]string? attendanceDate)
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
    }
}