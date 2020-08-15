using System;
using System.IO;
using System.Linq;
using AttendanceTracker.Models;
using AttendanceTracker.Models.IServices;
using AttendanceTracker.Models.Repository;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using TMLink_BackEnd.Models.Contracts;

namespace StudentController.Controllers
{
    [Route("api/Student")]
    [ApiController]
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IDataRepository<Classroom> _classroomRepo;
        private readonly IDataRepository<Student> _studentRepo;
        private readonly IDataRepository<Attendance> _attendanceRepo;
        public StudentController(IStudentService studentService,IDataRepository<Classroom> classroomRepo,
            IDataRepository<Student> studentRepo,
            IDataRepository<Attendance> attendanceRepo)
        {
            _studentService = studentService;
            _classroomRepo = classroomRepo;
            _studentRepo = studentRepo;
            _attendanceRepo = attendanceRepo;
        }

        // GET
        [HttpGet("ByClassroom/{studentId}")]
        public IActionResult IndexByClassRoom([FromQuery] int classRoomId)
        {
            var students = _studentService.GetStudents(classRoomId);
            return Ok(students);
        }
        
        // GET
        [HttpGet ("{studentId}")]
        public IActionResult Get( int studentId)
        {
            var student = _studentService.GetStudent(studentId);
            return Ok(student);
        }
        
        // POST
        [HttpPost]
        public IActionResult Create([FromBody] Student studentBody)
        {
            var student = _studentService.CreateStudent(studentBody);
            return Ok(student);
        }
        
        // PUT
        [HttpPut("{studentId}")]
        public IActionResult Update(int studentId, [FromBody] Student studentBody)
        {
            var student = _studentService.UpdateStudent(studentId, studentBody);
            return Ok(student);
        }
        
        // DELETE
        [HttpDelete("{studentId}")]
        public IActionResult Delete( int studentId)
        {
            try
            {
                _studentService.DeleteStudent(studentId);
                return Ok();
            }
            catch (Exception ex)
            {

                var errorResponse = new ErrorsResponse();
                var errorMessage = new ErrorMessage()
                {
                    Code = 400,
                    Message = ex.Message
                };
                errorResponse.Add(errorMessage);
                return new BadRequestObjectResult(errorResponse);
            }
         
        }

    }
}