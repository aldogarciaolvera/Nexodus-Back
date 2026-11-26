using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Diet;

namespace Nexodus_Back.Application.Interfaces;

/// <summary>
/// Service providing user kitchen and pantry inventory management.
/// </summary>
public interface IPantryService
{
    /// <summary>
    /// Retrieves all ingredients present in the user's pantry.
    /// </summary>
    Task<List<PantryItemDto>> GetUserPantryAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a single ingredient item to the user's pantry.
    /// </summary>
    Task<PantryItemDto> AddItemAsync(Guid userId, CreatePantryItemDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing pantry item's quantity or details.
    /// </summary>
    Task<PantryItemDto> UpdateItemAsync(Guid id, Guid userId, UpdatePantryItemDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an ingredient from the user's pantry.
    /// </summary>
    Task DeleteItemAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple ingredients in bulk to the user's pantry.
    /// </summary>
    Task<List<PantryItemDto>> BulkAddItemsAsync(Guid userId, List<CreatePantryItemDto> dtos, CancellationToken cancellationToken = default);
}
