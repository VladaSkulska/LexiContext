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
        private readonly ICardRepository _cardRepository;
        private readonly IUserCardProgressRepository _progressRepository;

        private readonly IValidator<CreateDeckDto> _createDeckValidator;
        private readonly IValidator<UpdateDeckDto> _updateDeckValidator;

        public DeckService(
            IDeckRepository deckRepository,
            ICardRepository cardRepository,
            IUserCardProgressRepository progressRepository,
            IValidator<CreateDeckDto> createValidator,
            IValidator<UpdateDeckDto> updateDeckValidator)
        {
            _deckRepository = deckRepository;
            _cardRepository = cardRepository;
            _progressRepository = progressRepository;
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
                // ДОДАНО: Зберігаємо ліміти
                DailyNewCardsLimit = dto.DailyNewCardsLimit,
                DailyReviewLimit = dto.DailyReviewLimit,
                CreatedId = userId
            };

            var createdId = await _deckRepository.CreateAsync(deckEntity);
            deckEntity.Id = createdId;

            return MapToDeckDto(deckEntity, 0, 0, 0);
        }

        public async Task<DeckDto> GetDeckByIdAsync(Guid id, Guid userId)
        {
            var deckEntity = await GetDeckOrThrowAsync(id, userId);

            var cards = await _cardRepository.GetByDeckIdAsync(deckEntity.Id);
            var progresses = await _progressRepository.GetByDeckIdAsync(userId, deckEntity.Id);

            var stats = CalculateStats(cards.Count, progresses);

            return MapToDeckDto(deckEntity, stats.New, stats.Learning, stats.Review);
        }

        public async Task<List<DeckDto>> GetAllDecksAsync(Guid userId)
        {
            var deckEntities = await _deckRepository.GetAllByUserIdAsync(userId);
            var dtos = new List<DeckDto>();

            foreach (var deck in deckEntities)
            {
                var cardsInDeck = await _cardRepository.GetByDeckIdAsync(deck.Id);
                var progresses = await _progressRepository.GetByDeckIdAsync(userId, deck.Id);

                var stats = CalculateStats(cardsInDeck.Count, progresses);

                dtos.Add(MapToDeckDto(deck, stats.New, stats.Learning, stats.Review));
            }

            return dtos;
        }

        public async Task<DeckDto> UpdateDeckAsync(Guid id, UpdateDeckDto dto, Guid userId)
        {
            await _updateDeckValidator.ValidateAndThrowCustomAsync(dto);

            var entity = await GetDeckOrThrowAsync(id, userId);

            entity.Title = dto.Title;
            entity.Description = dto.Description ?? string.Empty;
            entity.IsPublic = dto.IsPublic;
            entity.ProficiencyLevel = dto.ProficiencyLevel;
            entity.Tone = dto.Tone;
            // ДОДАНО: Оновлюємо ліміти
            entity.DailyNewCardsLimit = dto.DailyNewCardsLimit;
            entity.DailyReviewLimit = dto.DailyReviewLimit;

            await _deckRepository.UpdateAsync(entity);

            var cards = await _cardRepository.GetByDeckIdAsync(entity.Id);
            var progresses = await _progressRepository.GetByDeckIdAsync(userId, entity.Id);
            var stats = CalculateStats(cards.Count, progresses);

            return MapToDeckDto(entity, stats.New, stats.Learning, stats.Review);
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
                throw new UnauthorizedAccessException("You don't have access to this deck.");

            return deckEntity;
        }

        private static (int New, int Learning, int Review) CalculateStats(int totalCards, List<UserCardProgress> progresses)
        {
            var now = DateTime.UtcNow;

            int newCards = Math.Max(0, totalCards - progresses.Count);
            int learningCards = progresses.Count(p => p.IntervalDays == 0 && p.NextReviewAt <= now);
            int toReview = progresses.Count(p => p.IntervalDays > 0 && p.NextReviewAt <= now);

            return (newCards, learningCards, toReview);
        }

        private static DeckDto MapToDeckDto(Deck deck, int newCards, int learningCards, int toReview)
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
                Tone = deck.Tone,

                DailyNewCardsLimit = deck.DailyNewCardsLimit,
                DailyReviewLimit = deck.DailyReviewLimit,

                NewCards = newCards,
                LearningCards = learningCards,
                ToReview = toReview
            };
        }
    }
}