using Application.Abstractions;
using Application.DTOs;
using Domain.Aggregates.Game;
using MediatR;

namespace Application.UseCases.CreateGame;

/// <summary>
/// Handles the creation of a new game.
/// </summary>
public class CreateGameCommandHandler(
    IGameRepository gameRepository,
    IUnitOfWork unitOfWork,
    IRandomProvider randomProvider) : IRequestHandler<CreateGameCommand, GameDto>
{
    /// <summary>
    /// Handles the creation of a new game based on the provided command.
    /// </summary>
    /// <param name="request">The command containing the game creation details.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created game's DTO.</returns>
    public async Task<GameDto> Handle(CreateGameCommand request, CancellationToken cancellationToken)
    {
        var game = new Game(request.BoardSize, request.WinCondition, randomProvider);
        
        await gameRepository.AddAsync(game, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        
        return game.ToDto();
    }
}