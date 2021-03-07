using System.Collections.Generic;
using AttendanceTracker.Models.Contracts;
using AttendanceTracker.Models.Repository;

namespace AttendanceTracker.Models.IServices
{
    public interface IDashboardService
    {
        DashboardResponse GetDashboard(string attendanceDate);
    }
}