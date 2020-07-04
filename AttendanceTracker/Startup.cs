using System.Collections.Generic;
using System.Globalization;
using AttendanceTracker.Models;
using AttendanceTracker.Models.Implements;
using AttendanceTracker.Models.IServices;
using AttendanceTracker.Models.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AttendanceTracker
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddDbContext<AttendanceTracker_DevContext>(opts => opts.UseMySql(Configuration.GetConnectionString("AttendanceTrackerDB")));
            services.AddTransient(typeof(IDataRepository<>), typeof(DataRepository<>));
            services.AddScoped<IAttendanceService, AttendanceService>();
            services.AddScoped<IClassroomService, ClassroomService>();

            services.Configure<RequestLocalizationOptions>(
                options =>
                {
                    var supportedCultures = new List<CultureInfo>
                    {
                        new CultureInfo("en-US"),
                        new CultureInfo("de-CH"),
                        new CultureInfo("fr-CH"),
                        new CultureInfo("it-CH")
                    };

                    options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");
                    options.SupportedCultures = supportedCultures;
                    options.SupportedUICultures = supportedCultures;
                });
            services.AddControllers().AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllHeaders",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });
            services.AddRouting();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            
            var serviceProvider = services.BuildServiceProvider();
            var service = serviceProvider.GetService<IAttendanceService>();

            // service.ClassroomSync();
            // service.GenerateQRCode();
            // service.PraSync();
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {


            if (env.IsDevelopment()) 
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseHttpsRedirection();

         

            app.UseRouting();

            // app.UseAuthentication();
            //
            // app.UseAuthorization();
            
            app.UseCors("AllowAllHeaders");

            app.UseRequestLocalization();
            

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            
           

        }


    }
}