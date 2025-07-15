using Application;
using Infrastructure;
using Serilog;

namespace API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((context, loggerConfig) => 
            loggerConfig.ReadFrom.Configuration(context.Configuration));

        builder.Services
            .AddApplication()
            .AddInfrastructure()
            .AddPresentation();

        var app = builder.Build();

        await app.InitializeDatabaseAsync();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UsePresentation();

        await app.RunAsync();
    }
}