using Google.Apis.Auth;
using LexiContext.Application.Interfaces;
using LexiContext.Application.Models;
using Microsoft.Extensions.Configuration;

namespace LexiContext.Infrastructure.Services.Providers
{
    public class GoogleAuthProvider : IExternalAuthProvider
    {
        private readonly string _clientId;
        public GoogleAuthProvider(IConfiguration config)
        {
            _clientId = config["GoogleAuth:ClientId"]
                ?? throw new ArgumentNullException("Google ClientId is not configured.");
        }   
        public string ProviderName => "Google";

        public async Task<ExternalUserModel> VerifyTokenAsync(string token)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new[] { _clientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);

                return new ExternalUserModel
                {
                    Email = payload.Email,
                    ExternalId = payload.Subject,
                    FirstName = payload.GivenName ?? string.Empty,
                    LastName = payload.FamilyName ?? string.Empty
                };

            }
            catch (InvalidJwtException)
            {
                throw new UnauthorizedAccessException("Invalid Google token.");
            }
        }
    }
}
