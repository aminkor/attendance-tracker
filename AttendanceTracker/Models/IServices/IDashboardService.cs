using System.Collections.Generic;
using AttendanceTracker.Models.Contracts;
using AttendanceTracker.Models.Repository;

namespace AttendanceTracker.Models.IServices
{
    public interface IDashboardService
    {
        DashboardResponse GetDashboard(string attendanceDate);
        List<StudentResponse> StudentsFilter(string? attendanceDate, string? queryType, string? gradeId,
            int? classroomId);
    }
}