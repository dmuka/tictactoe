using Application.Abstractions;
using Application.UseCases.MakeMove;
using Domain.Aggregates.Game;
using Domain.Exceptions;
using Moq;

namespace UnitTests.Application.UseCases;

public class MakeMoveCommandHandlerTests
{
    private const int BoardSize = 3;
    private const int WinCondition = 3;

    private readonly Game _game;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    
    private readonly Mock<IGameRepository> _mockGameRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;

    private readonly MakeMoveCommandHandler _handler;

    public MakeMoveCommandHandlerTests()
    {
        _mockGameRepository = new Mock<IGameRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        var randomMock = new Mock<IRandomProvider>();
        randomMock.Setup(r => r.NextDouble()).Returns(0.5);
        
        _game = new Game(BoardSize, WinCondition, randomMock.Object);
            
        _handler = new MakeMoveCommandHandler(_mockGameRepository.Object, _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_ValidMove_ShouldUpdateGameAndCommitTransaction()
    {
        // Arrange
        var gameId = Guid.CreateVersion7();
        var expectedVersion = _game.Version;
            
        var command = new MakeMoveCommand(gameId, GameConstants.XPlayer, 0, 0, expectedVersion);

        _mockGameRepository
            .Setup(repo => repo.GetByIdAsync(gameId, _cancellationToken))
            .ReturnsAsync(_game);

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

        // Assert
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(_cancellationToken), Times.Once);
        _mockGameRepository.Verify(repo => repo.GetByIdAsync(gameId, _cancellationToken), Times.Once);
        _mockGameRepository.Verify(repo => repo.UpdateAsync(_game, _cancellationToken), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(_cancellationToken), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(_cancellationToken), Times.Never);
            
        Assert.Equal(GameConstants.XPlayer, _game.Board[0][0]);
        Assert.Equal(GameConstants.OPlayer, _game.CurrentPlayer);
        Assert.Equal(expectedVersion + 1, _game.Version);
        Assert.NotNull(result);
        Assert.Equal(_game.Id, result.Id);
    }

    [Fact]
    public async Task Handle_GameNotFound_ShouldThrowGameNotFoundException()
    {
        // Arrange
        var gameId = Guid.CreateVersion7();
        var command = new MakeMoveCommand(gameId, GameConstants.XPlayer, 0, 0, 1);

        _mockGameRepository
            .Setup(repo => repo.GetByIdAsync(gameId, _cancellationToken))
            .ReturnsAsync((Game)null!);

        // Act & Assert
        await Assert.ThrowsAsync<GameNotFoundException>(async () => 
            await _handler.Handle(command, _cancellationToken));
            
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(_cancellationToken), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(_cancellationToken), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(_cancellationToken), Times.Never);
    }

    [Fact]
    public async Task Handle_VersionMismatch_ShouldThrowConcurrencyException()
    {
        // Arrange
        var gameId = Guid.CreateVersion7();
        var wrongVersion = _game.Version + 1;
            
        var command = new MakeMoveCommand(gameId, GameConstants.XPlayer, 0, 0, wrongVersion);

        _mockGameRepository
            .Setup(repo => repo.GetByIdAsync(gameId, _cancellationToken))
            .ReturnsAsync(_game);

        // Act & Assert
        await Assert.ThrowsAsync<ConcurrencyException>(async () => 
            await _handler.Handle(command, _cancellationToken));
            
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(_cancellationToken), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(_cancellationToken), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(_cancellationToken), Times.Never);
    }

    [Fact]
    public async Task Handle_InvalidMove_ShouldThrowAndRollback()
    {
        // Arrange
        var gameId = Guid.CreateVersion7();
        var expectedVersion = _game.Version;
            
        _game.MakeMove(GameConstants.XPlayer, 0, 0);
            
        var command = new MakeMoveCommand(gameId, GameConstants.XPlayer, 0, 1, expectedVersion + 1);

        _mockGameRepository
            .Setup(repo => repo.GetByIdAsync(gameId, _cancellationToken))
            .ReturnsAsync(_game);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidMoveException>(async () => 
            await _handler.Handle(command, _cancellationToken));
            
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(_cancellationToken), Times.Once);
        _mockGameRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Game>(), _cancellationToken), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(_cancellationToken), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(_cancellationToken), Times.Never);
    }
}