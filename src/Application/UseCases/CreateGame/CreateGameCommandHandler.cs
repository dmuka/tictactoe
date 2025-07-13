using Application.Abstractions;
using Application.DTOs;
using Domain.Aggregates.Game;
using MediatR;

namespace Application.UseCases.CreateGame;

public class CreateGameCommandHandler(
    IGameRepository gameRepository,
    IUnitOfWork unitOfWork,
    IRandomProvider randomProvider) : IRequestHandler<CreateGameCommand, GameDto>
{
    public async Task<GameDto> Handle(CreateGameCommand request, CancellationToken cancellationToken)
    {
        var game = new Game(request.BoardSize, request.WinCondition, randomProvider);
        
        await gameRepository.AddAsync(game, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        
        return game.ToDto();
    }
}