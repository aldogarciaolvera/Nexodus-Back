using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Category;
using Nexodus_Back.Application.Interfaces;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Exceptions;
using Nexodus_Back.Core.Interfaces;

namespace Nexodus_Back.Application.Services;

/// <summary>
/// Provides category management operations for categorizing financial movements.
/// </summary>
public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));

    public async Task<CategoryDto> CreateAsync(Guid userId, CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var existingCategory = await _categoryRepository.GetByNameAsync(userId, request.Name, cancellationToken);
        if (existingCategory != null)
        {
            throw new ConflictException($"La categoría con el nombre '{request.Name}' ya existe.");
        }

        var category = new Category
        {
            UserId = userId,
            Name = request.Name,
            Description = request.Description,
            MonthlyLimit = request.MonthlyLimit,
            Type = "Money",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdCategory = await _categoryRepository.AddAsync(category, cancellationToken);

        return MapToDto(createdCategory);
    }

    public async Task<CategoryDto> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);

        if (category == null)
        {
            throw new NotFoundException($"Categoría con id {id} no encontrada.");
        }

        if (category.UserId != userId)
        {
            throw new UnauthorizedException("No estás autorizado para acceder a este registro.");
        }

        return MapToDto(category);
    }

    public async Task<IEnumerable<CategoryDto>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetByUserIdAsync(userId, cancellationToken);
        return categories.Select(MapToDto);
    }

    public async Task<CategoryDto> UpdateAsync(Guid userId, Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);

        if (category == null)
        {
            throw new NotFoundException($"Categoría con id {id} no encontrada.");
        }

        if (category.UserId != userId)
        {
            throw new UnauthorizedException("No estás autorizado para modificar este registro.");
        }

        if (category.Name.ToLower() != request.Name.ToLower())
        {
            var existingCategory = await _categoryRepository.GetByNameAsync(userId, request.Name, cancellationToken);
            if (existingCategory != null)
            {
                throw new ConflictException($"La categoría con el nombre '{request.Name}' ya existe.");
            }
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.MonthlyLimit = request.MonthlyLimit;
        category.UpdatedAt = DateTime.UtcNow;

        await _categoryRepository.UpdateAsync(category, cancellationToken);

        return MapToDto(category);
    }

    public async Task DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);

        if (category == null)
        {
            throw new NotFoundException($"Categoría con id {id} no encontrada.");
        }

        if (category.UserId != userId)
        {
            throw new UnauthorizedException("No estás autorizado para eliminar este registro.");
        }

        await _categoryRepository.DeleteAsync(category, cancellationToken);
    }

    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            UserId = category.UserId,
            Name = category.Name,
            Description = category.Description,
            MonthlyLimit = category.MonthlyLimit,
            Type = category.Type ?? "Money",
            CreatedAt = category.CreatedAt
        };
    }
}
