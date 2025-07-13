using Domain.Exceptions;

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

    private readonly IRandomProvider? _randomProvider;

    private Game() { }

    public Game(
        int boardSize, 
        int winCondition, 
        IRandomProvider randomProvider)
    {
        if (boardSize < GameConstants.MinBoardSize) throw new ArgumentException($"Board size must be at least {GameConstants.MinBoardSize}");
        if (winCondition < GameConstants.MinWinCondition || winCondition > boardSize) 
            throw new ArgumentException($"Win condition must be between {GameConstants.MinWinCondition} and board size");

        _randomProvider = randomProvider ?? throw new ArgumentNullException(nameof(randomProvider));
        
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

    public void MakeMove(string player, int row, int col)
    {
        ValidateMove(player, row, col);

        if (MoveCount > 0 && MoveCount % 3 == 0 && _randomProvider?.NextDouble() < 0.1)
        {
            player = player == GameConstants.XPlayer ? GameConstants.OPlayer : GameConstants.XPlayer;
        }

        Board[row][col] = player;
        MoveCount++;
        Version++;

        CheckGameStatus(player, row, col);
        
        if (Status == GameStatus.InProgress)
        {
            CurrentPlayer = CurrentPlayer == GameConstants.XPlayer ? GameConstants.OPlayer : GameConstants.XPlayer;
        }
    }

    private void ValidateMove(string player, int row, int col)
    {
        if (Status != GameStatus.InProgress)
            throw new InvalidMoveException(GameErrors.GameFinished);

        if (player != CurrentPlayer)
            throw new InvalidMoveException(GameErrors.NotThisPlayerTurn);

        if (row < 0 || row >= BoardSize || col < 0 || col >= BoardSize)
            throw new InvalidMoveException(GameErrors.OutOfBounds);

        if (Board[row][col] != "")
            throw new InvalidMoveException(GameErrors.OccupiedPosition);
    }

    private void CheckGameStatus(string player, int row, int col)
    {
        if (CheckWinCondition(player, row, col))
        {
            Status = player == GameConstants.XPlayer ? GameStatus.XPlayerWon : GameStatus.OPlayerWon;
        }
        else if (IsBoardFull())
        {
            Status = GameStatus.Draw;
        }
    }

    private bool CheckWinCondition(string player, int row, int col)
    {
        return CheckWinInLine(player, row, col, 1, 0) ||
               CheckWinInLine(player, row, col, 0, 1) ||
               CheckWinInLine(player, row, col, 1, 1) ||
               CheckWinInLine(player, row, col, 1, -1);
    }

    private bool CheckWinInLine(
        string player, 
        int row, 
        int col, 
        int deltaRow, 
        int deltaCol)
    {
        var count = 1;

        count += CheckLineInDirection(player, row, col, deltaRow, deltaCol);
        count += CheckLineInDirection(player, row, col, -deltaRow, -deltaCol);

        return count >= WinCondition;
    }

    private int CheckLineInDirection(
        string player, 
        int row, 
        int col, 
        int deltaRow, 
        int deltaCol)
    {
        var count = 0;
        var currentRow = row + deltaRow;
        var currentCol = col + deltaCol;

        while (IsInBounds(currentRow, currentCol) && 
               Board[currentRow][currentCol] == player)
        {
            count++;
            currentRow += deltaRow;
            currentCol += deltaCol;
        }

        return count;
    }

    private bool IsInBounds(int row, int col) => 
        row >= 0 && row < BoardSize && col >= 0 && col < BoardSize;

    private bool IsBoardFull() =>
        Board.All(row => row.All(cell => cell != ""));
}