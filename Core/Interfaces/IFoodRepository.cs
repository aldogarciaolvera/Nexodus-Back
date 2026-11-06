using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Core.Interfaces;

public interface IFoodRepository
{
    Task<(List<Food> Items, int TotalCount)> SearchAsync(string query, Guid? userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Food?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
    Task<Food?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Food> CreateAsync(Food food, CancellationToken cancellationToken = default);
}
