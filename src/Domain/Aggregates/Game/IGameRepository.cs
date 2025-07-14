namespace Domain.Aggregates.Game;

/// <summary>
/// Interface for game repository operations.
/// </summary>
public interface IGameRepository
{
    /// <summary>
    /// Retrieves a game by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the game.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>The game if found; otherwise, null.</returns>
    Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all games.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>An array of all games.</returns>
    Task<Game[]> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new game to the repository.
    /// </summary>
    /// <param name="game">The game to add.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    Task AddAsync(Game game, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing game in the repository.
    /// </summary>
    /// <param name="game">The game to update.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    Task UpdateAsync(Game game, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if a game exists by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the game.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>True if the game exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
}