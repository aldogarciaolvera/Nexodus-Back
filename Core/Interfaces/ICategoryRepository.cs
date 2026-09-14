using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Core.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetByUserIdAsync(Guid userId);
    Task<Category?> GetByIdAsync(Guid id);
    Task<Category?> GetByNameAsync(Guid userId, string name);
    Task<Category> AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(Category category);
}
