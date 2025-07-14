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
    private readonly Lock _lockObject = new ();

    public async Task<GameDto> Handle(MakeMoveCommand request, CancellationToken cancellationToken)
    {
        var game = await gameRepository.GetByIdAsync(request.GameId, cancellationToken);
        if (game is null) throw new GameNotFoundException(request.GameId);

        if (IsMoveMade(request, game)) return game.ToDto();

        lock (_lockObject)
        {
            game = gameRepository.GetByIdAsync(request.GameId, cancellationToken).GetAwaiter().GetResult();
            if (game is null) throw new GameNotFoundException(request.GameId);
    
            if (game.Version != request.ExpectedVersion)
            {
                if (IsMoveMadeByRequestPlayer(request, game)) return game.ToDto();
                
                throw new ConcurrencyException();
            }

            unitOfWork.BeginTransactionAsync(cancellationToken).GetAwaiter().GetResult();
            
            try
            {
                game.MakeMove(request.Player, request.Row, request.Col);
                gameRepository.UpdateAsync(game, cancellationToken).GetAwaiter().GetResult();
                
                unitOfWork.CommitAsync(cancellationToken).GetAwaiter().GetResult();
                
                return game.ToDto();
            }
            catch
            {
                unitOfWork.RollbackAsync(cancellationToken).GetAwaiter().GetResult();
                throw;
            }
        }
    }

    private static bool IsMoveMade(MakeMoveCommand request, Game game)
    {
        return game.Version != request.ExpectedVersion &&
               IsMoveMadeByRequestPlayer(request, game);
    }

    private static bool IsMoveMadeByRequestPlayer(MakeMoveCommand request, Game game)
    {
        return game.IsMoveMade(request.Row, request.Col) && 
               game.GetPlayerAtPosition(request.Row, request.Col) == request.Player;
    }
}