using Domain.Exceptions;

namespace Domain.Aggregates.Game;

/// <summary>
/// Represents a game with a board, players, and game status.
/// </summary>
public class Game
{
    /// <summary>
    /// Gets the unique identifier of the game.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the size of the game board.
    /// </summary>
    public int BoardSize { get; private set; }

    /// <summary>
    /// Gets the win condition for the game.
    /// </summary>
    public int WinCondition { get; private set; }

    /// <summary>
    /// Gets the current player.
    /// </summary>
    public string CurrentPlayer { get; private set; } = "";

    /// <summary>
    /// Gets the current status of the game.
    /// </summary>
    public GameStatus Status { get; private set; }

    /// <summary>
    /// Gets the game board.
    /// </summary>
    public List<List<string>> Board { get; private set; } = [];

    /// <summary>
    /// Gets the version of the game, used for concurrency control.
    /// </summary>
    public int Version { get; private set; }

    /// <summary>
    /// Gets the number of moves made in the game.
    /// </summary>
    public int MoveCount { get; private set; }

    private readonly IRandomProvider? _randomProvider;

    private Game() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Game"/> class.
    /// </summary>
    /// <param name="boardSize">The size of the game board.</param>
    /// <param name="winCondition">The win condition for the game.</param>
    /// <param name="randomProvider">The random provider for random events.</param>
    /// <exception cref="ArgumentException">Thrown when board size or win condition is invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown when random provider is null.</exception>
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

    /// <summary>
    /// Initializes the game board with empty cells.
    /// </summary>
    /// <param name="boardSize">The size of the board.</param>
    /// <returns>A 2D list representing the initialized board.</returns>
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

    /// <summary>
    /// Makes a move on the board for the current player.
    /// </summary>
    /// <param name="player">The player making the move.</param>
    /// <param name="row">The row index for the move.</param>
    /// <param name="col">The column index for the move.</param>
    /// <exception cref="InvalidMoveException">Thrown when the move is invalid.</exception>
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
    
    /// <summary>
    /// Checks if a move has been made at the specified position.
    /// </summary>
    /// <param name="row">The row index to check.</param>
    /// <param name="col">The column index to check.</param>
    /// <returns>True if a move has been made; otherwise, false.</returns>
    public bool IsMoveMade(int row, int col)
    {
        return Board[row][col] != "";
    }

    /// <summary>
    /// Gets the player at the specified board position.
    /// </summary>
    /// <param name="row">The row index of the position.</param>
    /// <param name="col">The column index of the position.</param>
    /// <returns>The player at the specified position.</returns>
    public string GetPlayerAtPosition(int row, int col)
    {
        return Board[row][col];
    }

    /// <summary>
    /// Validates a move for the specified player and position.
    /// </summary>
    /// <param name="player">The player making the move.</param>
    /// <param name="row">The row index for the move.</param>
    /// <param name="col">The column index for the move.</param>
    /// <exception cref="InvalidMoveException">Thrown when the move is invalid.</exception>
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

    /// <summary>
    /// Checks the game status after a move is made.
    /// </summary>
    /// <param name="player">The player who made the move.</param>
    /// <param name="row">The row index of the move.</param>
    /// <param name="col">The column index of the move.</param>
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

    /// <summary>
    /// Checks if the win condition is met for a player.
    /// </summary>
    /// <param name="player">The player to check for a win.</param>
    /// <param name="row">The row index of the last move.</param>
    /// <param name="col">The column index of the last move.</param>
    /// <returns>True if the win condition is met; otherwise, false.</returns>
    private bool CheckWinCondition(string player, int row, int col)
    {
        return CheckWinInLine(player, row, col, 1, 0) ||
               CheckWinInLine(player, row, col, 0, 1) ||
               CheckWinInLine(player, row, col, 1, 1) ||
               CheckWinInLine(player, row, col, 1, -1);
    }

    /// <summary>
    /// Checks if a player has won in a specific line direction.
    /// </summary>
    /// <param name="player">The player to check for a win.</param>
    /// <param name="row">The starting row index.</param>
    /// <param name="col">The starting column index.</param>
    /// <param name="deltaRow">The row direction to check.</param>
    /// <param name="deltaCol">The column direction to check.</param>
    /// <returns>True if the player has won in the line; otherwise, false.</returns>
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

    /// <summary>
    /// Checks the number of consecutive player marks in a specific direction.
    /// </summary>
    /// <param name="player">The player to check for consecutive marks.</param>
    /// <param name="row">The starting row index.</param>
    /// <param name="col">The starting column index.</param>
    /// <param name="deltaRow">The row direction to check.</param>
    /// <param name="deltaCol">The column direction to check.</param>
    /// <returns>The number of consecutive marks in the specified direction.</returns>
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

    /// <summary>
    /// Determines if a position is within the bounds of the board.
    /// </summary>
    /// <param name="row">The row index to check.</param>
    /// <param name="col">The column index to check.</param>
    /// <returns>True if the position is within bounds; otherwise, false.</returns>
    private bool IsInBounds(int row, int col) => 
        row >= 0 && row < BoardSize && col >= 0 && col < BoardSize;

    /// <summary>
    /// Checks if the board is full.
    /// </summary>
    /// <returns>True if the board is full; otherwise, false.</returns>
    private bool IsBoardFull() =>
        Board.All(row => row.All(cell => cell != ""));
}