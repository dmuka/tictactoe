using Application.Abstractions;
using Application.DTOs;
using Application.UseCases.CreateGame;
using Domain.Aggregates.Game;
using Moq;

namespace UnitTests.Application.UseCases;

public class CreateGameCommandHandlerTests
{
    private const int BoardSize = 3;
    private const int WinCondition = 3;
        
    private readonly Mock<IGameRepository> _mockGameRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        
    private readonly CreateGameCommandHandler _handler;

    public CreateGameCommandHandlerTests()
    {
        _mockGameRepository = new Mock<IGameRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        var randomMock = new Mock<IRandomProvider>();
        randomMock.Setup(randomProvider => randomProvider.NextDouble()).Returns(0.5);
            
        _handler = new CreateGameCommandHandler(_mockGameRepository.Object, _mockUnitOfWork.Object, randomMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateNewGame_WithCorrectParameters()
    {
        // Arrange
        var command = new CreateGameCommand(BoardSize, WinCondition);

        Game capturedGame = null!;
        _mockGameRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
            .Callback<Game, CancellationToken>((game, _) => capturedGame = game)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockGameRepository.Verify(repository => repository.AddAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            
        Assert.NotNull(capturedGame);
        Assert.Equal(command.BoardSize, capturedGame.BoardSize);
        Assert.Equal(command.WinCondition, capturedGame.WinCondition);
        Assert.Equal(GameConstants.XPlayer, capturedGame.CurrentPlayer);
        Assert.Equal(GameStatus.InProgress, capturedGame.Status);
        Assert.Equal(1, capturedGame.Version);
        Assert.Equal(0, capturedGame.MoveCount);
            
        Assert.NotNull(result);
        Assert.IsType<GameDto>(result);
        Assert.Equal(capturedGame.Id, result.Id);
        Assert.Equal(capturedGame.BoardSize, result.BoardSize);
        Assert.Equal(capturedGame.WinCondition, result.WinCondition);
    }
}