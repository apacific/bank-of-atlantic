using Banking.Application.Accounts.Commands;
using Banking.Application.Accounts.Queries;
using Banking.Domain.Accounts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

/// <summary>
/// API controller for managing customer accounts.
/// Provides endpoints for creating, retrieving, updating, and deleting accounts.
/// All endpoints require authentication except where explicitly marked with AllowAnonymous.
/// </summary>
[ApiController]
[Route("customers/{customerId:guid}/accounts")]
[Authorize]
public sealed class AccountsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Request body for account creation.
    /// </summary>
    public sealed record CreateAccountRequest(string AccountType);

    /// <summary>
    /// Creates a new account for a customer.
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <param name="body">Account creation request containing account type</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>201 Created with the newly created account details</returns>
    /// <response code="201">Account created successfully</response>
    /// <response code="400">Validation error (invalid account type)</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Customer not found</response>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromRoute(Name = "customerId")] Guid customerId,
        [FromBody] CreateAccountRequest body,
        CancellationToken ct)
    {
        if (!Enum.TryParse<AccountType>(body.AccountType, ignoreCase: true, out var at))
            return BadRequest(new
            {
                title = "Validation error",
                status = 400,
                detail = "Validation failed",
                errors = new Dictionary<string, string[]> { ["AccountType"] = ["Account type is invalid."] }
            });

        var created = await mediator.Send(new CreateAccountCommand(customerId, at), ct);

        return CreatedAtAction(nameof(GetById), new { customerId, accountId = created.Id }, created);
    }

    /// <summary>
    /// Retrieves a specific account by ID.
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <param name="accountId">The account ID to retrieve</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Account details</returns>
    /// <response code="200">Account found and returned</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Account or customer not found</response>
    [HttpGet("{accountId:guid}")]
    public async Task<IActionResult> GetById(
        [FromRoute(Name = "customerId")] Guid customerId,
        [FromRoute] Guid accountId,
        CancellationToken ct)
        => Ok(await mediator.Send(new GetAccountByIdQuery(customerId, accountId), ct));

    /// <summary>
    /// Updates an account.
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <param name="accountId">The account ID to update</param>
    /// <param name="body">Update command containing new account data</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated account details</returns>
    /// <response code="200">Account updated successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Account or customer not found</response>
    [HttpPut("{accountId:guid}")]
    public async Task<IActionResult> Update(
        [FromRoute(Name = "customerId")] Guid customerId,
        [FromRoute] Guid accountId,
        [FromBody] UpdateAccountCommand body,
        CancellationToken ct)
    {
        var updated = await mediator.Send(body with { CustomerId = customerId, AccountId = accountId }, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes an account. Requires Manager role.
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <param name="accountId">The account ID to delete</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Account deleted successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden (requires Manager role)</response>
    /// <response code="404">Account or customer not found</response>
    [Authorize(Roles = "Manager")]
    [HttpDelete("{accountId:guid}")]
    public async Task<IActionResult> DeleteAccount(
        [FromRoute(Name = "customerId")] Guid customerId,
        [FromRoute] Guid accountId,
        CancellationToken ct)
    {
        await mediator.Send(new DeleteAccountCommand(customerId, accountId), ct);
        return NoContent();
    }
}
