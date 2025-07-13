using Domain.Aggregates.Game;

namespace Infrastructure.Random;

public class RandomProvider : IRandomProvider
{
    private readonly System.Random _random = new();

    public double NextDouble() => _random.NextDouble();
}