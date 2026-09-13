using System;

namespace Nexodus_Back.Application.DTOs.Finance;

public class UpdateFinanceRequest
{
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Category { get; set; }
    public DateTime? TransactionDate { get; set; }
}
