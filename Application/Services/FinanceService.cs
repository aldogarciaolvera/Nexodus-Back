using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Finance;
using Nexodus_Back.Application.Interfaces;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Exceptions;
using Nexodus_Back.Core.Interfaces;

namespace Nexodus_Back.Application.Services;

/// <summary>
/// Provides personal finance management operations and financial summaries.
/// </summary>
public class FinanceService(IFinanceRepository financeRepository) : IFinanceService
{
    private readonly IFinanceRepository _financeRepository = financeRepository ?? throw new ArgumentNullException(nameof(financeRepository));

    public async Task<FinanceDto> CreateAsync(Guid userId, CreateFinanceRequest request, CancellationToken cancellationToken = default)
    {
        var finance = new Finance
        {
            UserId = userId,
            TransactionType = request.TransactionType,
            Amount = request.Amount,
            Description = request.Description,
            CategoryId = request.CategoryId,
            PaymentMethod = request.PaymentMethod,
            TransactionDate = request.TransactionDate ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdFinance = await _financeRepository.AddAsync(finance);

        return MapToDto(createdFinance);
    }

    public async Task<FinanceDto> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var finance = await _financeRepository.GetByIdAsync(id);

        if (finance == null)
        {
            throw new NotFoundException($"Registro financiero con id {id} no encontrado.");
        }

        if (finance.UserId != userId)
        {
            throw new UnauthorizedException("No estás autorizado para acceder a este registro.");
        }

        return MapToDto(finance);
    }

    public async Task<IEnumerable<FinanceDto>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var finances = await _financeRepository.GetAllByUserIdAsync(userId);
        return finances.Select(MapToDto);
    }

    public async Task<FinanceSummaryDto> GetSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var finances = await _financeRepository.GetAllByUserIdAsync(userId);
        
        var totalIncome = finances
            .Where(f => f.TransactionType.Equals("Ingreso", StringComparison.OrdinalIgnoreCase) || 
                        f.TransactionType.Equals("Income", StringComparison.OrdinalIgnoreCase))
            .Sum(f => f.Amount);

        var totalExpense = finances
            .Where(f => f.TransactionType.Equals("Gasto", StringComparison.OrdinalIgnoreCase) || 
                        f.TransactionType.Equals("Expense", StringComparison.OrdinalIgnoreCase))
            .Sum(f => f.Amount);

        return new FinanceSummaryDto
        {
            TotalIncome = totalIncome,
            TotalExpense = totalExpense
        };
    }

    public async Task<FinanceDto> UpdateAsync(Guid userId, Guid id, UpdateFinanceRequest request, CancellationToken cancellationToken = default)
    {
        var finance = await _financeRepository.GetByIdAsync(id);

        if (finance == null)
        {
            throw new NotFoundException($"Registro financiero con id {id} no encontrado.");
        }

        if (finance.UserId != userId)
        {
            throw new UnauthorizedException("No estás autorizado para modificar este registro.");
        }

        finance.TransactionType = request.TransactionType;
        finance.Amount = request.Amount;
        finance.Description = request.Description;
        finance.CategoryId = request.CategoryId;
        finance.PaymentMethod = request.PaymentMethod;
        if (request.TransactionDate.HasValue)
        {
            finance.TransactionDate = request.TransactionDate.Value;
        }
        finance.UpdatedAt = DateTime.UtcNow;

        await _financeRepository.UpdateAsync(finance);

        return MapToDto(finance);
    }

    public async Task DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var finance = await _financeRepository.GetByIdAsync(id);

        if (finance == null)
        {
            throw new NotFoundException($"Registro financiero con id {id} no encontrado.");
        }

        if (finance.UserId != userId)
        {
            throw new UnauthorizedException("No estás autorizado para eliminar este registro.");
        }

        await _financeRepository.DeleteAsync(finance);
    }

    private static FinanceDto MapToDto(Finance finance)
    {
        return new FinanceDto
        {
            Id = finance.Id,
            UserId = finance.UserId,
            TransactionType = finance.TransactionType,
            Amount = finance.Amount,
            Description = finance.Description,
            CategoryId = finance.CategoryId,
            PaymentMethod = finance.PaymentMethod,
            TransactionDate = finance.TransactionDate,
            CreatedAt = finance.CreatedAt
        };
    }
}
