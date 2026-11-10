using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Core.Interfaces;

public interface IPantryRepository
{
    Task<List<PantryItem>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PantryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PantryItem> CreateAsync(PantryItem item, CancellationToken cancellationToken = default);
    Task<PantryItem> UpdateAsync(PantryItem item, CancellationToken cancellationToken = default);
    Task DeleteAsync(PantryItem item, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<PantryItem> items, CancellationToken cancellationToken = default);
}
