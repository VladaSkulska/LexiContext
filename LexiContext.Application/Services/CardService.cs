using FluentValidation;
using LexiContext.Application.DTOs.Cards;
using LexiContext.Application.Interfaces;
using LexiContext.Application.Services.Interfaces;
using LexiContext.Domain.Entities;
using LexiContext.Domain.Enums;
using LexiContext.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace LexiContext.Application.Services
{
    public class CardService : ICardService
    {
        private readonly ICardRepository _cardRepository;
        private readonly IDeckRepository _deckRepository;
        private readonly IAiContextService _aiContextService;
        private readonly ILogger<CardService> _logger;

        private readonly IValidator<CreateCardDto> _createCardValidator;
        private readonly IValidator<UpdateCardDto> _updateCardValidator;

        public CardService(ICardRepository cardRepository,
            IDeckRepository deckRepository,
            IValidator<UpdateCardDto> updateCardValidator,
            IValidator<CreateCardDto> createCardValidator,
            IAiContextService aiContextService,
            ILogger<CardService> logger)
        {
            _cardRepository = cardRepository;
            _deckRepository = deckRepository;
            _updateCardValidator = updateCardValidator;
            _createCardValidator = createCardValidator;
            _aiContextService = aiContextService;
            _logger = logger;
        }

        public async Task<CardDto> CreateCardAsync(CreateCardDto dto)
        {
            var validationResult = await _createCardValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new Domain.Exceptions.ValidationException($"Card creation failed: {errors}");
            }

            var deckExists = await _deckRepository.GetByIdAsync(dto.DeckId);
            if (deckExists == null)
            {
                throw new NotFoundException("Deck", dto.DeckId);
            }

            string generatedContext = dto.GeneratedContext ?? string.Empty;
            string contextTranslation = dto.ContextTranslation ?? string.Empty;
            string contextReading = dto.ContextReading ?? string.Empty;
            string cardBack = dto.Back ?? string.Empty;

            if (dto.GenerateAiContext)
            {
                try
                {
                    var aiResult = await _aiContextService.GetAiContextAsync(
                        word: dto.Front,
                        learningLanguage: deckExists.TargetLanguage,
                        nativeLanguage: deckExists.NativeLanguage,
                        level: deckExists.ProficiencyLevel,
                        deckContext: deckExists.Title,
                        tone: deckExists.Tone
                    );

                    generatedContext = aiResult.GeneratedContext;
                    contextTranslation = aiResult.ContextTranslation;
                    contextReading = aiResult.ContextReading;

                    if (string.IsNullOrWhiteSpace(cardBack))
                    {
                        cardBack = aiResult.WordTranslation;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to generate full AI context for word '{Word}'", dto.Front);
                    throw new AiTranslationException("AI context generation failed. Card creation aborted.", ex);
                }
            }
            else if (string.IsNullOrWhiteSpace(cardBack))
            {
                try
                {
                    cardBack = await _aiContextService.TranslateWordAsync(
                        word: dto.Front,
                        learningLanguage: deckExists.TargetLanguage,
                        nativeLanguage: deckExists.NativeLanguage
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to perform fast translation for word '{Word}'", dto.Front);
                    throw new InvalidOperationException("Fast translation failed. Please enter the translation manually.", ex);
                }
            }

            // Фінальна перевірка безпеки
            if (string.IsNullOrWhiteSpace(cardBack))
            {
                throw new Domain.Exceptions.ValidationException("Translation (Back) cannot be empty. Please provide a translation.");
            }

            Card card = new Card
            {
                DeckId = dto.DeckId,
                Front = dto.Front,
                Back = cardBack, 
                GeneratedContext = generatedContext,
                ContextTranslation = contextTranslation,
                ContextReading = contextReading,
                ImageURL = dto.ImageURL,
                AdditionalMetadata = dto.AdditionalMetadata
            };

            var createdCardId = await _cardRepository.CreateAsync(card);
            card.Id = createdCardId;

            return MapToCardDto(card);
        }

        public async Task<CardDto> GetCardByIdAsync(Guid id)
        {
            var existingCard = await _cardRepository.GetByIdAsync(id);
            if (existingCard == null)
            {
                throw new NotFoundException("Card", id);
            }

            return MapToCardDto(existingCard);
        }

        public async Task<List<CardDto>> GetCardsByDeckIdAsync(Guid deckId)
        {
            var cards = await _cardRepository.GetByDeckIdAsync(deckId);
            return cards.Select(MapToCardDto).ToList();
        }

        public async Task<CardDto> UpdateCardAsync(Guid id, UpdateCardDto dto)
        {
            var validationResult = await _updateCardValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new Domain.Exceptions.ValidationException($"Card update failed: {errors}");
            }

            var existingCard = await _cardRepository.GetByIdAsync(id);
            if (existingCard == null)
            {
                throw new NotFoundException("Card", id);
            }

            if (dto.GenerateAiContext)
            {
                var deckExists = await _deckRepository.GetByIdAsync(existingCard.DeckId);

                if (deckExists == null) 
                    throw new NotFoundException("Deck", existingCard.DeckId);

                try
                {
                    var aiResult = await _aiContextService.GetAiContextAsync(
                        word: dto.Front,
                        learningLanguage: deckExists.TargetLanguage,
                        nativeLanguage: deckExists.NativeLanguage,
                        level: deckExists.ProficiencyLevel,
                        deckContext: deckExists.Title,
                        tone: deckExists.Tone
                    );

                    existingCard.GeneratedContext = aiResult.GeneratedContext;
                    existingCard.ContextTranslation = aiResult.ContextTranslation;
                    existingCard.ContextReading = aiResult.ContextReading;

                    existingCard.IsSimplified = false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to regenerate AI context for card '{Id}'", id);
                    throw new InvalidOperationException("AI context generation failed during update.", ex);
                }
            }
            else
            {
                existingCard.GeneratedContext = dto.GeneratedContext;
                existingCard.ContextTranslation = dto.ContextTranslation;
                existingCard.ContextReading = dto.ContextReading;
            }

            existingCard.Front = dto.Front;
            existingCard.Back = dto.Back;
            existingCard.ImageURL = dto.ImageURL;
            existingCard.AdditionalMetadata = dto.AdditionalMetadata;
            existingCard.UpdatedAt = DateTime.UtcNow;

            await _cardRepository.UpdateAsync(existingCard);

            return MapToCardDto(existingCard);
        }

        public async Task DeleteCardAsync(Guid id)
        {
            var existingCard = await _cardRepository.GetByIdAsync(id);

            if (existingCard == null)
            {
                throw new NotFoundException("Card", id);
            }

            await _cardRepository.DeleteAsync(existingCard);
        }

        public async Task<CardDto> SimplifyCardAsync(Guid id)
        {
            var existingCard = await _cardRepository.GetByIdAsync(id);
            
            if (existingCard == null)
                throw new NotFoundException("Card", id);

            if (existingCard.IsSimplified)
                throw new Domain.Exceptions.ValidationException("Це речення вже було спрощено. Якщо воно вам не підходить, створіть нову картку або відредагуйте текст вручну.");


            if (string.IsNullOrWhiteSpace(existingCard.GeneratedContext))
                throw new Domain.Exceptions.ValidationException("Card does not have generated context to simplify.");

            var deckExists = await _deckRepository.GetByIdAsync(existingCard.DeckId);

            if (deckExists == null)
                throw new NotFoundException("Deck", existingCard.DeckId);

            var simplerLevel = GetSimplerLevel(deckExists.ProficiencyLevel);

            var simplifiedResult = await _aiContextService.SimplifyContextAsync(
                existingCard.Front,
                existingCard.GeneratedContext,
                deckExists.TargetLanguage,
                deckExists.NativeLanguage,
                simplerLevel
            );

            existingCard.GeneratedContext = simplifiedResult.GeneratedContext;
            existingCard.ContextTranslation = simplifiedResult.ContextTranslation;
            existingCard.ContextReading = simplifiedResult.ContextReading;

            existingCard.IsSimplified = true;
            existingCard.UpdatedAt = DateTime.UtcNow;

            await _cardRepository.UpdateAsync(existingCard);

            return MapToCardDto(existingCard);
        }

        private static CardDto MapToCardDto(Card card)
        {
            return new CardDto
            {
                Id = card.Id,
                DeckId = card.DeckId,
                Front = card.Front,
                Back = card.Back,
                CreatedAt = card.CreatedAt,
                UpdatedAt = card.UpdatedAt,
                GeneratedContext = card.GeneratedContext,
                ContextTranslation = card.ContextTranslation,
                ContextReading = card.ContextReading,
                ImageURL = card.ImageURL,
                AdditionalMetadata = card.AdditionalMetadata,
                IsSimplified = card.IsSimplified
            };
        }

        private ProficiencyLevel GetSimplerLevel(ProficiencyLevel currentLevel)
        {
            return currentLevel switch
            {
                ProficiencyLevel.Advanced => ProficiencyLevel.Intermediate,
                ProficiencyLevel.Intermediate => ProficiencyLevel.Beginner,
                ProficiencyLevel.Beginner => ProficiencyLevel.Beginner,
                _ => ProficiencyLevel.Beginner
            };
        }
    }
}