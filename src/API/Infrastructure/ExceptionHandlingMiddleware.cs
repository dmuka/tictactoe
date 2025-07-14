using System.Text.Json;
using Application.Exceptions;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace API.Infrastructure;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (GameNotFoundException ex)
        {
            logger.LogError(ex, "GameNotFoundException occurred ({Message}).", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
        catch (InvalidMoveException ex)
        {
            logger.LogError(ex, "InvalidMoveException occurred ({Message}).", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
        catch (ConcurrencyException ex)
        {
            logger.LogError(ex, "ConcurrencyException occurred ({Message}).", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            GameNotFoundException ex => (
                StatusCodes.Status404NotFound,
                "Game not found",
                ex.Message),
                
            InvalidMoveException ex => (
                StatusCodes.Status400BadRequest,
                "Invalid move",
                ex.Message),
                
            ConcurrencyException => (
                StatusCodes.Status409Conflict,
                "Concurrency conflict",
                "The game state has changed since your last request"),
                
            _ => (
                StatusCodes.Status500InternalServerError,
                "Server Error",
                "An unexpected error occurred")
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = statusCode,
            Instance = context.Request.Path
        };

        if (exception is GameValidationException validationException)
        {
            problem.Extensions["errors"] = validationException.Errors;
        }

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}