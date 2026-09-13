using System.Collections.Generic;
using System.Threading.Tasks;
using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Core.Interfaces;

public interface IFinanceRepository
{
    Task<Finance> AddAsync(Finance finance);
    Task<Finance?> GetByIdAsync(Guid id);
    Task<IEnumerable<Finance>> GetAllByUserIdAsync(Guid userId);
    Task UpdateAsync(Finance finance);
    Task DeleteAsync(Finance finance);
}
