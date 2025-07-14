using Application.DTOs;
using Domain.Aggregates.Game;
using MediatR;

namespace Application.UseCases.GetAllGames;

/// <summary>
/// Handles the retrieval of all games.
/// </summary>
public class GetAllGamesQueryHandler(IGameRepository gameRepository) : IRequestHandler<GetAllGamesQuery, GameDto[]>
{
    /// <summary>
    /// Handles the retrieval of all games.
    /// </summary>
    /// <param name="query">The query to retrieve all games.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an array of game DTOs.</returns>
    public async Task<GameDto[]> Handle(GetAllGamesQuery query, CancellationToken cancellationToken)
    {
        var games = await gameRepository.GetAllAsync(cancellationToken);

        return games.Select(game => game.ToDto()).ToArray();
    }
}