using System.ComponentModel.DataAnnotations;
using Infrastructure.Configuration.Options;

namespace UnitTests.Infrastructure.Configuration.Options;

public class WinConditionAttributeTests
{
    private static readonly GameSettings GameSettings = new() { BoardSize = 5, WinCondition = 3 };
    private readonly ValidationContext _validationContext = new(GameSettings);
    
    private readonly WinConditionAttribute _attribute = new ();

    [Fact]
    public void IsValid_NonIntegerValue_ShouldReturnError()
    {
        // Arrange & Act
        var result = _attribute.GetValidationResult("not an integer", _validationContext);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Win condition must be an integer.", result.ErrorMessage);
    }

    [Fact]
    public void IsValid_WinConditionLessThanThree_ShouldReturnError()
    {
        // Arrange & Act
        var result = _attribute.GetValidationResult(2, _validationContext);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Win condition must be at least 3.", result.ErrorMessage);
    }

    [Fact]
    public void IsValid_WinConditionGreaterThanBoardSize_ShouldReturnError()
    {
        // Arrange & Act
        var result = _attribute.GetValidationResult(6, _validationContext);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Win condition can't be greater than board size.", result.ErrorMessage);
    }

    [Fact]
    public void IsValid_ValidWinCondition_ShouldReturnSuccess()
    {
        // Arrange & Act
        var result = _attribute.GetValidationResult(4, _validationContext);

        // Assert
        Assert.Null(result);
    }
}