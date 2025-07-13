using System.Text.Json;
using Domain.Aggregates.Game;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Game> Games { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(g => g.Id);
            
            entity.Property(g => g.Board)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<List<string>>>(v, (JsonSerializerOptions)null!) ?? new List<List<string>>());
            
            entity.Property(g => g.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => Enum.Parse<GameStatus>(v));
            
            entity.Property(g => g.Version)
                .IsConcurrencyToken();
        });
    }
}