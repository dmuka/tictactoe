using System.Text.Json;
using Domain.Aggregates.Game;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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
            
            entity.Property(e => e.Board)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<List<string>>>(v, (JsonSerializerOptions)null!) ?? new List<List<string>>(),
                    new ValueComparer<List<List<string>>>(
                        (first, second) => 
                            first == null && second == null || 
                            (first != null && second != null && first.SequenceEqual(second, new ListComparer())),
                        board => board.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        board => board.ToList()));
            
            entity.Property(g => g.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => Enum.Parse<GameStatus>(v));
            
            entity.Property(g => g.Version)
                .IsConcurrencyToken();
        });
    }

    private class ListComparer : IEqualityComparer<List<string>>
    {
        public bool Equals(List<string>? first, List<string>? second)
        {
            if (ReferenceEquals(first, second)) return true;
            if (first is null || second is null) return false;
            
            return first.SequenceEqual(second);
        }

        public int GetHashCode(List<string> obj)
        {
            return obj.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode()));
        }
    }
}