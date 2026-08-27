using System.IdentityModel.Tokens.Jwt;
using BankingApi.Api.Contracts;
using BankingApi.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BankingApi.Api.Controllers;

[ApiController, Authorize, Route("api/transactions")]
public sealed class TransactionsController(IBankingService service) : ControllerBase
{
    [HttpPost("transfer"), ProducesResponseType<TransferResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<TransferResponse>> Transfer(TransferRequest request, CancellationToken ct)
    {
        var response = await service.TransferAsync(UserId, request, ct);

        return StatusCode(StatusCodes.Status201Created, response);
    }


    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TransactionResponse>>> History(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var response = await service.GetTransactionsAsync(UserId, page, pageSize, ct);

        return Ok(response);
    }

    private string UserId => User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new UnauthorizedAccessException();
}
