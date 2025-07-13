namespace Domain.Aggregates.Game;

public interface IGameRepository
{
    Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Game[]> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(Game game, CancellationToken cancellationToken);
    Task UpdateAsync(Game game, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
}