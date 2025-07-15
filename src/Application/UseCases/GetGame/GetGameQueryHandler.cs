using Application.DTOs;
using Domain.Aggregates.Game;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.GetGame;

/// <summary>
/// Handles the retrieval of a game by its unique identifier.
/// </summary>
public class GetGameQueryHandler(IGameRepository gameRepository, ILogger<GetGameQueryHandler> logger) : IRequestHandler<GetGameQuery, GameDto>
{
    /// <summary>
    /// Handles the retrieval of a game by its unique identifier.
    /// </summary>
    /// <param name="query">The query containing the game ID to retrieve.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the game's DTO.</returns>
    /// <exception cref="GameNotFoundException">Thrown when the game with the specified ID is not found.</exception>
    public async Task<GameDto> Handle(GetGameQuery query, CancellationToken cancellationToken)
    {
        var game = await gameRepository.GetByIdAsync(query.GameId, cancellationToken);
            
        if (game is null) throw new GameNotFoundException(query.GameId);
        
        logger.LogInformation("Requested game with id: {Id}", game.Id);
        
        return game.ToDto();
    }
}