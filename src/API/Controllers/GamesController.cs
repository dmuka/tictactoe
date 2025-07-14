using API.Dtos;
using Application.DTOs;
using Application.UseCases.CreateGame;
using Application.UseCases.GetAllGames;
using Application.UseCases.GetGame;
using Application.UseCases.MakeMove;
using Domain.Exceptions;
using Infrastructure.Configuration.Options;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class GamesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(GameDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateGame(IOptions<GameSettings> settings)
    {
        var command = new CreateGameCommand(settings.Value.BoardSize, settings.Value.WinCondition);
        
        var game = await mediator.Send(command);
        
        return CreatedAtAction(nameof(CreateGame), new { id = game.Id }, game);
    }
    
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GameDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGame(Guid id)
    {
        var query = new GetGameQuery(id);
        
        var game = await mediator.Send(query);
        
        Response.Headers.ETag = $"W/\"{game.Version}\"";
        
        return Ok(game);
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(GameDto[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GelAllGames()
    {
        var query = new GetAllGamesQuery();
        
        var gamesDtos = await mediator.Send(query);
        
        return Ok(gamesDtos);
    }

    [HttpPost("{id:guid}/moves")]
    [ProducesResponseType(typeof(GameDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> MakeMove(
        Guid id, 
        [FromBody] MakeMoveRequest request,
        [FromHeader(Name = "If-Match")] string ifMatch)
    {
        if (!int.TryParse(ifMatch?.Replace("W/\"", "").Replace("\"", ""), out var version))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid ETag format",
                Detail = "ETag must be in format W/\"version\"",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            var command = new MakeMoveCommand(
                id, request.Player, request.Row, request.Col, version);
            
            var game = await mediator.Send(command);
                
            Response.Headers.ETag = $"W/\"{game.Version}\"";
            
            return Ok(game);
        }
        catch (GameNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Game not found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidMoveException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid move",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (ConcurrencyException)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Concurrency conflict",
                Detail = "The game state has changed since your last request",
                Status = StatusCodes.Status409Conflict
            });
        }
    }
}