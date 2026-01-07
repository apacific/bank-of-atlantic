using Banking.Application.Common.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Net;

namespace Banking.Api.Middleware;

/// <summary>
/// Middleware that handles and normalizes all exceptions thrown during request processing.
/// Converts domain exceptions to appropriate HTTP responses in RFC 7807 Problem Details format.
/// </summary>
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    /// <summary>
    /// Processes the HTTP request and catches any exceptions.
    /// Converts exceptions to normalized problem detail responses.
    /// </summary>
    /// <param name="ctx">The HTTP context</param>
    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (ValidationAppException ex)
        {
            await WriteProblem(ctx, HttpStatusCode.BadRequest, "Validation error", ex.Message, ex.Errors);
        }
        catch (NotFoundAppException ex)
        {
            await WriteProblem(ctx, HttpStatusCode.NotFound, "Not found", ex.Message);
        }
        catch (ConflictAppException ex)
        {
            await WriteProblem(ctx, HttpStatusCode.Conflict, "Conflict", ex.Message, ex.Errors);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
        {
            await WriteProblem(ctx, HttpStatusCode.Conflict, "Conflict", "A unique constraint was violated.");
        }
        catch (NpgsqlException ex)
        {
            logger.LogError(ex, "Database unavailable");
            await WriteProblem(ctx, HttpStatusCode.ServiceUnavailable, "Service unavailable", "Database is not available.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error");
            await WriteProblem(ctx, HttpStatusCode.InternalServerError, "Server error", "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Writes a normalized problem details response to the HTTP response.
    /// Follows RFC 7807 standard for error responses.
    /// </summary>
    /// <param name="ctx">The HTTP context</param>
    /// <param name="code">HTTP status code</param>
    /// <param name="title">Error title/category</param>
    /// <param name="detail">Detailed error message</param>
    /// <param name="errors">Optional field-level validation errors</param>
    private static async Task WriteProblem(
        HttpContext ctx,
        HttpStatusCode code,
        string title,
        string detail,
        IReadOnlyDictionary<string, string[]>? errors = null)
    {
        ctx.Response.ContentType = "application/problem+json";
        ctx.Response.StatusCode = (int)code;

        var pd = new ProblemDetails
        {
            Status = (int)code,
            Title = title,
            Detail = detail,
            Instance = ctx.Request.Path
        };

        if (errors is not null) pd.Extensions["errors"] = errors;
        pd.Extensions["traceId"] = ctx.TraceIdentifier;

        await ctx.Response.WriteAsJsonAsync(pd);
    }
}
