using CourseLibrary.API.DbContexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Data;
using Serilog;
using Serilog.Events;

namespace CourseLibrary.API
{
    public class Program
    {

        public static void Main(string[] args)
        {
            // Added logging following these sources:
            // https://nblumhardt.com/2019/10/serilog-in-aspnetcore-3/
            // https://github.com/serilog/serilog-aspnetcore
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                // .WriteTo.File("bla.txt")
                // .WriteTo.Seq(
                //     Environment.GetEnvironmentVariable("SEQ_URL") ?? "http://localhost:5341")
                .CreateLogger();

            try
            {
                var host = CreateHostBuilder(args).Build();

                // migrate the database.  Best practice = in Main, using service scope
                using (var scope = host.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetService<CourseLibraryContext>();

                    if (context == null) throw new DataException($"Missing Database context; {nameof(CourseLibraryContext)}");
                    
                    // for demo purposes, delete the database & migrate on startup so 
                    // we can start with a clean slate
                    context.Database.EnsureDeleted();
                    context.Database.Migrate();
                }

                // run the web app
                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "An error occurred while migrating the database.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
        
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
