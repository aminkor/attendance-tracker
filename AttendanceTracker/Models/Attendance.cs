using System;

namespace AttendanceTracker.Models
{
    public class Attendance
    {
        public Attendance()
        {
        }
        
        public int Id { get; set; }
        public int StudentId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    
}