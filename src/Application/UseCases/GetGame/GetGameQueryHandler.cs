using Application.DTOs;
using Domain.Aggregates;
using Domain.Aggregates.Game;
using Domain.Exceptions;
using MediatR;

namespace Application.UseCases.GetGame;

public class GetGameQueryHandler(IGameRepository gameRepository) : IRequestHandler<GetGameQuery, GameDto>
{
    public async Task<GameDto> Handle(GetGameQuery query, CancellationToken cancellationToken)
    {
        var game = await gameRepository.GetByIdAsync(query.GameId, cancellationToken);
            
        if (game is null) throw new GameNotFoundException(query.GameId);
            
        return game.ToDto();
    }
}