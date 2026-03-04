using LexiContext.Domain.Entities;

namespace LexiContext.Application.Interfaces
{
    public interface IJwtProvider
    {
        string GenerateToken(User user);
    }
}
