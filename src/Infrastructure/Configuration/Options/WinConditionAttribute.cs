using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Configuration.Options;

public class WinConditionAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is not int winCondition)
        {
            return new ValidationResult("Win condition must be an integer.");
        }

        var gameSettings = (GameSettings)validationContext.ObjectInstance;
        
        if (winCondition < 3)
        {
            return new ValidationResult("Win condition must be at least 3.");
        }
        
        return winCondition > gameSettings.BoardSize 
            ? new ValidationResult("Win condition can't be greater than board size.") 
            : ValidationResult.Success!;
    }
}