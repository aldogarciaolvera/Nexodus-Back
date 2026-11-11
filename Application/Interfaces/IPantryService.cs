using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Diet;

namespace Nexodus_Back.Application.Interfaces;

public interface IPantryService
{
    Task<List<PantryItemDto>> GetUserPantryAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PantryItemDto> AddItemAsync(Guid userId, CreatePantryItemDto dto, CancellationToken cancellationToken = default);
    Task<PantryItemDto> UpdateItemAsync(Guid id, Guid userId, UpdatePantryItemDto dto, CancellationToken cancellationToken = default);
    Task DeleteItemAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<List<PantryItemDto>> BulkAddItemsAsync(Guid userId, List<CreatePantryItemDto> dtos, CancellationToken cancellationToken = default);
}

