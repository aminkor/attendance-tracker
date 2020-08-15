using System.Collections.Generic;
using AttendanceTracker.Models.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceTracker.Models.IServices
{
    public interface IStudentService
    {
        IEnumerable<StudentResponse> GetStudents(int classRoomId);
        Student CreateStudent(Student student);
        
        Student UpdateStudent(int studentId, Student student);
        
        void DeleteStudent(int studentId);

        Student GetStudent(int studentId);
    }
}