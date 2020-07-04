using System.Collections.Generic;
using AttendanceTracker.Models.Contracts;

namespace AttendanceTracker.Models.IServices
{
    public interface IClassroomService
    {
        IEnumerable<ClassroomResponse> GetClassrooms();
    }
}