namespace Banking.Application.Common.Errors;

/// <summary>
/// Exception thrown when a requested resource is not found.
/// Typically results in a 404 Not Found HTTP response.
/// </summary>
public sealed class NotFoundAppException(string message) : Exception(message);

/// <summary>
/// Exception thrown when an operation conflicts with existing data.
/// Typically results in a 409 Conflict HTTP response.
/// May include field-level error details.
/// </summary>
public sealed class ConflictAppException : Exception
{
    /// <summary>
    /// Optional field-level error details mapping field names to error messages.
    /// </summary>
    public IReadOnlyDictionary<string, string[]>? Errors { get; }

    /// <summary>
    /// Creates a new conflict exception.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="errors">Optional field-level validation errors</param>
    public ConflictAppException(string message, IReadOnlyDictionary<string, string[]>? errors = null)
        : base(message) => Errors = errors;
}

/// <summary>
/// Exception thrown when input validation fails.
/// Contains field-level validation error details.
/// Typically results in a 400 Bad Request HTTP response.
/// </summary>
public sealed class ValidationAppException : Exception
{
    /// <summary>
    /// Field-level validation errors mapping field names to error message arrays.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    /// <summary>
    /// Creates a new validation exception with field-level errors.
    /// </summary>
    /// <param name="errors">Dictionary of field errors (field name to error messages)</param>
    public ValidationAppException(IReadOnlyDictionary<string, string[]> errors)
        : base("Validation failed") => Errors = errors;
}
