﻿using Application.Abstractions;
using Application.DTOs;
using Domain.Aggregates.Game;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.MakeMove;

/// <summary>
/// Handles the process of making a move in a game.
/// </summary>
public class MakeMoveCommandHandler(
    IGameRepository gameRepository,
    IUnitOfWork unitOfWork,
    ILogger<MakeMoveCommandHandler> logger) : IRequestHandler<MakeMoveCommand, GameDto>
{
    private readonly Lock _lockObject = new ();

    /// <summary>
    /// Handles the execution of a move in a game.
    /// </summary>
    /// <param name="request">The command containing the move details.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated game's DTO.</returns>
    /// <exception cref="GameNotFoundException">Thrown when the game with the specified ID is not found.</exception>
    /// <exception cref="ConcurrencyException">Thrown when a concurrency conflict occurs.</exception>
    public async Task<GameDto> Handle(MakeMoveCommand request, CancellationToken cancellationToken)
    {
        var game = await gameRepository.GetByIdAsync(request.GameId, cancellationToken);
        if (game is null) throw new GameNotFoundException(request.GameId);

        if (IsMoveMade(request, game))
        {
            logger.LogInformation("{Player} player already make this move (row: {Row}, column: {Column})", request.Player, request.Row, request.Col);
            
            return game.ToDto();
        }

        lock (_lockObject)
        {
            game = gameRepository.GetByIdAsync(request.GameId, cancellationToken).GetAwaiter().GetResult();
            if (game is null) throw new GameNotFoundException(request.GameId);
    
            if (game.Version != request.ExpectedVersion)
            {
                if (!IsMoveMadeByRequestPlayer(request, game)) throw new ConcurrencyException();
                
                logger.LogInformation("{Player} player already make this move (row: {Row}, column: {Column})", request.Player, request.Row, request.Col);
                    
                return game.ToDto();
            }

            unitOfWork.BeginTransactionAsync(cancellationToken).GetAwaiter().GetResult();
            
            try
            {
                game.MakeMove(request.Player, request.Row, request.Col);
                gameRepository.UpdateAsync(game, cancellationToken).GetAwaiter().GetResult();
                
                unitOfWork.CommitAsync(cancellationToken).GetAwaiter().GetResult();
                
                logger.LogInformation("{Player} player make move (row: {Row}, column: {Column})", request.Player, request.Row, request.Col);
                
                return game.ToDto();
            }
            catch
            {
                unitOfWork.RollbackAsync(cancellationToken).GetAwaiter().GetResult();
                
                logger.LogError("Game with id: {Id}, current player: {Player}, request player: {RequestPlayer}, move: {Row}, {Col}, version: {Version}, expected version: {ExVersion}", 
                    game.Id, game.CurrentPlayer, request.Player, request.Row, request.Col, game.Version, request.ExpectedVersion);
                
                throw;
            }
        }
    }

    /// <summary>
    /// Checks if a move has already been made in the game.
    /// </summary>
    /// <param name="request">The command containing the move details.</param>
    /// <param name="game">The game to check for the move.</param>
    /// <returns>True if the move has already been made; otherwise, false.</returns>
    private static bool IsMoveMade(MakeMoveCommand request, Game game)
    {
        return game.Version != request.ExpectedVersion &&
               IsMoveMadeByRequestPlayer(request, game);
    }

    /// <summary>
    /// Checks if the move was made by the player specified in the request.
    /// </summary>
    /// <param name="request">The command containing the move details.</param>
    /// <param name="game">The game to check for the move.</param>
    /// <returns>True if the move was made by the request player; otherwise, false.</returns>
    private static bool IsMoveMadeByRequestPlayer(MakeMoveCommand request, Game game)
    {
        return game.IsMoveMade(request.Row, request.Col) && 
               game.GetPlayerAtPosition(request.Row, request.Col) == request.Player;
    }
}