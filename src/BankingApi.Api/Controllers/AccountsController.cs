using System.IdentityModel.Tokens.Jwt;
using BankingApi.Api.Contracts;
using BankingApi.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BankingApi.Api.Controllers;

[ApiController, Authorize, Route("api/accounts")]
public sealed class AccountsController(IBankingService service) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<AccountResponse>> Get(CancellationToken ct) => Ok(await service.GetAccountAsync(UserId, ct));
    
    [HttpPut("me")]
    public async Task<ActionResult<AccountResponse>> Update(
        UpdateProfileRequest request,
        CancellationToken ct)
    {
        var response = await service.UpdateProfileAsync(UserId, request, ct);

        return Ok(response);
    }
    private string UserId => User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new UnauthorizedAccessException();
}
