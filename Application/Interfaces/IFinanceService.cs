using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Finance;

namespace Nexodus_Back.Application.Interfaces;

/// <summary>
/// Defines finance management operations.
/// </summary>
public interface IFinanceService
{
    Task<FinanceDto> CreateAsync(Guid userId, CreateFinanceRequest request, CancellationToken cancellationToken = default);
    Task<FinanceDto> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<FinanceDto>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<FinanceSummaryDto> GetSummaryAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<FinanceDto> UpdateAsync(Guid userId, Guid id, UpdateFinanceRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
}
