using LexiContext.Application.DTOs.Decks;
using LexiContext.Application.Interfaces;
using LexiContext.Application.Services.Interfaces;
using LexiContext.Domain.Entities;

namespace LexiContext.Application.Services
{
    public class DeckService : IDeckService
    {
        private readonly IDeckRepository _deckRepository;
        public DeckService(IDeckRepository deckRepository)
        {
            _deckRepository = deckRepository;
        }
        public async Task<DeckDto> CreateDeckAsync(CreateDeckDto dto)
        {
            Deck deckEntity = new Deck
            {
                Title = dto.Title,
                Description =dto.Description ?? string.Empty,
                IsPublic = dto.IsPublic,
                TargetLanguage = dto.TargetLanguage,
                NativeLanguage = dto.NativeLanguage
            };

            var newId = await _deckRepository.CreateAsync(deckEntity);

            return new DeckDto
            {
                Id = deckEntity.Id,
                Title = deckEntity.Title,
                Description = deckEntity.Description ?? string.Empty,
                IsPublic = deckEntity.IsPublic,
                CreatedAt = deckEntity.CreatedAt,
                TargetLanguage = deckEntity.TargetLanguage,
                NativeLanguage = deckEntity.NativeLanguage
            };
        }

        public async Task<DeckDto> GetDeckByIdAsync(Guid id)
        {
            var deckEntity = await _deckRepository.GetByIdAsync(id);

            if (deckEntity == null)
            {
                return null;
            }

            return new DeckDto
            {
                Id = deckEntity.Id,
                Title = deckEntity.Title,
                Description = deckEntity.Description,
                IsPublic = deckEntity.IsPublic,
                CreatedAt = deckEntity.CreatedAt,
                TargetLanguage = deckEntity.TargetLanguage,
                NativeLanguage = deckEntity.NativeLanguage
            };
        }

        public async Task<List<DeckDto>> GetAllDecksAsync()
        {
            var deckEntities = await _deckRepository.GetAllAsync();

            return deckEntities.Select(deckEntity => new DeckDto
            {
                Id = deckEntity.Id,
                Title = deckEntity.Title,
                Description = deckEntity.Description,
                IsPublic = deckEntity.IsPublic,
                CreatedAt = deckEntity.CreatedAt,
                TargetLanguage = deckEntity.TargetLanguage,
                NativeLanguage = deckEntity.NativeLanguage
            }).ToList();
        }
    }
}
