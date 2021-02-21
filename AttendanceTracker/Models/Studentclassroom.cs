using System;

namespace AttendanceTracker.Models
{
    public class Studentclassroom
    {
        public Studentclassroom()
        {
        }
        
        public int Id { get; set; }
        
        public int StudentId
        {
            get;
            set;
        }
        
        public int ClassroomId
        {
            get;
            set;
        }
        
     
      
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public bool? IsCurrent {get; set;}

    }
}