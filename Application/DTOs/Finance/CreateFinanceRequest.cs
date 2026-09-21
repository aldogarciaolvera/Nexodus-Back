using System;

namespace Nexodus_Back.Application.DTOs.Finance;

public class CreateFinanceRequest
{
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime? TransactionDate { get; set; }
}
