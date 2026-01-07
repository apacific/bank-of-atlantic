using Banking.Application.Common.Errors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Banking.Application;

/// <summary>
/// Dependency injection configuration for the Application layer.
/// Registers MediatR, validation behaviors, and FluentValidation validators.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers application layer services to the dependency injection container.
    /// Configures MediatR with pipeline behaviors for validation.
    /// Registers all FluentValidation validators from this assembly.
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    /// <returns>The configured service collection for method chaining</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }
}

/// <summary>
/// MediatR pipeline behavior that validates requests before handling.
/// Collects validation errors from all registered validators for a request type.
/// Throws ValidationAppException if validation errors are found.
/// </summary>
/// <typeparam name="TReq">The request type</typeparam>
/// <typeparam name="TRes">The response type</typeparam>
public sealed class ValidationBehavior<TReq, TRes>(IEnumerable<IValidator<TReq>> validators)
    : IPipelineBehavior<TReq, TRes> where TReq : notnull
{
    /// <summary>
    /// Validates the request using all registered validators.
    /// Throws if validation fails; otherwise passes to the next behavior.
    /// </summary>
    /// <param name="request">The request object to validate</param>
    /// <param name="next">The next handler in the pipeline</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The handler response</returns>
    /// <exception cref="ValidationAppException">Thrown if validation fails</exception>
    public async Task<TRes> Handle(TReq request, RequestHandlerDelegate<TRes> next, CancellationToken ct)
    {
        var ctx = new ValidationContext<TReq>(request);
        var failures = validators
            .Select(v => v.Validate(ctx))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count > 0)
        {
            var errors = failures
                .GroupBy(f => f.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).Distinct().ToArray());

            throw new ValidationAppException(errors);
        }

        return await next();
    }
}
