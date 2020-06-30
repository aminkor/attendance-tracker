using System;

namespace AttendanceTracker.Models
{
    public class Classroom
    {
        public Classroom()
        {
        }
        
        public int Id { get; set; }
        public string Name
        {
            get;
            set;
        }
        
        public string Grade { get; set; }


        public DateTime CreatedAt;
        public DateTime UpdatedAt;
    }
}