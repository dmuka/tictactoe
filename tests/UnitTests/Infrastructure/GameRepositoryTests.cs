using Domain.Aggregates.Game;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace UnitTests.Infrastructure;

public class GameRepositoryTests
{
    private readonly Mock<IRandomProvider> _randomMock;
    private readonly DbContextOptions<AppDbContext> _options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.CreateVersion7().ToString())
        .Options;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public GameRepositoryTests()
    {
        _randomMock = new Mock<IRandomProvider>();
        _randomMock.Setup(r => r.NextDouble()).Returns(0.5);
    }
    
    [Fact]
    public async Task GetByIdAsync_ReturnsGame_WhenGameExists()
    {
        // Arrange
        var game = CreateSampleGame();

        await using (var context = new AppDbContext(_options))
        {
            await context.Games.AddAsync(game, _cancellationToken);
            await context.SaveChangesAsync(_cancellationToken);
        }

        // Act
        await using (var context = new AppDbContext(_options))
        {
            var repository = new GameRepository(context);
            var result = await repository.GetByIdAsync(game.Id, _cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(game.Id, result.Id);
            Assert.Equal(game.Version, result.Version);
            Assert.Equal(game.BoardSize, result.BoardSize);
        }
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenGameDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        await using var context = new AppDbContext(_options);
        var repository = new GameRepository(context);
        var result = await repository.GetByIdAsync(nonExistentId, _cancellationToken);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllGames()
    {
        // Arrange
        var game1 = CreateSampleGame();
        var game2 = CreateSampleGame();

        await using (var context = new AppDbContext(_options))
        {
            await context.Games.AddRangeAsync(game1, game2);
            await context.SaveChangesAsync(_cancellationToken);
        }

        // Act
        await using (var context = new AppDbContext(_options))
        {
            var repository = new GameRepository(context);
            var result = await repository.GetAllAsync(_cancellationToken);

            // Assert
            Assert.Equal(2, result.Length);
            Assert.Contains(result, g => g.Id == game1.Id);
            Assert.Contains(result, g => g.Id == game2.Id);
        }
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyArray_WhenNoGamesExist()
    {
        // Act
        await using var context = new AppDbContext(_options);
        var repository = new GameRepository(context);
        var result = await repository.GetAllAsync(_cancellationToken);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task AddAsync_AddsGameToContext()
    {
        // Arrange
        var game = CreateSampleGame();

        // Act
        await using (var context = new AppDbContext(_options))
        {
            var repository = new GameRepository(context);
            await repository.AddAsync(game, _cancellationToken);
            await context.SaveChangesAsync(_cancellationToken);
        }

        // Assert
        await using (var context = new AppDbContext(_options))
        {
            var savedGame = await context.Games.FindAsync([game.Id], _cancellationToken);
            Assert.NotNull(savedGame);
            Assert.Equal(game.Id, savedGame.Id);
            Assert.Equal(game.Version, savedGame.Version);
        }
    }

    [Fact]
    public async Task UpdateAsync_UpdatesGame_WhenVersionMatches()
    {
        // Arrange
        var game = CreateSampleGame();

        await using (var context = new AppDbContext(_options))
        {
            await context.Games.AddAsync(game, _cancellationToken);
            await context.SaveChangesAsync(_cancellationToken);
        }

        // Act
        await using (var context = new AppDbContext(_options))
        {
            var repository = new GameRepository(context);
            var gameToUpdate = await repository.GetByIdAsync(game.Id, _cancellationToken);
                
            gameToUpdate!.MakeMove(GameConstants.XPlayer, 0, 0);
                
            await repository.UpdateAsync(gameToUpdate, _cancellationToken);
            await context.SaveChangesAsync(_cancellationToken);
        }

        // Assert
        await using (var context = new AppDbContext(_options))
        {
            var updatedGame = await context.Games.FindAsync([game.Id], _cancellationToken);
            Assert.NotNull(updatedGame);
            Assert.Equal(game.Id, updatedGame.Id);
            Assert.Equal(game.Version + 1, updatedGame.Version);
            Assert.NotNull(updatedGame.Board);
        }
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenGameExists()
    {
        // Arrange
        var game = CreateSampleGame();

        await using (var context = new AppDbContext(_options))
        {
            await context.Games.AddAsync(game, _cancellationToken);
            await context.SaveChangesAsync(_cancellationToken);
        }

        // Act
        await using (var context = new AppDbContext(_options))
        {
            var repository = new GameRepository(context);
            var exists = await repository.ExistsAsync(game.Id, _cancellationToken);

            // Assert
            Assert.True(exists);
        }
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenGameDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        await using var context = new AppDbContext(_options);
        var repository = new GameRepository(context);
        var exists = await repository.ExistsAsync(nonExistentId, _cancellationToken);

        // Assert
        Assert.False(exists);
    }

    private Game CreateSampleGame()
    {
        return new Game(3, 3, _randomMock.Object);
    }
}