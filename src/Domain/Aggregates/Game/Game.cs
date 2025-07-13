namespace Domain.Aggregates.Game;

public class Game
{
    public Guid Id { get; private set; }
    public int BoardSize { get; private set; }
    public int WinCondition { get; private set; }
    public string CurrentPlayer { get; private set; } = "";
    public GameStatus Status { get; private set; }
    public List<List<string>> Board { get; private set; } = [];
    public int Version { get; private set; }
    public int MoveCount { get; private set; }

    private Game() { }

    public Game(int boardSize, int winCondition)
    {
        if (boardSize < GameConstants.MinBoardSize) throw new ArgumentException($"Board size must be at least {GameConstants.MinBoardSize}");
        if (winCondition < GameConstants.MinWinCondition || winCondition > boardSize) 
            throw new ArgumentException($"Win condition must be between {GameConstants.MinWinCondition} and board size");
        
        Id = Guid.CreateVersion7();
        BoardSize = boardSize;
        WinCondition = winCondition;
        CurrentPlayer = GameConstants.XPlayer;
        Status = GameStatus.InProgress;
        Board = InitBoard(boardSize);
        Version = 1;
        MoveCount = 0;
    }

    private static List<List<string>> InitBoard(int boardSize)
    {
        var board = new List<List<string>>();
        for (var i = 0; i < boardSize; i++)
        {
            var row = new List<string>();
            for (var j = 0; j < boardSize; j++)
            {
                row.Add(GameConstants.EmptyCell);
            }
            board.Add(row);
        }
        
        return board;
    }
}