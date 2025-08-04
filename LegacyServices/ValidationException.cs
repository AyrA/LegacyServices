using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace LegacyServices;

internal class ValidationException : Exception
{
    public ValidationException() : this("Validation failed")
    {
        //NOOP
    }

    public ValidationException(string? message) : base(message)
    {
        //NOOP
    }

    public ValidationException(string? message, Exception? innerException) : base(message, innerException)
    {
        //NOOP
    }

    public static void ThrowIfNull([NotNull] object? value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (Equals(value, null))
        {
            throw new ValidationException($"Property '{paramName}' cannot be null");
        }
    }
}