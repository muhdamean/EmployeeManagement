using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                //configuration for nlog file logging
                .ConfigureLogging((hostingContext, loggingBuilder) =>
                {
                    loggingBuilder.Configure(options =>
                    {
                        options.ActivityTrackingOptions = ActivityTrackingOptions.SpanId
                                                            | ActivityTrackingOptions.TraceId
                                                            | ActivityTrackingOptions.ParentId;
                    });
                    loggingBuilder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    loggingBuilder.AddConsole();
                    loggingBuilder.AddDebug();
                    loggingBuilder.AddEventSourceLogger();
                    //NLog added as one of the logging provider
                    loggingBuilder.AddNLog();
                })
                // default configuration here
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
