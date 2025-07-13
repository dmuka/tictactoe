using Application.Abstractions;
using Domain.Aggregates.Game;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Infrastructure.Random;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DI
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services
            .AddDatabase()
            .AddRandomProvider();
            
        return services;
    }
    
    private static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            var dbPath = Path.Join(path, "tictactoe.db");
            
            options.UseSqlite($"Data Source={dbPath}");
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IGameRepository, GameRepository>();

        return services;
    }

    private static IServiceCollection AddRandomProvider(this IServiceCollection services)
    {
        services.AddSingleton<IRandomProvider, RandomProvider>();

        return services;
    }
}