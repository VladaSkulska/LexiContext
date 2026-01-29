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
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                throw new ArgumentException("Deck Title can't be null.");
            }

            if (dto.Title.Length > 100)
            {
                throw new ArgumentException("Deck Title is too long (max 100 symbols).");
            }

            if (dto.Description?.Length > 500)
            {
                throw new ArgumentException("Deck description is too long (max 500 symbols).");
            }

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

        public async Task<DeckDto?> GetDeckByIdAsync(Guid id)
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

        public async Task<DeckDto?> UpdateDeckAsync(Guid id, UpdateDeckDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                throw new ArgumentException("Deck Title can't be null.");
            }

            if (dto.Title.Length > 100)
            {
                throw new ArgumentException("Deck Title is too long (max 100 symbols).");
            }

            if (dto.Description?.Length > 500)
            {
                throw new ArgumentException("Deck description is too long (max 500 symbols).");
            }

            var entity = await _deckRepository.GetByIdAsync(id);

            if (entity == null) return null;

            entity.Title = dto.Title;
            entity.Description = dto.Description ?? string.Empty;
            entity.IsPublic = dto.IsPublic;

            await _deckRepository.UpdateAsync(entity);

            return new DeckDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                IsPublic = entity.IsPublic,
                CreatedAt = entity.CreatedAt,
                TargetLanguage = entity.TargetLanguage,
                NativeLanguage = entity.NativeLanguage
            };
        }

        public async Task<bool> DeleteDeckAsync(Guid id)
        {
            var entity =  await _deckRepository.GetByIdAsync(id);
            if (entity == null) return false;
            await _deckRepository.DeleteAsync(entity);
            return true;
        }
    }
}
