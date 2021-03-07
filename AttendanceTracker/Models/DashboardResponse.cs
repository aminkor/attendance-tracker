using System.Collections.Generic;
using AttendanceTracker.Models.Implements;

namespace AttendanceTracker.Models.Repository
{
    public class DashboardResponse
    {
        public int StudentsCount { get; set; }
        public int AttendancesCount { get; set; }
        public int AttendancesOnTimeCount { get; set; }
        public int AttendancesLateCount { get; set; }
        public List<AttendanceSeries> GraphAttendancePercentages { get; set; }
        public List<List<PunctualSeriesComponent>> PunctualSeriesForGrades { get; set; }

        public DashboardResponse()
        {
            
        }
    }
}