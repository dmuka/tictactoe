using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Configuration.Options;

public sealed record GameSettings
{
    [Required, Range(3, 25)]
    public required int BoardSize { get; set; } 
    [Required, WinCondition]
    public required int WinCondition { get; set; }
}