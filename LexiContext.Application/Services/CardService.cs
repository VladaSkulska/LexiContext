using FluentValidation;
using LexiContext.Application.Common.Extensions;
using LexiContext.Application.DTOs.Cards;
using LexiContext.Application.Interfaces.Repos;
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

        public async Task<CardDto> CreateCardAsync(CreateCardDto dto, Guid userId)
        {
            await _createCardValidator.ValidateAndThrowCustomAsync(dto);
            var deck = await GetDeckOrThrowAsync(dto.DeckId, userId);

            var card = new Card
            {
                DeckId = dto.DeckId,
                Front = CleanString(dto.Front),
                Back = CleanString(dto.Back),
                GeneratedContext = CleanString(dto.GeneratedContext),
                ContextTranslation = CleanString(dto.ContextTranslation),
                ContextReading = CleanString(dto.ContextReading),
                ImageURL = CleanString(dto.ImageURL),
                AdditionalMetadata = CleanString(dto.AdditionalMetadata)
            };

            await ProcessAiGenerationAsync(card, deck, dto.GenerateAiContext);

            if (string.IsNullOrWhiteSpace(card.Back))
                throw new Domain.Exceptions.ValidationException("Translation (Back) cannot be empty. Please provide a translation.");

            card.Id = await _cardRepository.CreateAsync(card);
            return MapToCardDto(card);
        }

        public async Task<CardDto> GetCardByIdAsync(Guid id, Guid userId)
        {
            var card = await GetCardOrThrowAsync(id, userId);
            return MapToCardDto(card);
        }

        public async Task<List<CardDto>> GetCardsByDeckIdAsync(Guid deckId, Guid userId)
        {
            await GetDeckOrThrowAsync(deckId, userId);

            var cards = await _cardRepository.GetByDeckIdAsync(deckId);
            return cards.Select(MapToCardDto).ToList();
        }

        public async Task<CardDto> UpdateCardAsync(Guid id, UpdateCardDto dto, Guid userId)
        {
            await _updateCardValidator.ValidateAndThrowCustomAsync(dto);
            var existingCard = await GetCardOrThrowAsync(id, userId);

            existingCard.Front = CleanString(dto.Front);
            existingCard.Back = CleanString(dto.Back);
            existingCard.ImageURL = CleanString(dto.ImageURL);
            existingCard.AdditionalMetadata = CleanString(dto.AdditionalMetadata);
            existingCard.UpdatedAt = DateTime.UtcNow;

            if (dto.GenerateAiContext)
            {
                var deck = await GetDeckOrThrowAsync(existingCard.DeckId, userId);
                await ProcessAiGenerationAsync(existingCard, deck, generateSentence: true);
                existingCard.IsSimplified = false;
            }
            else
            {
                existingCard.GeneratedContext = CleanString(dto.GeneratedContext);
                existingCard.ContextTranslation = CleanString(dto.ContextTranslation);
                existingCard.ContextReading = CleanString(dto.ContextReading);
            }

            await _cardRepository.UpdateAsync(existingCard);
            return MapToCardDto(existingCard);
        }

        public async Task DeleteCardAsync(Guid id, Guid userId)
        {
            var card = await GetCardOrThrowAsync(id, userId);
            await _cardRepository.DeleteAsync(card);
        }

        public async Task<CardDto> SimplifyCardAsync(Guid id, Guid userId)
        {
            var card = await GetCardOrThrowAsync(id, userId);

            if (card.IsSimplified)
                throw new Domain.Exceptions.ValidationException("This sentence has already been simplified. If it doesn't suit you, create a new card or edit the text manually.");

            if (string.IsNullOrWhiteSpace(card.GeneratedContext))
                throw new Domain.Exceptions.ValidationException("Card does not have generated context to simplify.");

            var deck = await GetDeckOrThrowAsync(card.DeckId, userId); // 👈
            var simplerLevel = GetSimplerLevel(deck.ProficiencyLevel);

            var simplifiedResult = await _aiContextService.SimplifyContextAsync(
                card.Front, card.GeneratedContext, deck.TargetLanguage, deck.NativeLanguage, simplerLevel);

            card.GeneratedContext = simplifiedResult.GeneratedContext;
            card.ContextTranslation = simplifiedResult.ContextTranslation;
            card.ContextReading = simplifiedResult.ContextReading;
            card.IsSimplified = true;
            card.UpdatedAt = DateTime.UtcNow;

            await _cardRepository.UpdateAsync(card);
            return MapToCardDto(card);
        }

        private async Task ProcessAiGenerationAsync(Card card, Deck deck, bool generateSentence)
        {
            bool needsTranslation = string.IsNullOrWhiteSpace(card.Back);

            if (generateSentence)
            {
                try
                {
                    var aiResult = await _aiContextService.GetAiContextAsync(
                        word: card.Front,
                        learningLanguage: deck.TargetLanguage,
                        nativeLanguage: deck.NativeLanguage,
                        level: deck.ProficiencyLevel,
                        deckContext: deck.Title,
                        tone: deck.Tone
                    );

                    card.GeneratedContext = aiResult.GeneratedContext;
                    card.ContextTranslation = aiResult.ContextTranslation;
                    card.ContextReading = aiResult.ContextReading;

                    if (!string.IsNullOrWhiteSpace(aiResult.CorrectedWord))
                        card.Front = aiResult.CorrectedWord;

                    if (needsTranslation)
                        card.Back = aiResult.WordTranslation;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to generate full AI context for word '{Word}'", card.Front);
                    throw new AiTranslationException("AI context generation failed.", ex);
                }
            }
            else if (needsTranslation)
            {
                try
                {
                    var rawTranslationResult = await _aiContextService.TranslateWordAsync(
                        word: card.Front,
                        learningLanguage: deck.TargetLanguage,
                        nativeLanguage: deck.NativeLanguage
                    );

                    var parts = rawTranslationResult.Split(new[] { " - " }, 2, StringSplitOptions.TrimEntries);

                    if (parts.Length == 2)
                    {
                        card.Front = parts[0]; 
                        card.Back = parts[1];
                    }
                    else
                    {
                        card.Back = rawTranslationResult;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to perform fast translation for word '{Word}'", card.Front);
                    throw new InvalidOperationException("Fast translation failed. Please enter manually.", ex);
                }
            }
        }

        private async Task<Deck> GetDeckOrThrowAsync(Guid deckId, Guid userId)
        {
            var deck = await _deckRepository.GetByIdAsync(deckId)
                ?? throw new NotFoundException("Deck", deckId);

            if (deck.CreatedId != userId)
                throw new UnauthorizedAccessException("Ви не маєте доступу до цієї колоди.");

            return deck;
        }

        private async Task<Card> GetCardOrThrowAsync(Guid cardId, Guid userId)
        {
            var card = await _cardRepository.GetByIdAsync(cardId)
                ?? throw new NotFoundException("Card", cardId);

            await GetDeckOrThrowAsync(card.DeckId, userId);

            return card;
        }

        private static string CleanString(string? input)
        {
            return (input == "string" || string.IsNullOrWhiteSpace(input)) ? string.Empty : input.Trim();
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

        private static ProficiencyLevel GetSimplerLevel(ProficiencyLevel currentLevel)
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