using System;

namespace AttendanceTracker.Models.Contracts
{
    public class StudentResponse
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentIcNumber { get; set; }
        public int ClassroomId { get; set; }
        public string ClassroomName { get; set; }
    }
}