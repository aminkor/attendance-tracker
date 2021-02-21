using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace AttendanceTracker.Models
{
    public partial class AttendanceTracker_DevContext : DbContext
    {
        public AttendanceTracker_DevContext()
        {
        }

        public AttendanceTracker_DevContext(DbContextOptions<AttendanceTracker_DevContext> options)
            : base(options)
        {
        }
        
        public virtual DbSet<Classroom> Classroom { get; set; }
        public virtual DbSet<Student> Student { get; set; }
        public virtual DbSet<Attendance> Attendance { get; set; }
        public virtual DbSet<Studentclassroom> StudentClassroom { get; set; }


        static public IConfigurationRoot Configuration { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                #warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
                Configuration = builder.Build();
                optionsBuilder.UseSqlServer(Configuration.GetConnectionString("TMLinkDB"));

                // if (Environment.GetEnvironmentVariable("SQLAZURECONNSTR_TMLinkDB") != null)
                // {
                //     // get environment variable from app service
                //     optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("SQLAZURECONNSTR_TMLinkDB"));
                //
                // }
                // else
                // {
                //     // revert to appsettings.json
                //     optionsBuilder.UseSqlServer(Configuration.GetConnectionString("TMLinkDB"));
                //
                // }
            }
            
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
        }
        
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);


    }
}