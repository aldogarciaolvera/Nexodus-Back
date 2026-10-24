using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Core.Interfaces;

public interface IMealRepository
{
    Task<List<Meal>> GetByDateAsync(Guid userId, string date, CancellationToken cancellationToken = default);
    Task<Meal?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<Meal> CreateAsync(Meal meal, CancellationToken cancellationToken = default);
    Task<Meal> UpdateAsync(Meal meal, CancellationToken cancellationToken = default);
    Task DeleteAsync(Meal meal, CancellationToken cancellationToken = default);
}
