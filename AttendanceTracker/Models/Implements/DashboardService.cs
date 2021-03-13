using System;
using System.Collections.Generic;
using System.Linq;
using AttendanceTracker.Models.Contracts;
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

        public List<StudentResponse> StudentsFilter(string? attendanceDate, string? queryType, string? gradeId,
            int? classroomId)
        {
            List<StudentResponse> studentResponses = new List<StudentResponse>();
            
            if (queryType == "allStudent")
            {
                this.GetAllStudents(studentResponses);
            }
            else if (queryType == "totalAttendance")
            {
                this.GetAttendance(studentResponses, attendanceDate, "totalAttendance");
            }
            else if (queryType == "totalOnTime")
            {
                this.GetAttendance(studentResponses, attendanceDate, "totalOnTime");
            }

            else if (queryType == "totalLate")
            {
                this.GetAttendance(studentResponses, attendanceDate, "totalLate");
            }
            else if (queryType == "totalAttendByGrade")
            {
                this.GetGradeAttendance(studentResponses, attendanceDate, queryType, gradeId);
            }
            else if (queryType == "totalAbsenceByGrade")
            {
                this.GetGradeAttendance(studentResponses, attendanceDate, queryType, gradeId);
            }
            else if (queryType == "totalOnTimeByClassroom")
            {
                this.GetClassroomAttendance(studentResponses, attendanceDate, queryType, gradeId, classroomId);
            }
            else if (queryType == "totalLateByClassroom")
            {
                this.GetClassroomAttendance(studentResponses, attendanceDate, queryType, gradeId, classroomId);
            }


            return studentResponses;

        }

        public List<ClassroomPie> ClassroomPieByGrade(string? attendanceDate, string? gradeId)
        {
            List<ClassroomPie> classroomPies = new List<ClassroomPie>();
            var classrooms = _classroomRepo.GetAll().Where(x => x.Grade == gradeId).ToList();
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

            List<Attendance> selectedDateAttendances =
                new List<Attendance>(_attendanceRepo.GetAll().Where(x => x.CreatedAt >= startDay && x.CreatedAt <= endDay));
            foreach (var classroom in classrooms)
            {
                AttendanceSeries attendanceSeries = new AttendanceSeries();
                List<AttendanceSeriesComponent> attendanceSeriesComponents = new List<AttendanceSeriesComponent>();

                string chartName = classroom.Grade + " " + classroom.Name;
                var classroomsStudents = _studentClassroomRepo.GetAll()
                    .Where(x => x.IsCurrent == true && x.ClassroomId == classroom.Id).Select(x => x.StudentId).ToList();
                var gradeAttendances = selectedDateAttendances.Where(x => classroomsStudents.Contains(x.StudentId));
                
                
                // ontime v late

                PunctualSeriesComponent punctualSeriesComponentOnTime = new PunctualSeriesComponent();
                PunctualSeriesComponentExtra punctualSeriesComponentExtraOnTime = new PunctualSeriesComponentExtra();
                PunctualSeriesComponent punctualSeriesComponentLate = new PunctualSeriesComponent();
                PunctualSeriesComponentExtra punctualSeriesComponentExtraLate = new PunctualSeriesComponentExtra();
          

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
                
            

                List<PunctualSeriesComponent> punctualSeries = new List<PunctualSeriesComponent>();
                punctualSeries.Add(punctualSeriesComponentOnTime);
                punctualSeries.Add(punctualSeriesComponentLate);
                // punctualSeries.Add(punctualSeriesComponentAbsence);

                ClassroomPie classroomPie = new ClassroomPie();
                classroomPie.ResultsGraph = punctualSeries;
                classroomPie.Name = chartName;
                classroomPie.NoGraphData = false;
                classroomPie.MajorityGrade = new MajorityGrade();
                classroomPie.ClassroomId = classroom.Id;
                classroomPie.ClassroomName = classroom.Name;
                classroomPies.Add(classroomPie);



            }

            return classroomPies;
        }

        private List<StudentResponse> GetGradeAttendance(List<StudentResponse> studentResponses, string? attendanceDate, string? queryType, string? gradeId)
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

            List<Attendance> selectedDateAttendances =
                new List<Attendance>(_attendanceRepo.GetAll().Where(x => x.CreatedAt >= startDay && x.CreatedAt <= endDay));
            var classroomsByGrade = _classroomRepo.GetAll().Where(x => x.Grade == gradeId).Select(x => x.Id).ToList();
            var classroomsStudents = _studentClassroomRepo.GetAll()
                .Where(x => x.IsCurrent == true && classroomsByGrade.Contains(x.ClassroomId)).Select(x => x.StudentId).ToList();
            var gradeAttendances = selectedDateAttendances.Where(x => classroomsStudents.Contains(x.StudentId));

            List<Student> studentsList = new List<Student>();
            if (queryType == "totalAttendByGrade")
            {
                studentsList = _studentRepo.GetAll()
                    .Where(x => gradeAttendances.Select(attendance => attendance.StudentId).ToList().Contains(x.Id)).ToList();
            }
            else if (queryType == "totalAbsenceByGrade")
            {
                studentsList = _studentRepo.GetAll().Where(student =>
                    classroomsStudents.Contains(student.Id) && !gradeAttendances
                        .Select(attendance => attendance.StudentId).ToList().Contains(student.Id)).ToList();
            }

            foreach (var student in studentsList)
            {
                StudentResponse studentResponse = new StudentResponse();
                // get student obj
                // get classroom obj
                if (student != null)
                {
                    Studentclassroom studentclassroom = this.GetStudentClassroom(student);
                    Classroom classroom = this.GetClassroom(studentclassroom.ClassroomId);
                        
                    studentResponse.StudentId = student.Id;
                    studentResponse.StudentName = student.Name;
                    studentResponse.StudentIcNumber = student.IcNumber;
                    studentResponse.ClassroomId = classroom?.Id ?? 0;
                    studentResponse.ClassroomName = classroom == null ? "" : classroom.Grade + " " + classroom.Name;;
                    
                    studentResponses.Add(studentResponse);
                }
            }

            return studentResponses;
        }
        private List<StudentResponse> GetClassroomAttendance(List<StudentResponse> studentResponses, string? attendanceDate, string? queryType, string? gradeId, int? classroomId)
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

            List<Attendance> selectedDateAttendances =
                new List<Attendance>(_attendanceRepo.GetAll().Where(x => x.CreatedAt >= startDay && x.CreatedAt <= endDay));
            var classroomObj = _classroomRepo.GetAll().Where(x => x.Id == classroomId).FirstOrDefault();
            var classroomsStudents = _studentClassroomRepo.GetAll()
                .Where(x => x.IsCurrent == true && x.ClassroomId == classroomObj.Id).Select(x => x.StudentId).ToList();
            var classroomAttendances = selectedDateAttendances.Where(x => classroomsStudents.Contains(x.StudentId)).ToList();
            TimeSpan lateCutoff = new TimeSpan(7, 31, 0);

            List<Student> studentsList = new List<Student>();
            if (queryType == "totalOnTimeByClassroom")
            {
                var onTime = classroomAttendances.Where(x => x.CreatedAt.Value.TimeOfDay < lateCutoff).Select( x=> x.StudentId).ToList();
                studentsList = _studentRepo.GetAll().Where(x => onTime.Contains(x.Id)).ToList();
            }
            else if (queryType == "totalLateByClassroom")
            {
                var late = classroomAttendances.Where(x => x.CreatedAt.Value.TimeOfDay >= lateCutoff).Select( x=> x.StudentId).ToList();;
                studentsList = _studentRepo.GetAll().Where(x => late.Contains(x.Id)).ToList();

            }

            foreach (var student in studentsList)
            {
                StudentResponse studentResponse = new StudentResponse();
                // get student obj
                // get classroom obj
                if (student != null)
                {
                    Studentclassroom studentclassroom = this.GetStudentClassroom(student);
                    Classroom classroom = this.GetClassroom(studentclassroom.ClassroomId);
                        
                    studentResponse.StudentId = student.Id;
                    studentResponse.StudentName = student.Name;
                    studentResponse.StudentIcNumber = student.IcNumber;
                    studentResponse.ClassroomId = classroom?.Id ?? 0;
                    studentResponse.ClassroomName = classroom == null ? "" : classroom.Grade + " " + classroom.Name;;
                    
                    studentResponses.Add(studentResponse);
                }
            }

            return studentResponses;
        }

        private void GetAttendance(List<StudentResponse> studentResponses, string attendanceDate, string queryType)
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
            List<Attendance> selectedDateAttendances =
                new List<Attendance>(_attendanceRepo.GetAll().Where(x => x.CreatedAt >= startDay && x.CreatedAt <= endDay));
            
            TimeSpan lateCutoff = new TimeSpan(7, 31, 0);

            if (queryType == "allAttendance")
            {
                selectedDateAttendances = selectedDateAttendances.ToList();
            }
            else if (queryType == "totalOnTime")
            {
                selectedDateAttendances = selectedDateAttendances.Where(x => x.CreatedAt.Value.TimeOfDay < lateCutoff).ToList();
            }
            else if (queryType == "totalLate")
            {
                selectedDateAttendances = selectedDateAttendances.Where(x => x.CreatedAt.Value.TimeOfDay >= lateCutoff)
                    .ToList();

            }
            
            foreach (var attendance in selectedDateAttendances)
            {
                StudentResponse studentResponse = new StudentResponse();
                // get student obj
                Student student = this.GetStudent(attendance.StudentId);
                // get classroom obj
                if (student != null)
                {
                    Studentclassroom studentclassroom = this.GetStudentClassroom(student);
                    Classroom classroom = this.GetClassroom(studentclassroom.ClassroomId);
                        
                    studentResponse.StudentId = student.Id;
                    studentResponse.StudentName = student.Name;
                    studentResponse.StudentIcNumber = student.IcNumber;
                    studentResponse.ClassroomId = classroom?.Id ?? 0;
                    studentResponse.ClassroomName = classroom == null ? "" : classroom.Grade + " " + classroom.Name;;
                    
                    studentResponses.Add(studentResponse);
                }
                
             
            }
        }
        
        private Studentclassroom GetStudentClassroom(Student student)
        {
            var currentTime = DateTime.Now;
            return  _studentClassroomRepo.GetAll().Where(x => x.StudentId == student.Id && x.IsCurrent == true).FirstOrDefault();
        }

        private void GetAllStudents(List<StudentResponse> studentResponses)
        {
            var allStudent = _studentClassroomRepo.GetAll().Where(x => x.IsCurrent == true).ToList();

            foreach (var studentClassroom in allStudent)
            {
                StudentResponse studentResponse = new StudentResponse();
                // get student obj
                Student student = this.GetStudent(studentClassroom.StudentId);
                // get classroom obj
                Classroom classroom = this.GetClassroom(studentClassroom.ClassroomId);

                studentResponse.StudentId = student.Id;
                studentResponse.StudentName = student.Name;
                studentResponse.StudentIcNumber = student.IcNumber;
                studentResponse.ClassroomId = classroom?.Id ?? 0;
                studentResponse.ClassroomName = classroom == null ? "" : classroom.Grade + " " + classroom.Name;;
                    
                studentResponses.Add(studentResponse);

            }
        }


        private Classroom GetClassroom(int studentClassroomClassroomId)
        {
            return _classroomRepo.GetAll().Where(x => x.Id == studentClassroomClassroomId).FirstOrDefault();
        }

        private Student GetStudent(int studentClassroomStudentId)
        {
            return _studentRepo.GetAll().Where(x => x.Id == studentClassroomStudentId).FirstOrDefault();
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

    public class MajorityGrade
    {
    }

    public class ClassroomPie
    {
        public List<PunctualSeriesComponent> ResultsGraph { get; set; }
        public string Name { get; set; }
        public bool NoGraphData { get; set; }
        
        public MajorityGrade MajorityGrade { get; set; }
        
        public int ClassroomId { get; set; }
        public string ClassroomName { get; set; }

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