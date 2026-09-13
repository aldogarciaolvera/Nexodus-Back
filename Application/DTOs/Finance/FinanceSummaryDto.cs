namespace Nexodus_Back.Application.DTOs.Finance;

public class FinanceSummaryDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetBalance => TotalIncome - TotalExpense;
}
