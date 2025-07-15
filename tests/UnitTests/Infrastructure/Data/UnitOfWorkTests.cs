using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace UnitTests.Infrastructure.Data;

public class UnitOfWorkTests
{
    private readonly Mock<TestableAppDbContext> _mockContext;
    private readonly Mock<IDbContextTransaction> _mockTransaction;
    private readonly Mock<DatabaseFacade> _mockDatabaseFacade;
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        _mockContext = new Mock<TestableAppDbContext>();
        _mockTransaction = new Mock<IDbContextTransaction>();
        _mockDatabaseFacade = new Mock<DatabaseFacade>(_mockContext.Object);

        _mockDatabaseFacade.Setup(db => db.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockTransaction.Object);

        _mockContext.Setup(c => c.Database).Returns(_mockDatabaseFacade.Object);

        _unitOfWork = new UnitOfWork(_mockContext.Object);
    }

    [Fact]
    public async Task BeginTransactionAsync_ShouldBeginTransaction()
    {
        // Act
        await _unitOfWork.BeginTransactionAsync(CancellationToken.None);

        // Assert
        _mockDatabaseFacade.Verify(db => db.BeginTransactionAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CommitAsync_ShouldCommitTransaction()
    {
        // Arrange
        await _unitOfWork.BeginTransactionAsync(CancellationToken.None);

        // Act
        await _unitOfWork.CommitAsync(CancellationToken.None);

        // Assert
        _mockContext.Verify(c => c.SaveChangesAsync(CancellationToken.None), Times.Once);
        _mockTransaction.Verify(t => t.CommitAsync(CancellationToken.None), Times.Once);
        _mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task CommitAsync_ShouldRollbackOnException()
    {
        // Arrange
        await _unitOfWork.BeginTransactionAsync(CancellationToken.None);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException());

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() => _unitOfWork.CommitAsync(CancellationToken.None));
        _mockTransaction.Verify(t => t.RollbackAsync(CancellationToken.None), Times.Once);
        _mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task RollbackAsync_ShouldRollbackTransaction()
    {
        // Arrange
        await _unitOfWork.BeginTransactionAsync(CancellationToken.None);

        // Act
        await _unitOfWork.RollbackAsync(CancellationToken.None);

        // Assert
        _mockTransaction.Verify(t => t.RollbackAsync(CancellationToken.None), Times.Once);
        _mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task Dispose_ShouldDisposeTransactionAndContext()
    {
        // Arrange
        await _unitOfWork.BeginTransactionAsync(CancellationToken.None);

        // Act
        await _unitOfWork.Dispose();

        // Assert
        _mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
        _mockContext.Verify(c => c.DisposeAsync(), Times.Once);
    }
}

// Derived class for testing
public class TestableAppDbContext() : AppDbContext(new DbContextOptions<AppDbContext>());