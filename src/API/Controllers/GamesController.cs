using API.Dtos;
using Application.DTOs;
using Application.UseCases.CreateGame;
using Application.UseCases.GetAllGames;
using Application.UseCases.GetGame;
using Application.UseCases.MakeMove;
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
    /// <summary>
    /// Creates a new game.
    /// </summary>
    /// <param name="settings">The game settings including board size and win condition.</param>
    /// <returns>The created game details.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(GameDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateGame(IOptions<GameSettings> settings)
    {
        var command = new CreateGameCommand(settings.Value.BoardSize, settings.Value.WinCondition);
        
        var game = await mediator.Send(command);
        
        return CreatedAtAction(nameof(CreateGame), new { id = game.Id }, game);
    }
    
    /// <summary>
    /// Retrieves a game by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the game.</param>
    /// <returns>The game details if found; otherwise, a 404 status.</returns>
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
    
    /// <summary>
    /// Retrieves all games.
    /// </summary>
    /// <returns>A list of all games.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(GameDto[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GelAllGames()
    {
        var query = new GetAllGamesQuery();
        
        var gamesDtos = await mediator.Send(query);
        
        return Ok(gamesDtos);
    }

    /// <summary>
    /// Makes a move in a game.
    /// </summary>
    /// <param name="id">The unique identifier of the game.</param>
    /// <param name="request">The move request containing player and move details.</param>
    /// <param name="ifMatch">The ETag header value for concurrency control in the format:
    /// W/"version", where "W/" indicates a weak validator and
    /// "version" is the current version of the resource (integer sequential number).</param>
    /// <returns>The updated game details if successful; otherwise, an error status.</returns>
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
        
        var command = new MakeMoveCommand(
            id, request.Player, request.Row, request.Col, version);
            
        var game = await mediator.Send(command);
                
        Response.Headers.ETag = $"W/\"{game.Version}\"";
            
        return Ok(game);
    }
}