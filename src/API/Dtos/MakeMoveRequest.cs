using System.ComponentModel.DataAnnotations;

namespace API.Dtos;

public class MakeMoveRequest
{
    [Required]
    [RegularExpression("X|O", ErrorMessage = "Player must be either X or O")]
    public required string Player { get; set; }
    
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Row must be a positive number")]
    public int Row { get; set; }
    
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Column must be a positive number")]
    public int Col { get; set; }
}