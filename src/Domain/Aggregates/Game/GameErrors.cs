namespace Domain.Aggregates.Game;

public class GameErrors
{
    public const string NotThisPlayerTurn = "It's not this player's turn.";
    public const string OccupiedPosition = "Position is already occupied.";
    public const string OutOfBounds = "Position is out of bounds.";
    public const string GameFinished = "Game is already finished.";
}