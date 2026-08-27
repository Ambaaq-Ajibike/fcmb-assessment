using BankingApi.Api.Contracts;
using BankingApi.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BankingApi.Api.Controllers;

[ApiController, Route("api/auth")]
public sealed class AuthController(IAuthService service) : ControllerBase
{
    [AllowAnonymous, HttpPost("register"), ProducesResponseType<AuthResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var response = await service.RegisterAsync(request, ct);
        return CreatedAtAction(nameof(Register), new { id = response.UserId }, response);
    }

    [AllowAnonymous, HttpPost("login"), ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request,
        CancellationToken ct)
    {
        var response = await service.LoginAsync(request, ct);

        return Ok(response);
    }
}
