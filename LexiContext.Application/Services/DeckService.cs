using ValidationException = LexiContext.Domain.Exceptions.ValidationException;
using LexiContext.Application.DTOs.Decks;
using LexiContext.Application.Interfaces;
using LexiContext.Application.Services.Interfaces;
using LexiContext.Domain.Entities;
using LexiContext.Domain.Exceptions;
using FluentValidation;

namespace LexiContext.Application.Services
{
    public class DeckService : IDeckService
    {
        private readonly IDeckRepository _deckRepository;

        private readonly IValidator<CreateDeckDto> _createDeckValidator;
        private readonly IValidator<UpdateDeckDto> _updateDeckValidator;

        public DeckService(IDeckRepository deckRepository, 
            IValidator<CreateDeckDto> createValidator, 
            IValidator<UpdateDeckDto> updateDeckValidator)
        {
            _deckRepository = deckRepository;
            _updateDeckValidator = updateDeckValidator;
            _createDeckValidator = createValidator;
        }
        public async Task<DeckDto> CreateDeckAsync(CreateDeckDto dto)
        {
            var validationResult = await _createDeckValidator.ValidateAsync(dto);

            if(!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException(errors);
            }

            Deck deckEntity = new Deck
            {
                Title = dto.Title,
                Description = dto.Description ?? string.Empty,
                IsPublic = dto.IsPublic,
                TargetLanguage = dto.TargetLanguage,
                NativeLanguage = dto.NativeLanguage
            };

            var createdId = await _deckRepository.CreateAsync(deckEntity);

            deckEntity.Id = createdId;

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
                throw new NotFoundException("Deck", id);

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

        public async Task<DeckDto> UpdateDeckAsync(Guid id, UpdateDeckDto dto)
        {
            var validationResult = await _updateDeckValidator.ValidateAsync(dto);

            if(!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException(errors);
            }

            var entity = await _deckRepository.GetByIdAsync(id);

            if (entity == null)
                throw new NotFoundException("Deck", id);

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

        public async Task DeleteDeckAsync(Guid id)
        {
            var entity =  await _deckRepository.GetByIdAsync(id);
            if (entity == null) 
                throw new NotFoundException("Deck", id);
            await _deckRepository.DeleteAsync(entity);
        }
    }
}
