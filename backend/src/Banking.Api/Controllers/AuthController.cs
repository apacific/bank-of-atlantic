using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Banking.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Banking.Api.Controllers;

/// <summary>
/// API controller for user authentication.
/// Provides endpoints for user login and token generation.
/// </summary>
[ApiController]
[Route("auth")]
public sealed class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IConfiguration cfg
) : ControllerBase
{
    /// <summary>
    /// Login request containing user credentials.
    /// </summary>
    public sealed record LoginRequest(string Email, string Password);

    /// <summary>
    /// Login response containing JWT token and user role.
    /// </summary>
    public sealed record LoginResponse(string AccessToken, string Role);

    /// <summary>
    /// Authenticates a user and returns a JWT access token.
    /// Validates email and password against the identity store.
    /// Returns the user's assigned role for authorization purposes.
    /// </summary>
    /// <param name="req">Login credentials (email and password)</param>
    /// <returns>JWT access token and user role</returns>
    /// <response code="200">Authentication successful, token returned</response>
    /// <response code="400">Invalid request format</response>
    /// <response code="401">Invalid email or password</response>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req)
    {
        var email = (req.Email ?? "").Trim().ToLowerInvariant();
        var user = await userManager.FindByEmailAsync(email);

        if (user is null) return Unauthorized(new { message = "Invalid credentials." });

        var ok = await signInManager.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: false);
        if (!ok.Succeeded) return Unauthorized(new { message = "Invalid credentials." });

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Employee";

        var jwt = cfg.GetSection("Jwt");
        var key = jwt["Key"]!;
        var issuer = jwt["Issuer"]!;
        var audience = jwt["Audience"]!;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? email),
            new(ClaimTypes.Role, role),
        };

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        return Ok(new LoginResponse(accessToken, role));
    }
}
