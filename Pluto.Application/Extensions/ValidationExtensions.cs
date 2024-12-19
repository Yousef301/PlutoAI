using FluentValidation;

namespace Pluto.Application.Extensions;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, string> ValidPassword<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Length(8, 50).WithMessage("Your password length must be at least 8.")
            .Matches(@"[A-Z]+").WithMessage("Your password must contain at least one uppercase letter.")
            .Matches(@"[0-9]+").WithMessage("Your password must contain at least one number.");
    }

    public static IRuleBuilderOptions<T, object> ValidString<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        int minLength,
        int maxLength,
        string propertyName)
    {
        return ruleBuilder
            .Length(minLength, maxLength)
            .WithMessage(
                $"{propertyName} must be between {minLength} and {maxLength} characters long and contain only letters.");
    }
}