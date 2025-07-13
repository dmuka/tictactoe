using Application.DTOs;
using MediatR;

namespace Application.UseCases.CreateGame;

public record CreateGameCommand(int BoardSize, int WinCondition) : IRequest<GameDto>;