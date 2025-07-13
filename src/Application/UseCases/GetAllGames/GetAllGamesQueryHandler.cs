using Application.DTOs;
using Domain.Aggregates.Game;
using MediatR;

namespace Application.UseCases.GetAllGames;

public class GetAllGamesQueryHandler(IGameRepository gameRepository) : IRequestHandler<GetAllGamesQuery, GameDto[]>
{
    public async Task<GameDto[]> Handle(GetAllGamesQuery query, CancellationToken cancellationToken)
    {
        var games = await gameRepository.GetAllAsync(cancellationToken);

        return games.Select(game => game.ToDto()).ToArray();
    }
}