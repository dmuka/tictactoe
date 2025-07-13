using Application.UseCases.GetGame;
using Domain.Aggregates.Game;
using Domain.Exceptions;
using Moq;

namespace UnitTests.Application.UseCases;

public class GetGameQueryHandlerTests
{
    private const int BoardSize = 3;
    private const int WinCondition = 3;
        
    private readonly Mock<IGameRepository> _mockRepository;
    private readonly Mock<IRandomProvider> _randomMock;
        
    private readonly GetGameQueryHandler _handler;

    public GetGameQueryHandlerTests()
    {
        _mockRepository = new Mock<IGameRepository>();

        _randomMock = new Mock<IRandomProvider>();
        _randomMock.Setup(r => r.NextDouble()).Returns(0.5);
        
        _handler = new GetGameQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ExistingGame_ReturnsGameDto()
    {
        // Arrange
        var gameId = Guid.CreateVersion7();
        var game = new Game(BoardSize, WinCondition, _randomMock.Object);
            
        _mockRepository.Setup(repo => repo.GetByIdAsync(gameId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var query = new GetGameQuery(gameId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.BoardSize);
        Assert.Equal(3, result.WinCondition);
        _mockRepository.Verify(repo => repo.GetByIdAsync(gameId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentGame_ThrowsGameNotFoundException()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        _mockRepository.Setup(repo => repo.GetByIdAsync(gameId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Game)null!);

        var query = new GetGameQuery(gameId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<GameNotFoundException>(() => 
            _handler.Handle(query, CancellationToken.None));
            
        Assert.Equal($"Game with id {gameId} not found", exception.Message);
        _mockRepository.Verify(repo => repo.GetByIdAsync(gameId, It.IsAny<CancellationToken>()), Times.Once);
    }
}