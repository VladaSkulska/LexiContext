using LexiContext.Domain.Entities;

namespace LexiContext.Application.Interfaces.Repos
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task UpdateAsync(User user);
        Task<User?> GetByEmailAsync(string email);
        Task CreateAsync(User user);
    }
}