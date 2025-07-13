using Application.DTOs;
using Application.UseCases.CreateGame;
using Application.UseCases.GetGame;
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
}