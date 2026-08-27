namespace BankingApi.Api.Options;

public sealed class BankingOptions
{
    public const string SectionName = "Banking";
    public decimal OpeningBalance { get; init; }
}
