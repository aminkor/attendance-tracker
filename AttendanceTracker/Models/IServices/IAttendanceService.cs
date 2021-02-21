using System.Collections.Generic;
using AttendanceTracker.Models.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceTracker.Models.IServices
{
    public interface IAttendanceService
    {
        IEnumerable<AttendanceResponse> GetAttendance(string attendanceDate = null, int classRoomId = 0);

        AttendanceResponse CreateAttendance(string icNumber);

        void ClassroomSync();
        void GenerateQRCode();
        void PraSync();
        
        void SeedStudentClassrooms();

    }
}