using Application;
using Infrastructure;

namespace API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

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