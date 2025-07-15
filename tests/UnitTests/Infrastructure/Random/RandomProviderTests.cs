using Infrastructure.Random;

namespace UnitTests.Infrastructure.Random;

public class RandomProviderTests
{
    private readonly RandomProvider _randomProvider = new();

    [Fact]
    public void NextDouble_ShouldReturnValueBetweenZeroAndOne()
    {
        // Act
        var result = _randomProvider.NextDouble();

        // Assert
        Assert.InRange(result, 0.0, 1.0);
    }

    [Fact]
    public void NextDouble_ShouldReturnDifferentValuesOnSubsequentCalls()
    {
        // Act
        var firstResult = _randomProvider.NextDouble();
        var secondResult = _randomProvider.NextDouble();

        // Assert
        Assert.NotEqual(firstResult, secondResult);
    }
}