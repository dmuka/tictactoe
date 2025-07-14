using Application.Abstractions;
using Application.DTOs;
using Domain.Aggregates.Game;
using Domain.Exceptions;
using MediatR;

namespace Application.UseCases.MakeMove;

public class MakeMoveCommandHandler(
    IGameRepository gameRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<MakeMoveCommand, GameDto>
{
    public async Task<GameDto> Handle(MakeMoveCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var game = await gameRepository.GetByIdAsync(request.GameId, cancellationToken);
            
            if (game == null) throw new GameNotFoundException(request.GameId);

            if (game.Version != request.ExpectedVersion) throw new ConcurrencyException();

            game.MakeMove(request.Player, request.Row, request.Col);
            await gameRepository.UpdateAsync(game, cancellationToken);
            
            await unitOfWork.CommitAsync(cancellationToken);
            
            return game.ToDto();
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}