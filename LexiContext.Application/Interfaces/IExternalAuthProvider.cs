using LexiContext.Application.Models;

namespace LexiContext.Application.Interfaces
{
    public interface IExternalAuthProvider
    {
        string ProviderName { get; }
        Task<ExternalUserModel> VerifyTokenAsync(string token);
    }
}
