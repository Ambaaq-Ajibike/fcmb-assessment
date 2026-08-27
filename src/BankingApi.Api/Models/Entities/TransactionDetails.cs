namespace BankingApi.Api.Models.Entities;

public sealed class TransactionDetails
{
    public string Id { get; set; } = string.Empty;
    public string SenderAccountNumber { get; set; } = string.Empty;
    public string RecipientAccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
}
