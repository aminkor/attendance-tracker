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
        private readonly IDataRepository<Studentclassroom> _studentclassroomRepo;

        public StudentService(IDataRepository<Classroom> classroomRepo,
            IDataRepository<Student> studentRepo,
            IDataRepository<Attendance> attendanceRepo,
            IDataRepository<Studentclassroom> studentclassroomRepo)
        {
            _classroomRepo = classroomRepo;
            _studentRepo = studentRepo;
            _attendanceRepo = attendanceRepo;
            _studentclassroomRepo = studentclassroomRepo;
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
                // getting classroom by relationship
                var studentClassroom = this.GetStudentClassroom(student);
                if (studentClassroom != null)
                {
                    result.ClassroomId = studentClassroom.ClassroomId;
                }
                else
                {
                    result.ClassroomId = 0;
                }
            }

            return result;
        }

        private Studentclassroom GetStudentClassroom(Student student)
        {
            var currentTime = DateTime.Now;
            return  _studentclassroomRepo.GetAll().Where(x => x.StudentId == student.Id && x.IsCurrent == true).FirstOrDefault();
        }

        public Student CreateStudent(Student student)
        {
            var insertStudent = new Student();
            insertStudent = student;

            var result = _studentRepo.InsertOnCommit(insertStudent);
            _studentRepo.CommitChanges();

            // creating classroom for student
            var studentClassroom = this.CreateStudentClassroom(student);
            if (studentClassroom != null)
            {
                result.ClassroomId = studentClassroom.ClassroomId;

            }
            _studentRepo.CommitChanges();
            return result;
        }

        private Studentclassroom CreateStudentClassroom(Student student, Student updateObj = null)
        {
            this.UnCurrentPastClassroom(student);
            Studentclassroom studentclassroom = new Studentclassroom();
            studentclassroom.StudentId = student.Id;
            if (updateObj != null)
            {
                studentclassroom.ClassroomId = updateObj.ClassroomId;
            }
            else
            {
                studentclassroom.ClassroomId = student.ClassroomId;

            }
            var currentTime = DateTime.Now;
            studentclassroom.CreatedAt = currentTime;
            studentclassroom.UpdatedAt = currentTime;
            int year = DateTime.Now.Year;
            DateTime firstDay = new DateTime(year , 1, 1);
            studentclassroom.EffectiveFrom = firstDay;
            studentclassroom.IsCurrent = true;
            studentclassroom = _studentclassroomRepo.InsertOnCommit(studentclassroom);
            _studentclassroomRepo.CommitChanges();
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

        public Student UpdateStudent(int studentId, Student student)
        {
            var updateStudent = _studentRepo.Get(x => x.Id == studentId).FirstOrDefault();
            Student result = new Student();
            if (updateStudent != null)
            {
                updateStudent.Name = student.Name;
                updateStudent.IcNumber = student.IcNumber;
                // updateStudent.ClassroomId = student.ClassroomId;
                var studentClassroom = this.CreateStudentClassroom(updateStudent, student);
                if (studentClassroom != null)
                {
                    result.ClassroomId = studentClassroom.ClassroomId;
                }
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