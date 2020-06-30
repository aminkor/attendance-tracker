using System;

namespace AttendanceTracker.Models
{
    public partial class Student
    {
        public Student()
        {
        }
        
        public int Id { get; set; }
        public string IcNumber { get; set; }
        public string BirthCertificate { get; set; }
        public string Name { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string FatherIcNumber { get; set; }
        public string FatherName { get; set; }
        public string MotherIcNumber { get; set; }
        public string MotherName { get; set; }
        public int ClassroomId { get; set; }

        public DateTime CreatedAt;
        public DateTime UpdatedAt;
        public Classroom Classroom { get; set; }
    }
}