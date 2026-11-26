using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Core.Interfaces;

/// <summary>
/// Repository interface for user finance transactions (incomes and expenses).
/// </summary>
public interface IFinanceRepository
{
    /// <summary>
    /// Persists a new financial transaction.
    /// </summary>
    Task<Finance> AddAsync(Finance finance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a financial transaction by its ID.
    /// </summary>
    Task<Finance?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all financial transactions for a user.
    /// </summary>
    Task<IEnumerable<Finance>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing financial transaction.
    /// </summary>
    Task UpdateAsync(Finance finance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a financial transaction.
    /// </summary>
    Task DeleteAsync(Finance finance, CancellationToken cancellationToken = default);
}
