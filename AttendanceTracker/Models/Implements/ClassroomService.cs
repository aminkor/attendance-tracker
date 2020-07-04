using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using AttendanceTracker.Models.Contracts;
using AttendanceTracker.Models.IServices;
using AttendanceTracker.Models.Repository;
namespace AttendanceTracker.Models.Implements
{
    public class ClassroomService : IClassroomService
    
    {
        private readonly IDataRepository<Classroom> _classroomRepo;
        private readonly IDataRepository<Student> _studentRepo;
        private readonly IDataRepository<Attendance> _attendanceRepo;

        public ClassroomService(IDataRepository<Classroom> classroomRepo,
            IDataRepository<Student> studentRepo,
            IDataRepository<Attendance> attendanceRepo)
        {
            _classroomRepo = classroomRepo;
            _studentRepo = studentRepo;
            _attendanceRepo = attendanceRepo;
        }
        public IEnumerable<ClassroomResponse> GetClassrooms()
        {
            IEnumerable<ClassroomResponse> classroomResponses = new ClassroomResponse []{};
            var classrooms = _classroomRepo.GetAll();
            foreach (var classroom in classrooms)
            {
                ClassroomResponse classroomResponse = new ClassroomResponse();
                classroomResponse.ClassroomId = classroom.Id;
                classroomResponse.ClassroomName = classroom.Grade + " " + classroom.Name;
                classroomResponses = classroomResponses.Append(classroomResponse);
            }
            
            return classroomResponses;
        }

    }
    
}