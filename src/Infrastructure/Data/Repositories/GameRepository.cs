using Domain.Aggregates.Game;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class GameRepository(AppDbContext context) : IGameRepository
{
    public async Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Games
            .AsNoTracking()
            .FirstOrDefaultAsync(game => game.Id == id, cancellationToken);
    }
    
    public async Task<Game[]> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.Games
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);
    }

    public async Task AddAsync(Game game, CancellationToken cancellationToken)
    {
        await context.Games.AddAsync(game, cancellationToken);
    }

    public async Task UpdateAsync(Game game, CancellationToken cancellationToken)
    {
        var existingGame = await context.Games
            .FirstOrDefaultAsync(gameDb => gameDb.Id == game.Id, cancellationToken);

        if (existingGame is null) throw new ConcurrencyException();
    
        context.Entry(existingGame).CurrentValues.SetValues(game);
        context.Entry(existingGame).Property(g => g.Board).IsModified = true;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Games
            .AnyAsync(g => g.Id == id, cancellationToken);
    }
}