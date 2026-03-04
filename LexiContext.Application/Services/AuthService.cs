using LexiContext.Application.Interfaces;
using LexiContext.Application.Interfaces.Repos;
using LexiContext.Domain.Entities;

namespace LexiContext.Application.Services
{
    public class AuthService
    {
        private readonly IExternalAuthProvider _authProvider;
        private readonly IUserRepository _userRepository;
        private readonly IJwtProvider _jwtProvider;

        public AuthService(
            IExternalAuthProvider authProvider,
            IUserRepository userRepository,
            IJwtProvider jwtProvider)
        {
            _authProvider = authProvider;
            _userRepository = userRepository;
            _jwtProvider = jwtProvider;
        }

        public async Task<string> LoginWithGoogleAsync(string googleToken)
        {
            if (string.IsNullOrWhiteSpace(googleToken))
            {
                throw new ArgumentException("Токен Google не може бути порожнім.", nameof(googleToken));
            }

            var externalUser = await _authProvider.VerifyTokenAsync(googleToken);

            var user = await _userRepository.GetByEmailAsync(externalUser.Email);

            if (user == null)
            {
                user = new User
                {
                    Email = externalUser.Email,
                    Username = !string.IsNullOrWhiteSpace(externalUser.FirstName)
                        ? externalUser.FirstName
                        : externalUser.Email.Split('@')[0],
                    AuthProvider = _authProvider.ProviderName,
                    ExternalProviderId = externalUser.ExternalId,
                    CurrentStreak = 0,
                    LastStudyDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _userRepository.CreateAsync(user);
            }
            else
            {
                user.LastStudyDate = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
            }

            return _jwtProvider.GenerateToken(user);
        }
    }
}