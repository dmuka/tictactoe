using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using API;
using API.Dtos;
using Application.DTOs;
using Domain.Aggregates.Game;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace IntegrationTests.API.Controllers;

public class GamesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    private readonly MakeMoveRequest _moveXPlayerRow0ColO;

    public GamesControllerTests(WebApplicationFactory<Program> factory)
    {
        var factory1 = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
            });
        });
        
        _client = factory1.CreateClient();
        _moveXPlayerRow0ColO = new MakeMoveRequest
        {
            Player = GameConstants.XPlayer,
            Row = 0,
            Col = 0
        };
    }

    [Fact]
    public async Task CreateGame_ReturnsCreatedGame()
    {
        // Act
        var response = await _client.PostAsync("/api/games", null);
        
        // Assert
        response.EnsureSuccessStatusCode();
        var game = await response.Content.ReadFromJsonAsync<GameDto>();
        Assert.Equal(3, game?.BoardSize);
        Assert.Equal(GameConstants.XPlayer, game?.CurrentPlayer);
    }

    [Fact]
    public async Task MakeMove_WithoutETag_ReturnsBadRequest()
    {
        // Arrange
        var createResponse = await _client.PostAsync("/api/games", null);
        var game = await createResponse.Content.ReadFromJsonAsync<GameDto>();
        
        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/games/{game?.Id}/moves", _moveXPlayerRow0ColO);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task MakeMove_WithValidETag_UpdatesGame()
    {
        // Arrange
        var createResponse = await _client.PostAsync("/api/games", null);
        var game = await createResponse.Content.ReadFromJsonAsync<GameDto>();
        
        _client.DefaultRequestHeaders.Add("If-Match", $"W/\"{game?.Version}\"");
        
        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/games/{game?.Id}/moves", _moveXPlayerRow0ColO);
        
        // Assert
        response.EnsureSuccessStatusCode();
        var updatedGame = await response.Content.ReadFromJsonAsync<GameDto>();
        Assert.Equal(GameConstants.OPlayer, updatedGame?.CurrentPlayer);
        Assert.Equal(GameConstants.XPlayer, updatedGame?.Board[0][0]);
    }

    [Fact]
    public async Task MakeMove_WithStaleETag_ReturnsConflict()
    {
        // Arrange
        var createResponse = await _client.PostAsync("/api/games", null);
        var game = await createResponse.Content.ReadFromJsonAsync<GameDto>();
        
        // First move: Version up
        var firstMove = _moveXPlayerRow0ColO;
        _client.DefaultRequestHeaders.Add("If-Match", $"W/\"{game?.Version}\"");
        await _client.PostAsJsonAsync($"/api/games/{game?.Id}/moves", firstMove);
        
        // Second move: ETag with old version
        var secondMove = new MakeMoveRequest { Player = GameConstants.OPlayer, Row = 1, Col = 1 };
        _client.DefaultRequestHeaders.Remove("If-Match");
        _client.DefaultRequestHeaders.Add("If-Match", $"W/\"{game?.Version}\"");
        
        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/games/{game?.Id}/moves", secondMove);
        
        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");
        
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task MakeMove_WhenTwoIdenticalRequestsAreMade_BothShouldSucceed()
    {
        // Arrange
        var createResponse = await _client.PostAsync("/api/Games", null);
        createResponse.EnsureSuccessStatusCode();
        
        var gameContent = await createResponse.Content.ReadAsStringAsync();
        var game = JsonSerializer.Deserialize<GameDto>(gameContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        var getResponse = await _client.GetAsync($"/api/Games/{game?.Id}");
        getResponse.EnsureSuccessStatusCode();
        var etag = getResponse.Headers.ETag?.Tag;
        
        var content = new StringContent(
            JsonSerializer.Serialize(_moveXPlayerRow0ColO),
            Encoding.UTF8,
            "application/json");
        
        var requestFirstMove = new HttpRequestMessage(HttpMethod.Post, $"/api/Games/{game?.Id}/moves")
        {
            Content = content,
            Headers = { { "If-Match", etag } }
        };
        
        var requestSecondMove = new HttpRequestMessage(HttpMethod.Post, $"/api/Games/{game?.Id}/moves")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(_moveXPlayerRow0ColO),
                Encoding.UTF8,
                "application/json"),
            Headers = { { "If-Match", etag } }
        };
        
        // Act
        var response1Task = _client.SendAsync(requestFirstMove);
        var response2Task = _client.SendAsync(requestSecondMove);
        
        await Task.WhenAll(response1Task, response2Task);
        
        var response1 = await response1Task;
        var response2 = await response2Task;
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        Assert.Equal(response1.Headers.ETag?.Tag, response2.Headers.ETag?.Tag);
    }
}