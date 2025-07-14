using Application.DTOs;
using MediatR;

namespace Application.UseCases.MakeMove;

public record MakeMoveCommand(Guid GameId, string Player, int Row, int Col, int ExpectedVersion) 
    : IRequest<GameDto>;