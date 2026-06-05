using FluentValidation;
using LexiContext.Application.Common.Extensions;
using LexiContext.Application.DTOs.Cards;
using LexiContext.Application.DTOs.Cards.Study;
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
        private readonly IUserCardProgressRepository _progressRepository;
        private readonly IAiContextService _aiContextService;
        private readonly ISpacedRepetitionService _srsService;
        private readonly ILogger<CardService> _logger;
        private readonly IValidator<CreateCardDto> _createCardValidator;
        private readonly IValidator<UpdateCardDto> _updateCardValidator;

        public CardService(ICardRepository cardRepository,
            IDeckRepository deckRepository,
            IUserCardProgressRepository progressRepository,
            ISpacedRepetitionService srsService,
            IValidator<UpdateCardDto> updateCardValidator,
            IValidator<CreateCardDto> createCardValidator,
            IAiContextService aiContextService,
            ILogger<CardService> logger)
        {
            _cardRepository = cardRepository;
            _deckRepository = deckRepository;
            _progressRepository = progressRepository;
            _srsService = srsService;
            _updateCardValidator = updateCardValidator;
            _createCardValidator = createCardValidator;
            _aiContextService = aiContextService;
            _logger = logger;
        }

        public async Task<CardDto> CreateCardAsync(CreateCardDto dto, Guid userId)
        {
            await _createCardValidator.ValidateAndThrowCustomAsync(dto);
            var deck = await GetDeckOrThrowAsync(dto.DeckId, userId);

            var cleanFront = CleanString(dto.Front);

            if (!dto.GenerateAiContext && (deck.TargetLanguage == LearningLanguage.Chinese || deck.TargetLanguage == LearningLanguage.Japanese))
            {
                cleanFront = await _aiContextService.FormatAsianWordAsync(cleanFront, deck.TargetLanguage);
            }
            // ================================================

            var exists = await _cardRepository.ExistsAsync(dto.DeckId, cleanFront.ToLower());
            if (exists)
            {
                throw new Domain.Exceptions.ValidationException($"Word or phrase '{cleanFront}' already exists in this deck.");
            }

            var card = new Card
            {
                DeckId = dto.DeckId,
                Front = cleanFront,
                Back = CleanString(dto.Back),
                GeneratedContext = CleanString(dto.GeneratedContext),
                ContextTranslation = CleanString(dto.ContextTranslation),
                ContextReading = CleanString(dto.ContextReading),
                ImageURL = CleanString(dto.ImageURL),
                AdditionalMetadata = CleanString(dto.AdditionalMetadata),
                IsSimplified = false
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

        public async Task<List<DueCardDto>> GetCardsForStudyAsync(Guid deckId, Guid userId)
        {
            var deck = await GetDeckOrThrowAsync(deckId, userId);

            var allCards = await _cardRepository.GetByDeckIdAsync(deckId);
            var userProgresses = await _progressRepository.GetByDeckIdAsync(userId, deckId);

            var reviewCards = new List<DueCardDto>();
            var newCards = new List<DueCardDto>();

            foreach (var card in allCards)
            {
                var progress = userProgresses.FirstOrDefault(p => p.CardId == card.Id);
                bool isNew = progress == null || progress.Repetitions == 0;

                var dueCardDto = new DueCardDto
                {
                    CardId = card.Id,
                    Front = card.Front,
                    Back = card.Back,
                    GeneratedContext = card.GeneratedContext ?? string.Empty,
                    ContextTranslation = card.ContextTranslation ?? string.Empty,
                    ContextReading = card.ContextReading ?? string.Empty,
                    ImageURL = card.ImageURL ?? string.Empty,
                    IsNew = isNew
                };

                if (isNew)
                {
                    newCards.Add(dueCardDto);
                }
                else if (progress != null && progress.NextReviewAt <= DateTime.UtcNow)
                {
                    reviewCards.Add(dueCardDto);
                }
            }

            var studyCards = new List<DueCardDto>();

            studyCards.AddRange(reviewCards.OrderBy(c => c.CardId).Take(deck.DailyReviewLimit));
            studyCards.AddRange(newCards.OrderBy(c => c.CardId).Take(deck.DailyNewCardsLimit));

            return studyCards;
        }

        public async Task ReviewCardAsync(ReviewCardDto dto, Guid userId)
        {
            var card = await GetCardOrThrowAsync(dto.CardId, userId);

            var progress = await _progressRepository.GetByCardIdAsync(userId, dto.CardId);
            bool isNew = false;

            if (progress == null)
            {
                progress = new UserCardProgress
                {
                    UserId = userId,
                    CardId = dto.CardId,
                    Repetitions = 0,
                    EaseFactor = 2.5,
                    IntervalDays = 0,
                    NextReviewAt = DateTime.UtcNow
                };
                isNew = true;
            }

            var srsResult = _srsService.CalculateNextReview(
                progress.Repetitions,
                progress.IntervalDays,
                progress.EaseFactor,
                dto.Quality
            );

            progress.Repetitions = srsResult.Repetitions;
            progress.IntervalDays = srsResult.IntervalDays;
            progress.EaseFactor = srsResult.EaseFactor;
            progress.NextReviewAt = srsResult.NextReviewAt;
            progress.UpdatedAt = DateTime.UtcNow;

            if (isNew)
            {
                await _progressRepository.CreateAsync(progress);
            }
            else
            {
                await _progressRepository.UpdateAsync(progress);
            }
        }

        public async Task<CardDto> UpdateCardAsync(Guid id, UpdateCardDto dto, Guid userId)
        {
            await _updateCardValidator.ValidateAndThrowCustomAsync(dto);
            var existingCard = await GetCardOrThrowAsync(id, userId);
            var deck = await GetDeckOrThrowAsync(existingCard.DeckId, userId);

            var cleanFront = CleanString(dto.Front);

            if (!dto.GenerateAiContext && (deck.TargetLanguage == LearningLanguage.Chinese || deck.TargetLanguage == LearningLanguage.Japanese))
            {
                if (existingCard.Front != cleanFront)
                {
                    cleanFront = await _aiContextService.FormatAsianWordAsync(cleanFront, deck.TargetLanguage);
                }
            }

            if (existingCard.Front.ToLower() != cleanFront.ToLower())
            {
                var exists = await _cardRepository.ExistsAsync(existingCard.DeckId, cleanFront.ToLower());
                if (exists)
                {
                    throw new Domain.Exceptions.ValidationException($"Слово або фраза '{cleanFront}' вже існує у цій колоді.");
                }
            }

            existingCard.Front = cleanFront;
            existingCard.Back = CleanString(dto.Back);
            existingCard.ImageURL = CleanString(dto.ImageURL);
            existingCard.AdditionalMetadata = CleanString(dto.AdditionalMetadata);
            existingCard.UpdatedAt = DateTime.UtcNow;

            if (dto.GenerateAiContext)
            {
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
                throw new Domain.Exceptions.ValidationException("This sentence has already been simplified.");

            if (string.IsNullOrWhiteSpace(card.GeneratedContext))
                throw new Domain.Exceptions.ValidationException("Card does not have generated context to simplify.");

            var deck = await GetDeckOrThrowAsync(card.DeckId, userId);
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
                    _logger.LogError(ex, "Failed to generate full AI context.");
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
                    _logger.LogError(ex, "Failed to perform fast translation.");
                    throw new InvalidOperationException("Fast translation failed.", ex);
                }
            }
        }

        private async Task<Deck> GetDeckOrThrowAsync(Guid deckId, Guid userId)
        {
            var deck = await _deckRepository.GetByIdAsync(deckId)
                ?? throw new NotFoundException("Deck", deckId);

            if (deck.CreatedId != userId)
                throw new UnauthorizedAccessException("You do not have access to this Deck.");

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