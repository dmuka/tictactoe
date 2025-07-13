using Application.DTOs;
using MediatR;

namespace Application.UseCases.GetGame;

public sealed record GetGameQuery(Guid GameId) : IRequest<GameDto>;