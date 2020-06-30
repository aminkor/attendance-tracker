using System.Collections.Generic;
using AttendanceTracker.Models.Contracts;

namespace AttendanceTracker.Models.IServices
{
    public interface IAttendanceService
    {
        IEnumerable<AttendanceResponse> GetAttendance(string attendanceDate = null);

        AttendanceResponse CreateAttendance(string icNumber);

        void ClassroomSync();
        void GenerateQRCode();
        void PraSync();
    }
}