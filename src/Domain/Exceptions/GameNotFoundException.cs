namespace Domain.Exceptions;

public class GameNotFoundException(Guid gameId) : Exception($"Game with id {gameId} not found");