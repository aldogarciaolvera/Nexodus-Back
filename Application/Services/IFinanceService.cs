using System.Collections.Generic;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Finance;

namespace Nexodus_Back.Application.Services;

public interface IFinanceService
{
    Task<FinanceDto> CreateAsync(Guid userId, CreateFinanceRequest request);
    Task<FinanceDto> GetByIdAsync(Guid userId, Guid id);
    Task<IEnumerable<FinanceDto>> GetAllByUserIdAsync(Guid userId);
    Task<FinanceSummaryDto> GetSummaryAsync(Guid userId);
    Task<FinanceDto> UpdateAsync(Guid userId, Guid id, UpdateFinanceRequest request);
    Task DeleteAsync(Guid userId, Guid id);
}
