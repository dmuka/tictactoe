using Domain.Aggregates.Game;
using Domain.Exceptions;
using Moq;

namespace UnitTests.Domain;

public class GameTests
{
    private const int BoardSize = 3;
    private const int WinCondition = 3;
    
    private readonly Mock<IRandomProvider> _randomMock;
    
    public GameTests()
    {
        _randomMock = new Mock<IRandomProvider>();
        _randomMock.Setup(r => r.NextDouble()).Returns(0.5);
    }
    
    [Fact]
    public void CreateGame_WithValidSize_InitializesCorrectly()
    {
        // Arrange & Act
        var game = new Game(BoardSize, WinCondition, _randomMock.Object);
        
        Assert.Multiple(() =>
        {

            // Assert
            Assert.Equal(BoardSize, game.BoardSize);
            Assert.Equal(WinCondition, game.WinCondition);
            Assert.Equal(GameConstants.XPlayer, game.CurrentPlayer);
            Assert.Equal(GameStatus.InProgress, game.Status);
            Assert.All(game.Board, row => Assert.All(row, cell => Assert.Equal("", cell)));
        });
    }

    [Theory]
    [InlineData(2)]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateGame_WithInvalidSize_ThrowsException(int size)
    {
        Assert.Throws<ArgumentException>(() => new Game(size, 3, _randomMock.Object));
    }

    [Fact]
    public void MakeMove_ValidMove_UpdatesBoard()
    {
        // Arrange
        var game = new Game(BoardSize, WinCondition, _randomMock.Object);
        
        // Act
        game.MakeMove(GameConstants.XPlayer, 0, 0);
        
        // Assert
        Assert.Equal(GameConstants.XPlayer, game.Board[0][0]);
        Assert.Equal(GameConstants.OPlayer, game.CurrentPlayer);
    }

    [Fact]
    public void CheckWin_HorizontalWin_DetectsCorrectly()
    {
        // Arrange
        var game = new Game(BoardSize, WinCondition, _randomMock.Object);
        game.MakeMove(GameConstants.XPlayer, 0, 0);
        game.MakeMove(GameConstants.OPlayer, 1, 0);
        game.MakeMove(GameConstants.XPlayer, 0, 1);
        game.MakeMove(GameConstants.OPlayer, 1, 1);
        
        // Act
        game.MakeMove(GameConstants.XPlayer, 0, 2);
        
        // Assert
        Assert.Equal(GameStatus.XPlayerWon, game.Status);
    }

    [Fact]
    public void CheckWin_DiagonalWin_DetectsCorrectly()
    {
        // Arrange
        var game = new Game(3, 3, _randomMock.Object);
        game.MakeMove(GameConstants.XPlayer, 0, 0);
        game.MakeMove(GameConstants.OPlayer, 0, 1);
        game.MakeMove(GameConstants.XPlayer, 1, 1);
        game.MakeMove(GameConstants.OPlayer, 0, 2);
        
        // Act
        game.MakeMove(GameConstants.XPlayer, 2, 2);
        
        // Assert
        Assert.Equal(GameStatus.XPlayerWon, game.Status);
    }

    [Fact]
    public void IsBoardFull_WhenFull_ReturnsTrue()
    {
        // Arrange
        var game = new Game(BoardSize, WinCondition, _randomMock.Object);
        
        game.MakeMove(GameConstants.XPlayer, 0, 0);
        game.MakeMove(GameConstants.OPlayer, 0, 1);
        game.MakeMove(GameConstants.XPlayer, 0, 2);
        game.MakeMove(GameConstants.OPlayer, 1, 0);
        game.MakeMove(GameConstants.XPlayer, 1, 2);
        game.MakeMove(GameConstants.OPlayer, 1, 1);
        game.MakeMove(GameConstants.XPlayer, 2, 0);
        game.MakeMove(GameConstants.OPlayer, 2, 2);
        game.MakeMove(GameConstants.XPlayer, 2, 1);
        
        // Assert
        Assert.Equal(GameStatus.Draw, game.Status);
    }    
    
    [Fact]
    public void MakeMove_ShouldPlacePlayerMarkAndSwitchCurrentPlayer()
    {
        // Arrange
        var game = new Game(BoardSize, WinCondition, _randomMock.Object);

        // Act
        game.MakeMove(GameConstants.XPlayer, 0, 0);

        // Assert
        Assert.Equal(GameConstants.XPlayer, game.Board[0][0]);
        Assert.Equal(GameConstants.OPlayer, game.CurrentPlayer);
        Assert.Equal(1, game.MoveCount);
        Assert.Equal(GameStatus.InProgress, game.Status);
    }

    [Fact]
    public void MakeMove_ShouldRandomlyChangePlayer_WhenMoveCountDivisibleBy3AndRandomBelowThreshold()
    {
        // Arrange
        _randomMock.Setup(r => r.NextDouble()).Returns(0.05);

        var game = new Game(BoardSize, WinCondition, _randomMock.Object);

        game.MakeMove(GameConstants.XPlayer, 0, 0);
        game.MakeMove(GameConstants.OPlayer, 0, 1);
        game.MakeMove(GameConstants.XPlayer, 1, 0);

        game.MakeMove(GameConstants.OPlayer, 1, 1);

        Assert.Equal(GameConstants.XPlayer, game.Board[1][1]);
    }

    [Fact]
    public void MakeMove_ShouldThrow_WhenNotPlayersTurn()
    {
        // Arrange
        var game = new Game(BoardSize, WinCondition, _randomMock.Object);

        // Act & Assert
        var ex = Assert.Throws<InvalidMoveException>(() =>
            game.MakeMove(GameConstants.OPlayer, 0, 0)
        );
        Assert.Equal(GameErrors.NotThisPlayerTurn, ex.Message);
    }

    [Fact]
    public void MakeMove_ShouldThrow_WhenPositionIsOccupied()
    {
        var game = new Game(BoardSize, WinCondition, _randomMock.Object);

        game.MakeMove(GameConstants.XPlayer, 0, 0);

        var ex = Assert.Throws<InvalidMoveException>(() =>
            game.MakeMove(GameConstants.OPlayer, 0, 0)
        );
        Assert.Equal(GameErrors.OccupiedPosition, ex.Message);
    }
}