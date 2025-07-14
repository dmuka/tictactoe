using System.Text.Json;
using API.Infrastructure;
using Application.Exceptions;
using Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace UnitTests.API;

public class ExceptionHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _mockLogger;
    private readonly ExceptionHandlingMiddleware _middleware;
    private readonly Mock<RequestDelegate> _nextMock;

    public ExceptionHandlingMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        _nextMock = new Mock<RequestDelegate>();
        _middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task InvokeAsync_WithNoException_ShouldCallNext()
    {
        // Arrange
        var context = new DefaultHttpContext();
        _nextMock
            .Setup(next => next(context))
            .Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Once);
    }

    [Theory]
    [InlineData(typeof(GameNotFoundException), StatusCodes.Status404NotFound, ProblemConstants.GameNotFound)]
    [InlineData(typeof(InvalidMoveException), StatusCodes.Status400BadRequest, ProblemConstants.InvalidMoveRequest)]
    [InlineData(typeof(ConcurrencyException), StatusCodes.Status409Conflict, ProblemConstants.ConcurrencyConflict)]
    [InlineData(typeof(Exception), StatusCodes.Status500InternalServerError, ProblemConstants.ServerInternalError)]
    public async Task InvokeAsync_WithException_ShouldSetCorrectStatusCode(
        Type exceptionType, 
        int expectedStatusCode,
        string expectedTitle)
    {
        // Arrange
        var context = new DefaultHttpContext
        {
            Response =
            {
                Body = new MemoryStream()
            }
        };

        var exception = (Exception)Activator.CreateInstance(exceptionType, exceptionType == typeof(GameNotFoundException) 
            ? [Guid.NewGuid()]
            : [])!;
            
        _nextMock
            .Setup(next => next(context))
            .ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(expectedStatusCode, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);
            
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
            
        Assert.NotNull(problemDetails);
        Assert.Equal(expectedTitle, problemDetails.Title);
        Assert.Equal(expectedStatusCode, problemDetails.Status);
        Assert.Equal(context.Request.Path, problemDetails.Instance);
    }

    [Fact]
    public async Task InvokeAsync_WithValidationException_ShouldIncludeErrorsInResponse()
    {
        // Arrange
        var context = new DefaultHttpContext
        {
            Response =
            {
                Body = new MemoryStream()
            }
        };

        var errors = new Dictionary<string, string[]>
        {
            {"Field1", ["Error1", "Error2"] },
            {"Field2", ["Error3"] }
        };
            
        var exception = new GameValidationException(errors);
            
        _nextMock
            .Setup(next => next(context))
            .ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
            
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var responseObject = JsonSerializer.Deserialize<JsonDocument>(responseBody);
            
        Assert.NotNull(responseObject);
        Assert.True(responseObject.RootElement.TryGetProperty("errors", out var errorsElement));
            
        foreach (var error in errors)
        {
            Assert.True(errorsElement.TryGetProperty(error.Key, out var fieldErrors));
            Assert.Equal(JsonValueKind.Array, fieldErrors.ValueKind);
                
            for (var i = 0; i < error.Value.Length; i++)
            {
                Assert.Equal(error.Value[i], fieldErrors[i].GetString());
            }
        }
    }
        
    [Fact]
    public async Task InvokeAsync_WithException_ShouldLogError()
    {
        // Arrange
        var context = new DefaultHttpContext
        {
            Response =
            {
                Body = new MemoryStream()
            }
        };

        var exception = new Exception("Test exception");
            
        _nextMock
            .Setup(next => next(context))
            .ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}