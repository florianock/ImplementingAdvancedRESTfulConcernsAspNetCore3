using CourseLibrary.API.DbContexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace CourseLibrary.API
{
    public class Program
    {

        public static void Main(string[] args)
        {
            // Added logging following this tutorial:
            // https://nblumhardt.com/2019/10/serilog-in-aspnetcore-3/
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
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
