using NoorAI.API.Models;

namespace NoorAI.API.Repositories.Interfaces;

public interface IInterviewRepository
{
    Task<Interview?> GetByIdAsync(int id);
    Task<IEnumerable<Interview>> GetAllAsync();
    Task AddAsync(Interview interview);
    Task SaveChangesAsync();
}