using Domain.Aggregates.Game;

namespace Application.DTOs;

/// <summary>
/// Data Transfer Object for representing game details.
/// </summary>
public class GameDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the game.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the size of the game board.
    /// </summary>
    public int BoardSize { get; set; }

    /// <summary>
    /// Gets or sets the win condition for the game.
    /// </summary>
    public int WinCondition { get; set; }

    /// <summary>
    /// Gets or sets the current player.
    /// </summary>
    public string CurrentPlayer { get; set; } = "";

    /// <summary>
    /// Gets or sets the current status of the game.
    /// </summary>
    public string Status { get; set; } = "";

    /// <summary>
    /// Gets or sets the game board.
    /// </summary>
    public List<List<string>> Board { get; set; } = [];

    /// <summary>
    /// Gets or sets the version of the game, used for concurrency control.
    /// </summary>
    public int Version { get; set; }
}

/// <summary>
/// Extension methods for converting game entities to DTOs.
/// </summary>
public static class GameExtensions
{
    /// <summary>
    /// Converts a <see cref="Game"/> entity to a <see cref="GameDto"/>.
    /// </summary>
    /// <param name="game">The game entity to convert.</param>
    /// <returns>A <see cref="GameDto"/> representing the game details.</returns>
    public static GameDto ToDto(this Game game)
    {
        return new GameDto
        {
            Id = game.Id,
            BoardSize = game.BoardSize,
            WinCondition = game.WinCondition,
            CurrentPlayer = game.CurrentPlayer,
            Status = game.Status.ToString(),
            Board = game.Board,
            Version = game.Version
        };
    }
}