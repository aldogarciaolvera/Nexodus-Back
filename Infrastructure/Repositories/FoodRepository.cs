using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Interfaces;
using Nexodus_Back.Infrastructure.Data;

namespace Nexodus_Back.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of food catalog repository.
/// </summary>
public class FoodRepository(NexodusDbContext context) : IFoodRepository
{
    private readonly NexodusDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<(List<Food> Items, int TotalCount)> SearchAsync(string query, Guid? userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var baseQuery = _context.Foods
            .AsNoTracking()
            .Where(f => !f.IsCustom || f.UserId == null || (userId.HasValue && f.UserId == userId.Value));

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lowerQuery = query.Trim().ToLower();
            baseQuery = baseQuery.Where(f =>
                f.Name.ToLower().Contains(lowerQuery) ||
                (f.Brand != null && f.Brand.ToLower().Contains(lowerQuery)));
        }

        // Ordenamos priorizando los alimentos personalizados del usuario y los más recientes
        var allCandidates = await baseQuery
            .OrderByDescending(f => f.IsCustom)
            .ThenByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);

        // Deduplicación en memoria para unificar copias personalizadas y del sistema
        var deduplicated = new List<Food>();
        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in allCandidates)
        {
            var key = !string.IsNullOrWhiteSpace(item.Barcode)
                ? $"barcode:{item.Barcode.Trim()}"
                : $"name:{item.Name.Trim()}::{item.Brand?.Trim() ?? string.Empty}";

            if (seenKeys.Add(key))
            {
                deduplicated.Add(item);
            }
        }

        var totalCount = deduplicated.Count;
        var pagedItems = deduplicated
            .OrderBy(f => f.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (pagedItems, totalCount);
    }

    public async Task<Food?> GetByBarcodeAsync(string barcode, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return null;

        var clean = barcode.Trim();

        // 1. Si el usuario actual tiene una personalización previa para este código de barras, priorizarla
        if (userId.HasValue)
        {
            var userCustom = await _context.Foods
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.IsCustom && f.UserId == userId.Value && f.Barcode == clean, cancellationToken);
            if (userCustom != null) return userCustom;
        }

        // 2. Si no, retornar el alimento público general del catálogo
        return await _context.Foods
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Barcode == clean, cancellationToken);
    }

    public async Task<Food?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Foods
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<Food> CreateAsync(Food food, CancellationToken cancellationToken = default)
    {
        food.Id = Guid.NewGuid();
        food.CreatedAt = DateTime.UtcNow;
        await _context.Foods.AddAsync(food, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return food;
    }

    public async Task<Food> UpdateAsync(Food food, CancellationToken cancellationToken = default)
    {
        _context.Foods.Update(food);
        await _context.SaveChangesAsync(cancellationToken);
        return food;
    }

    public async Task<Food?> GetUserCustomFoodAsync(Guid userId, string? barcode, string name, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(barcode))
        {
            var cleanBarcode = barcode.Trim();
            var matchBarcode = await _context.Foods
                .FirstOrDefaultAsync(f => f.IsCustom && f.UserId == userId && f.Barcode == cleanBarcode, cancellationToken);
            if (matchBarcode != null) return matchBarcode;
        }

        var lowerName = name.Trim().ToLower();
        return await _context.Foods
            .FirstOrDefaultAsync(f => f.IsCustom && f.UserId == userId && f.Name.ToLower() == lowerName, cancellationToken);
    }

    public async Task<Food?> GetPublicFoodAsync(string? barcode, string name, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(barcode))
        {
            var cleanBarcode = barcode.Trim();
            var matchBarcode = await _context.Foods
                .FirstOrDefaultAsync(f => (!f.IsCustom || f.UserId == null) && f.Barcode == cleanBarcode, cancellationToken);
            if (matchBarcode != null) return matchBarcode;
        }

        var lowerName = name.Trim().ToLower();
        return await _context.Foods
            .FirstOrDefaultAsync(f => (!f.IsCustom || f.UserId == null) && f.Name.ToLower() == lowerName, cancellationToken);
    }
}
