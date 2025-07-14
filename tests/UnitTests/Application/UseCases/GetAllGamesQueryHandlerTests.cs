using Application.UseCases.GetAllGames;
using Domain.Aggregates.Game;
using Moq;

namespace UnitTests.Application.UseCases;

public class GetAllGamesQueryHandlerTests
{
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
 
    private readonly Mock<IGameRepository> _mockGameRepository;
    private readonly Mock<IRandomProvider> _randomMock;
    
    private readonly GetAllGamesQueryHandler _handler;

    public GetAllGamesQueryHandlerTests()
    {
        _mockGameRepository = new Mock<IGameRepository>();

        _randomMock = new Mock<IRandomProvider>();
        _randomMock.Setup(r => r.NextDouble()).Returns(0.5);
        
        _handler = new GetAllGamesQueryHandler(_mockGameRepository.Object);
    }

    [Fact]
    public async Task Handle_WithNoGames_ShouldReturnEmptyArray()
    {
        // Arrange
        _mockGameRepository
            .Setup(repo => repo.GetAllAsync(_cancellationToken))
            .ReturnsAsync([]);

        var query = new GetAllGamesQuery();

        // Act
        var result = await _handler.Handle(query, _cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockGameRepository.Verify(repo => repo.GetAllAsync(_cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMultipleGames_ShouldReturnAllGameDtos()
    {
        // Arrange
        var games = new Game[]
        {
            new (3, 3, _randomMock.Object),
            new (5, 4, _randomMock.Object),
            new (7, 5, _randomMock.Object)
        };

        _mockGameRepository
            .Setup(repo => repo.GetAllAsync(_cancellationToken))
            .ReturnsAsync(games);

        var query = new GetAllGamesQuery();

        // Act
        var result = await _handler.Handle(query, _cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(games.Length, result.Length);
            
        for (var i = 0; i < games.Length; i++)
        {
            Assert.Equal(games[i].Id, result[i].Id);
            Assert.Equal(games[i].BoardSize, result[i].BoardSize);
            Assert.Equal(games[i].WinCondition, result[i].WinCondition);
            Assert.Equal(games[i].CurrentPlayer, result[i].CurrentPlayer);
            Assert.Equal(games[i].Status.ToString(), result[i].Status);
        }
            
        _mockGameRepository.Verify(repo => repo.GetAllAsync(_cancellationToken), Times.Once);
    }
        
    [Fact]
    public async Task Handle_WithLargeNumberOfGames_ShouldProcessInParallel()
    {
        // Arrange
        var games = Enumerable.Range(0, 100)
            .Select(_ => new Game(3, 3, _randomMock.Object))
            .ToArray();

        _mockGameRepository
            .Setup(repo => repo.GetAllAsync(_cancellationToken))
            .ReturnsAsync(games);

        var query = new GetAllGamesQuery();

        // Act
        var result = await _handler.Handle(query, _cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(games.Length, result.Length);
            
        var gameIds = games.Select(g => g.Id).ToHashSet();
        var resultIds = result.Select(dto => dto.Id).ToHashSet();
            
        Assert.True(gameIds.SetEquals(resultIds));
            
        _mockGameRepository.Verify(repo => repo.GetAllAsync(_cancellationToken), Times.Once);
    }
}