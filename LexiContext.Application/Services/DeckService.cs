using LexiContext.Application.Common.Extensions;
using LexiContext.Application.DTOs.Decks;
using LexiContext.Application.Services.Interfaces;
using LexiContext.Domain.Entities;
using LexiContext.Domain.Exceptions;
using FluentValidation;
using LexiContext.Application.Interfaces.Repos;

namespace LexiContext.Application.Services
{
    public class DeckService : IDeckService
    {
        private readonly IDeckRepository _deckRepository;
        private readonly IValidator<CreateDeckDto> _createDeckValidator;
        private readonly IValidator<UpdateDeckDto> _updateDeckValidator;

        public DeckService(
            IDeckRepository deckRepository,
            IValidator<CreateDeckDto> createValidator,
            IValidator<UpdateDeckDto> updateDeckValidator)
        {
            _deckRepository = deckRepository;
            _createDeckValidator = createValidator;
            _updateDeckValidator = updateDeckValidator;
        }

        public async Task<DeckDto> CreateDeckAsync(CreateDeckDto dto, Guid userId)
        {
            await _createDeckValidator.ValidateAndThrowCustomAsync(dto);

            Deck deckEntity = new Deck
            {
                Title = dto.Title,
                Description = dto.Description ?? string.Empty,
                IsPublic = dto.IsPublic,
                TargetLanguage = dto.TargetLanguage,
                NativeLanguage = dto.NativeLanguage,
                ProficiencyLevel = dto.ProficiencyLevel,
                Tone = dto.Tone,
                CreatedId = userId
            };

            var createdId = await _deckRepository.CreateAsync(deckEntity);
            deckEntity.Id = createdId;

            return MapToDeckDto(deckEntity);
        }

        public async Task<DeckDto> GetDeckByIdAsync(Guid id, Guid userId)
        {
            var deckEntity = await GetDeckOrThrowAsync(id, userId);
            return MapToDeckDto(deckEntity);
        }

        public async Task<List<DeckDto>> GetAllDecksAsync(Guid userId)
        {
            var deckEntities = await _deckRepository.GetAllByUserIdAsync(userId);

            return deckEntities.Select(MapToDeckDto).ToList();
        }

        public async Task<DeckDto> UpdateDeckAsync(Guid id, UpdateDeckDto dto, Guid userId)
        {
            await _updateDeckValidator.ValidateAndThrowCustomAsync(dto);

            var entity = await GetDeckOrThrowAsync(id, userId);

            entity.Title = dto.Title;
            entity.Description = dto.Description ?? string.Empty;
            entity.IsPublic = dto.IsPublic;

            await _deckRepository.UpdateAsync(entity);

            return MapToDeckDto(entity);
        }

        public async Task DeleteDeckAsync(Guid id, Guid userId)
        {
            var entity = await GetDeckOrThrowAsync(id, userId);
            await _deckRepository.DeleteAsync(entity);
        }
        private async Task<Deck> GetDeckOrThrowAsync(Guid id, Guid userId)
        {
            var deckEntity = await _deckRepository.GetByIdAsync(id);
            if (deckEntity == null)
                throw new NotFoundException("Deck", id);

            if (deckEntity.CreatedId != userId)
                throw new UnauthorizedAccessException("Ви не маєте доступу до цієї колоди.");

            return deckEntity;
        }

        private static DeckDto MapToDeckDto(Deck deck)
        {
            return new DeckDto
            {
                Id = deck.Id,
                Title = deck.Title,
                Description = deck.Description ?? string.Empty,
                IsPublic = deck.IsPublic,
                CreatedAt = deck.CreatedAt,
                TargetLanguage = deck.TargetLanguage,
                NativeLanguage = deck.NativeLanguage,
                ProficiencyLevel = deck.ProficiencyLevel,
                Tone = deck.Tone
            };
        }
    }
}