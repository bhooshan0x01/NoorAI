using Microsoft.EntityFrameworkCore;
using NoorAI.API.Data;
using NoorAI.API.Models;
using NoorAI.API.Repositories.Interfaces;

namespace NoorAI.API.Repositories;

public class InterviewRepository(ApplicationDbContext context) : IInterviewRepository
{
    public async Task<Interview?> GetByIdAsync(int id)
    {
        return await context.Interviews
            .Include(i => i.Questions)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task AddAsync(Interview interview)
    {
        await context.Interviews.AddAsync(interview);
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Interview>> GetAllAsync()
    {
        return await context.Interviews
            .Include(i => i.Questions)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

}