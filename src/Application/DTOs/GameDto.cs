using Domain.Aggregates.Game;

namespace Application.DTOs;

public class GameDto
{
    public Guid Id { get; set; }
    public int BoardSize { get; set; }
    public int WinCondition { get; set; }
    public string CurrentPlayer { get; set; } = "";
    public string Status { get; set; } = "";
    public List<List<string>> Board { get; set; } = [];
    public int Version { get; set; }
}

public static class GameExtensions
{
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