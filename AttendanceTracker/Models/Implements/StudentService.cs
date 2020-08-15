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

namespace AttendanceTracker.Models.Implements
{
    public class StudentService : IStudentService
    {
        private readonly IDataRepository<Classroom> _classroomRepo;
        private readonly IDataRepository<Student> _studentRepo;
        private readonly IDataRepository<Attendance> _attendanceRepo;

        public StudentService(IDataRepository<Classroom> classroomRepo,
            IDataRepository<Student> studentRepo,
            IDataRepository<Attendance> attendanceRepo)
        {
            _classroomRepo = classroomRepo;
            _studentRepo = studentRepo;
            _attendanceRepo = attendanceRepo;
        }

        public IEnumerable<StudentResponse> GetStudents(int classRoomId)
        {
            throw new NotImplementedException();
        }
        
        public Student GetStudent(int studentId)
        {
            var student = _studentRepo.Get(x => x.Id == studentId).FirstOrDefault();
            Student result = new Student();
            if (student != null)
            {
                result = student;
            }

            return result;
        }

        public Student CreateStudent(Student student)
        {
            var insertStudent = new Student();
            insertStudent = student;

            var result = _studentRepo.InsertOnCommit(insertStudent);
            _studentRepo.CommitChanges();
            return result;
        }

        public Student UpdateStudent(int studentId, Student student)
        {
            var updateStudent = _studentRepo.Get(x => x.Id == studentId).FirstOrDefault();
            Student result = new Student();
            if (updateStudent != null)
            {
                updateStudent.Name = student.Name;
                updateStudent.IcNumber = student.IcNumber;
                updateStudent.ClassroomId = student.ClassroomId;
                result = _studentRepo.UpdateOnCommit(updateStudent);
                _studentRepo.CommitChanges();

            }

            return result;
        }

        public void DeleteStudent(int studentId)
        {
            var deleteStudent = _studentRepo.Get(x => x.Id == studentId).FirstOrDefault();
            if (deleteStudent != null)
            {
                _studentRepo.DeletePermanentOnCommit(deleteStudent);
                _studentRepo.CommitChanges();

            }
        }

      
    }
}