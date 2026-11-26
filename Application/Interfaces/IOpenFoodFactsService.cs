using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Application.Interfaces;

/// <summary>
/// Service for querying commercial barcode products from the global Open Food Facts database.
/// </summary>
public interface IOpenFoodFactsService
{
    Task<Food?> LookupBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
}

