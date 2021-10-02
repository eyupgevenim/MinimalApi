using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

const string AppName = "MinimalApi";

var Configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).AddEnvironmentVariables()
        .Build();

Log.Logger = new LoggerConfiguration().MinimumLevel.Verbose()
        .Enrich.WithProperty("ApplicationContext", AppName).Enrich.FromLogContext()
        .WriteTo.File("Logs/Log_.txt", rollingInterval: RollingInterval.Day)
        .ReadFrom.Configuration(Configuration)
        .CreateLogger();

try
{
    Log.Information("Configuring web host ({ApplicationContext})...", AppName);

    var host = Host
        .CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(x => x.AddConfiguration(Configuration))
        .UseContentRoot(Directory.GetCurrentDirectory())
        .ConfigureLogging(x => x.AddConsole().SetMinimumLevel(LogLevel.Information))
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.ConfigureServices(services =>
            {
                services
                .Configure<HostOptions>(option => { option.ShutdownTimeout = TimeSpan.FromSeconds(60); })
                .Configure<Appsettings>(Configuration)
                .Configure<Appsettings.MySettingsOptions>(Configuration.GetSection(nameof(Appsettings.MySettings)));

                services.AddScoped<IDemo, Demo>();
                    
            })
            .Configure((hostingContext, app) =>
            {
                var serviceProvider = app.ApplicationServices;
                if (hostingContext.HostingEnvironment.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/", async context =>
                    {
                        await context.Response.BodyWriter.WriteAsync(Source(new { key ="value..." }));
                    });

                    endpoints.MapGet("/hello", async context =>
                    {
                        await context.Response.BodyWriter.WriteAsync(Source("get hello page"));
                    });

                    endpoints.MapPost("/hello", async context =>
                    {
                        await context.Response.BodyWriter.WriteAsync(Source("post hello page"));
                    });

                    endpoints.MapGet("/demo", async context =>
                    {
                        using (var scope = serviceProvider.CreateScope())
                        {
                            var demo = scope.ServiceProvider.GetRequiredService<IDemo>();
                            var result = demo.Run("...testting serice....");
                            await context.Response.BodyWriter.WriteAsync(Source(result));
                        }
                    });

                    endpoints.MapGet("/MySettings", async context =>
                    {
                        var mySettings = serviceProvider.GetRequiredService<IOptions<Appsettings.MySettingsOptions>>().Value;
                        await context.Response.BodyWriter.WriteAsync(Source(mySettings));
                    });
                });
            });
        })
        .Build();

    Log.Information("Starting web host ({ApplicationContext})...", AppName);

    await host.RunAsync();

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", AppName);
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

ReadOnlyMemory<byte> Source(object obj)
{
    string jsonString = JsonSerializer.Serialize(obj);
    return Encoding.ASCII.GetBytes(jsonString).AsMemory();
}

interface IDemo
{
    string Run(string value);
}

class Demo : IDemo
{
    public string Run(string value) => $"Running Run method - value :{value}";
}

class Appsettings
{
    public MySettingsOptions MySettings { get; set; }

    public class MySettingsOptions
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}

