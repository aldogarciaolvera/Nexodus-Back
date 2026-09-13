using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Interfaces;
using Nexodus_Back.Infrastructure.Data;

namespace Nexodus_Back.Infrastructure.Repositories;

public class FinanceRepository : IFinanceRepository
{
    private readonly NexodusDbContext _context;

    public FinanceRepository(NexodusDbContext context)
    {
        _context = context;
    }

    public async Task<Finance> AddAsync(Finance finance)
    {
        _context.Finances.Add(finance);
        await _context.SaveChangesAsync();
        return finance;
    }

    public async Task<Finance?> GetByIdAsync(Guid id)
    {
        return await _context.Finances.FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<IEnumerable<Finance>> GetAllByUserIdAsync(Guid userId)
    {
        return await _context.Finances
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.TransactionDate)
            .ToListAsync();
    }

    public async Task UpdateAsync(Finance finance)
    {
        _context.Finances.Update(finance);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Finance finance)
    {
        _context.Finances.Remove(finance);
        await _context.SaveChangesAsync();
    }
}
