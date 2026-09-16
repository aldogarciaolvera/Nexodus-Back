using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Finance;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Exceptions;
using Nexodus_Back.Core.Interfaces;

namespace Nexodus_Back.Application.Services;

public class FinanceService : IFinanceService
{
    private readonly IFinanceRepository _financeRepository;

    public FinanceService(IFinanceRepository financeRepository)
    {
        _financeRepository = financeRepository;
    }

    public async Task<FinanceDto> CreateAsync(Guid userId, CreateFinanceRequest request)
    {
        var finance = new Finance
        {
            UserId = userId,
            TransactionType = request.TransactionType,
            Amount = request.Amount,
            CategoryId = request.CategoryId,
            TransactionDate = request.TransactionDate ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdFinance = await _financeRepository.AddAsync(finance);

        return MapToDto(createdFinance);
    }

    public async Task<FinanceDto> GetByIdAsync(Guid userId, Guid id)
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

    public async Task<IEnumerable<FinanceDto>> GetAllByUserIdAsync(Guid userId)
    {
        var finances = await _financeRepository.GetAllByUserIdAsync(userId);
        return finances.Select(MapToDto);
    }

    public async Task<FinanceSummaryDto> GetSummaryAsync(Guid userId)
    {
        var finances = await _financeRepository.GetAllByUserIdAsync(userId);
        
        var totalIncome = finances.Where(f => f.TransactionType == "Ingreso").Sum(f => f.Amount);
        var totalExpense = finances.Where(f => f.TransactionType == "Gasto").Sum(f => f.Amount);

        return new FinanceSummaryDto
        {
            TotalIncome = totalIncome,
            TotalExpense = totalExpense
        };
    }

    public async Task<FinanceDto> UpdateAsync(Guid userId, Guid id, UpdateFinanceRequest request)
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
        finance.CategoryId = request.CategoryId;
        if (request.TransactionDate.HasValue)
        {
            finance.TransactionDate = request.TransactionDate.Value;
        }
        finance.UpdatedAt = DateTime.UtcNow;

        await _financeRepository.UpdateAsync(finance);

        return MapToDto(finance);
    }

    public async Task DeleteAsync(Guid userId, Guid id)
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
            CategoryId = finance.CategoryId,
            TransactionDate = finance.TransactionDate,
            CreatedAt = finance.CreatedAt
        };
    }
}
