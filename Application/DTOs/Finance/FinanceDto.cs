using System;

namespace Nexodus_Back.Application.DTOs.Finance;

public class FinanceDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
