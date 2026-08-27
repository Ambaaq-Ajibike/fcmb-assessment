using BankingApi.Api.Models;
using BankingApi.Api.Models.Entities;

namespace BankingApi.Api.Services;

public interface ITokenService
{
    AccessTokenResult Create(User user);
}
