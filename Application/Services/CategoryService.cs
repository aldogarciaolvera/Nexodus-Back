using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Category;
using Nexodus_Back.Application.Interfaces;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Exceptions;
using Nexodus_Back.Core.Interfaces;

namespace Nexodus_Back.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryDto> CreateAsync(Guid userId, CreateCategoryRequest request)
    {
        var existingCategory = await _categoryRepository.GetByNameAsync(userId, request.Name);
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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdCategory = await _categoryRepository.AddAsync(category);

        return MapToDto(createdCategory);
    }

    public async Task<CategoryDto> GetByIdAsync(Guid userId, Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

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

    public async Task<IEnumerable<CategoryDto>> GetAllByUserIdAsync(Guid userId)
    {
        var categories = await _categoryRepository.GetByUserIdAsync(userId);
        return categories.Select(MapToDto);
    }

    public async Task<CategoryDto> UpdateAsync(Guid userId, Guid id, UpdateCategoryRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        if (category == null)
        {
            throw new NotFoundException($"Categoría con id {id} no encontrada.");
        }

        if (category.UserId != userId)
        {
            throw new UnauthorizedException("No estás autorizado para modificar este registro.");
        }

        // Si cambia el nombre, validar que no exista
        if (category.Name.ToLower() != request.Name.ToLower())
        {
            var existingCategory = await _categoryRepository.GetByNameAsync(userId, request.Name);
            if (existingCategory != null)
            {
                throw new ConflictException($"La categoría con el nombre '{request.Name}' ya existe.");
            }
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.MonthlyLimit = request.MonthlyLimit;
        category.UpdatedAt = DateTime.UtcNow;

        await _categoryRepository.UpdateAsync(category);

        return MapToDto(category);
    }

    public async Task DeleteAsync(Guid userId, Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        if (category == null)
        {
            throw new NotFoundException($"Categoría con id {id} no encontrada.");
        }

        if (category.UserId != userId)
        {
            throw new UnauthorizedException("No estás autorizado para eliminar este registro.");
        }

        await _categoryRepository.DeleteAsync(category);
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
            CreatedAt = category.CreatedAt
        };
    }
}
