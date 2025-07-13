using Application.DTOs;
using MediatR;

namespace Application.UseCases.GetAllGames;

public sealed record GetAllGamesQuery : IRequest<GameDto[]>;