using System;
using System.Collections.Generic;
using System.Linq;
using AttendanceTracker.Models.IServices;
using AttendanceTracker.Models.Repository;

namespace AttendanceTracker.Models.Implements
{
    public class DashboardService : IDashboardService
    {        
        private readonly IDataRepository<Student> _studentRepo;
        private readonly IDataRepository<Attendance> _attendanceRepo;
        private readonly IDataRepository<Classroom> _classroomRepo;
        private readonly IDataRepository<Studentclassroom> _studentClassroomRepo;

        public DashboardService(
            IDataRepository<Student> studentRepo,
            IDataRepository<Attendance> attendanceRepo,
            IDataRepository<Classroom> classroomRepo, IDataRepository<Studentclassroom> studentClassroomRepo)
        {
            _studentRepo = studentRepo;
            _attendanceRepo = attendanceRepo;
            _classroomRepo = classroomRepo;
            _studentClassroomRepo = studentClassroomRepo;
        }

        public DashboardResponse GetDashboard(string attendanceDate)
        {
            DateTime selectedDate;
            if (attendanceDate == null)
            {
                selectedDate = DateTime.Today;
            }
            else
            { 
                selectedDate = DateTime.Parse(attendanceDate);

            }
            
            var startDay = selectedDate.Date;
            var endDay = selectedDate.AddDays(1).AddTicks(-1);
            var selectedDateAttendances =
                _attendanceRepo.GetAll().Where(x => x.CreatedAt >= startDay && x.CreatedAt <= endDay);
            
            TimeSpan lateCutoff = new TimeSpan(7, 31, 0);

            // var graphAttendancePercentages = this.BuildAttendancePercentagesGraph(selectedDateAttendances);
            var dashboardStatistics = this.BuildAttendancePercentagesGraph(selectedDateAttendances);

            var totalStudentsByCurrent = _studentClassroomRepo.GetAll().Count(x => x.IsCurrent == true);
            DashboardResponse dashboardResponse = new DashboardResponse
            {
                StudentsCount = totalStudentsByCurrent,
                AttendancesCount = selectedDateAttendances.Count(),
                AttendancesOnTimeCount = selectedDateAttendances.Count(x => x.CreatedAt.Value.TimeOfDay < lateCutoff),
                AttendancesLateCount = selectedDateAttendances.Count(x => x.CreatedAt.Value.TimeOfDay >= lateCutoff),
                GraphAttendancePercentages = dashboardStatistics.AttendanceSeries,
                PunctualSeriesForGrades = dashboardStatistics.PunctualSeriesForGrades
            };

            
            return dashboardResponse;
        }

        private DashboardStatistics BuildAttendancePercentagesGraph(IQueryable<Attendance> selectedDateAttendances)
        {
            List<string> grades = new List<string> {"1", "2", "3", "4", "5", "6", "KHAS", "PRA"};
            List<AttendanceSeries> attendanceSeriesList = new List<AttendanceSeries>();
            List<List<PunctualSeriesComponent>> punctualSeriesForGrades = new List<List<PunctualSeriesComponent>>();
            foreach (var grade in grades)
            {
                AttendanceSeries attendanceSeries = new AttendanceSeries();
                List<AttendanceSeriesComponent> attendanceSeriesComponents = new List<AttendanceSeriesComponent>();

                string chartName = this.GenChartName(grade);
                var classroomsByGrade = _classroomRepo.GetAll().Where(x => x.Grade == grade).Select(x => x.Id).ToList();
                var classroomsStudents = _studentClassroomRepo.GetAll()
                    .Where(x => x.IsCurrent == true && classroomsByGrade.Contains(x.ClassroomId)).Select(x => x.StudentId).ToList();
                var gradeAttendances = selectedDateAttendances.Where(x => classroomsStudents.Contains(x.StudentId));
                
                
                AttendanceSeriesComponent attendanceSeriesComponentAttend = new AttendanceSeriesComponent();
                attendanceSeriesComponentAttend.Name = "Attend";
                attendanceSeriesComponentAttend.Value = gradeAttendances.Count();
                
                AttendanceSeriesComponent attendanceSeriesComponentAbsence = new AttendanceSeriesComponent();
                attendanceSeriesComponentAbsence.Name = "Absence";
                attendanceSeriesComponentAbsence.Value = (classroomsStudents.Count - gradeAttendances.Count());
                
                attendanceSeriesComponents.Add(attendanceSeriesComponentAttend);
                attendanceSeriesComponents.Add(attendanceSeriesComponentAbsence);

                attendanceSeries.Name = chartName;
                attendanceSeries.Series = attendanceSeriesComponents;
                
                attendanceSeriesList.Add(attendanceSeries);
                
                // ontime v late

                PunctualSeriesComponent punctualSeriesComponentOnTime = new PunctualSeriesComponent();
                PunctualSeriesComponentExtra punctualSeriesComponentExtraOnTime = new PunctualSeriesComponentExtra();
                PunctualSeriesComponent punctualSeriesComponentLate = new PunctualSeriesComponent();
                PunctualSeriesComponentExtra punctualSeriesComponentExtraLate = new PunctualSeriesComponentExtra();
                PunctualSeriesComponent punctualSeriesComponentAbsence = new PunctualSeriesComponent();
                PunctualSeriesComponentExtra punctualSeriesComponentExtraAbsence = new PunctualSeriesComponentExtra();

                TimeSpan lateCutoff = new TimeSpan(7, 31, 0);

                punctualSeriesComponentOnTime.Name = "On Time";
                punctualSeriesComponentOnTime.Value =
                    gradeAttendances.Count(x => x.CreatedAt.Value.TimeOfDay < lateCutoff);

                punctualSeriesComponentExtraOnTime.Color = "rgb(0, 177, 169)";
                punctualSeriesComponentExtraOnTime.Percentage =
                    (double) punctualSeriesComponentOnTime.Value / gradeAttendances.Count() * 100;

                punctualSeriesComponentOnTime.Extra = punctualSeriesComponentExtraOnTime;
                
                punctualSeriesComponentLate.Name = "Late";
                punctualSeriesComponentLate.Value =
                    gradeAttendances.Count(x => x.CreatedAt.Value.TimeOfDay >= lateCutoff);

                punctualSeriesComponentExtraLate.Color = "rgba(104, 70, 139, 0.9)";
                punctualSeriesComponentExtraLate.Percentage =
                    (double) punctualSeriesComponentLate.Value / gradeAttendances.Count() * 100;

                punctualSeriesComponentLate.Extra = punctualSeriesComponentExtraLate;
                
                // absence
                
                // punctualSeriesComponentAbsence.Name = "Absence";
                // punctualSeriesComponentAbsence.Value =
                //     gradeAttendances.Count() - punctualSeriesComponentOnTime.Value - punctualSeriesComponentLate.Value;
                //
                // punctualSeriesComponentExtraAbsence.Color = "#ccc";
                // punctualSeriesComponentExtraAbsence.Percentage =
                //     (double) punctualSeriesComponentAbsence.Value / classroomsStudents.Count() * 100;
                //
                // punctualSeriesComponentAbsence.Extra = punctualSeriesComponentExtraAbsence;

                List<PunctualSeriesComponent> punctualSeries = new List<PunctualSeriesComponent>();
                punctualSeries.Add(punctualSeriesComponentOnTime);
                punctualSeries.Add(punctualSeriesComponentLate);
                // punctualSeries.Add(punctualSeriesComponentAbsence);

                punctualSeriesForGrades.Add(punctualSeries);



            }

            return new DashboardStatistics
            {
                AttendanceSeries = attendanceSeriesList,
                PunctualSeriesForGrades = punctualSeriesForGrades
            };
        }

        private string GenChartName(string grade)
        {
            if (grade == "KHAS" || grade == "PRA")
            {
                return grade;
            }
            else
            {
                return "TAHUN " + grade;
            }
        }
    }

    public class DashboardStatistics
    {
        public List<AttendanceSeries> AttendanceSeries { get; set; }
        public List<List<PunctualSeriesComponent>> PunctualSeriesForGrades { get; set; }
    }

    public class AttendanceSeries
    {
        public string Name { get; set; }
        public List<AttendanceSeriesComponent> Series { get; set; }
    }

    public class AttendanceSeriesComponent
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    public class PunctualSeriesComponent
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public PunctualSeriesComponentExtra Extra {get; set; }
    }

    public class PunctualSeriesComponentExtra
    {
        public string Color { get; set; }
        public double Percentage { get; set; }
    }
}