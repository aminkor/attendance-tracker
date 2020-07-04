using System;

namespace AttendanceTracker.Models.Contracts
{
    public class AttendanceResponse
    {
        public int AttendanceId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentIcNumber { get; set; }
        public int ClassroomId { get; set; }
        public string ClassroomName { get; set; }
        public DateTime? CheckedInTime { get; set; }
        
        public string Status { get; set; }

        public bool Success { get; set; }
        public string Reason { get; set; }
    }
}