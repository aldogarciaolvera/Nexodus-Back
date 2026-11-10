using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Diet;
using Nexodus_Back.Application.Interfaces;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Exceptions;
using Nexodus_Back.Core.Interfaces;

namespace Nexodus_Back.Application.Services;

/// <summary>
/// Service managing user's home pantry inventory for smart recipe matching and shopping list generation.
/// </summary>
public class PantryService(
    IPantryRepository pantryRepo,
    IFoodRepository foodRepo,
    IUserRepository userRepo) : IPantryService
{
    private readonly IPantryRepository _pantryRepo = pantryRepo ?? throw new ArgumentNullException(nameof(pantryRepo));
    private readonly IFoodRepository _foodRepo = foodRepo ?? throw new ArgumentNullException(nameof(foodRepo));
    private readonly IUserRepository _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));

    public async Task<List<PantryItemDto>> GetUserPantryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var items = await _pantryRepo.GetByUserIdAsync(userId, cancellationToken);
        return items.Select(MapToDto).ToList();
    }

    public async Task<PantryItemDto> AddItemAsync(Guid userId, CreatePantryItemDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException("El nombre del ingrediente es obligatorio.");

        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new NotFoundException("Usuario no encontrado.");

        Food? food = null;
        if (dto.FoodId.HasValue)
        {
            food = await _foodRepo.GetByIdAsync(dto.FoodId.Value, cancellationToken);
        }

        var item = new PantryItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FoodId = dto.FoodId,
            Food = food,
            Name = dto.Name.Trim(),
            Quantity = Math.Max(0, dto.Quantity),
            Unit = string.IsNullOrWhiteSpace(dto.Unit) ? "g" : dto.Unit.Trim(),
            Category = string.IsNullOrWhiteSpace(dto.Category) ? "Other" : dto.Category.Trim(),
            ExpirationDate = dto.ExpirationDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _pantryRepo.CreateAsync(item, cancellationToken);
        return MapToDto(created);
    }

    public async Task<PantryItemDto> UpdateItemAsync(Guid id, Guid userId, UpdatePantryItemDto dto, CancellationToken cancellationToken = default)
    {
        var item = await _pantryRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Elemento de despensa '{id}' no encontrado.");

        if (item.UserId != userId)
            throw new UnauthorizedException("No tienes permisos para modificar este elemento.");

        if (!string.IsNullOrWhiteSpace(dto.Name)) item.Name = dto.Name.Trim();
        if (dto.Quantity.HasValue) item.Quantity = Math.Max(0, dto.Quantity.Value);
        if (!string.IsNullOrWhiteSpace(dto.Unit)) item.Unit = dto.Unit.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Category)) item.Category = dto.Category.Trim();
        if (dto.ExpirationDate.HasValue) item.ExpirationDate = dto.ExpirationDate.Value;

        var updated = await _pantryRepo.UpdateAsync(item, cancellationToken);
        return MapToDto(updated);
    }

    public async Task DeleteItemAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var item = await _pantryRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Elemento de despensa '{id}' no encontrado.");

        if (item.UserId != userId)
            throw new UnauthorizedException("No tienes permisos para eliminar este elemento.");

        await _pantryRepo.DeleteAsync(item, cancellationToken);
    }

    public async Task<List<PantryItemDto>> BulkAddItemsAsync(Guid userId, List<CreatePantryItemDto> dtos, CancellationToken cancellationToken = default)
    {
        var results = new List<PantryItemDto>();
        foreach (var dto in dtos)
        {
            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                var created = await AddItemAsync(userId, dto, cancellationToken);
                results.Add(created);
            }
        }
        return results;
    }

    private static PantryItemDto MapToDto(PantryItem entity) => new()
    {
        Id = entity.Id,
        UserId = entity.UserId,
        FoodId = entity.FoodId,
        Name = entity.Name,
        Quantity = entity.Quantity,
        Unit = entity.Unit,
        Category = entity.Category,
        ExpirationDate = entity.ExpirationDate,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
        CaloriesPer100g = entity.Food?.Calories,
        ProteinPer100g = entity.Food?.Protein,
        CarbsPer100g = entity.Food?.Carbs,
        FatPer100g = entity.Food?.Fat
    };
}
