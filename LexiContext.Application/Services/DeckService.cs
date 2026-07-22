using LexiContext.Application.Common.Extensions;
using LexiContext.Application.DTOs.Decks;
using LexiContext.Application.Services.Interfaces;
using LexiContext.Domain.Entities;
using LexiContext.Domain.Entities.Classes;
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
        private readonly IClassroomRepository _classroomRepository;

        private readonly IValidator<CreateDeckDto> _createDeckValidator;
        private readonly IValidator<UpdateDeckDto> _updateDeckValidator;

        public DeckService(
            IDeckRepository deckRepository,
            ICardRepository cardRepository,
            IUserCardProgressRepository progressRepository,
            IClassroomRepository classroomRepository,
            IValidator<CreateDeckDto> createValidator,
            IValidator<UpdateDeckDto> updateDeckValidator)
        {
            _deckRepository = deckRepository;
            _cardRepository = cardRepository;
            _progressRepository = progressRepository;
            _classroomRepository = classroomRepository;
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
                TargetLanguage = dto.TargetLanguage,
                NativeLanguage = dto.NativeLanguage,
                ProficiencyLevel = dto.ProficiencyLevel,
                Tone = dto.Tone,
                DailyNewCardsLimit = dto.DailyNewCardsLimit,
                DailyReviewLimit = dto.DailyReviewLimit,
                CreatedId = userId,
                OwnerClassroomId = dto.ClassroomId
            };

            var createdId = await _deckRepository.CreateAsync(deckEntity);
            deckEntity.Id = createdId;

            return MapToDeckDto(deckEntity, 0, 0, 0);
        }

        public async Task<DeckDto> GetDeckByIdAsync(Guid id, Guid userId)
        {
            var deckEntity = await GetDeckForReadAsync(id, userId);

            var cards = await _cardRepository.GetByDeckIdAsync(deckEntity.Id);
            var progresses = await _progressRepository.GetByDeckIdAsync(userId, deckEntity.Id);

            var stats = CalculateStats(cards.Count, progresses);

            return MapToDeckDto(deckEntity, stats.New, stats.Learning, stats.Review);
        }

        public async Task<List<DeckDto>> GetAllDecksAsync(Guid userId)
        {
            var deckEntities = await _deckRepository.GetPersonalDecksByUserIdAsync(userId); 
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

            var entity = await GetDeckForEditAsync(id, userId);

            entity.Title = dto.Title;
            entity.Description = dto.Description ?? string.Empty;
            entity.ProficiencyLevel = dto.ProficiencyLevel;
            entity.Tone = dto.Tone;
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
            var entity = await GetDeckForEditAsync(id, userId);

            if (entity.OwnerClassroomId == null)
            {
                bool isShared = await _classroomRepository.IsDeckSharedWithAnyClassroomAsync(id);
                if (isShared)
                {
                    throw new InvalidOperationException(
                        "You cannot delete a deck that is being used in classes. First, remove it from the class materials.");
                }
            }

            await _deckRepository.DeleteAsync(entity);
        }

        private async Task<Deck> GetDeckForReadAsync(Guid id, Guid userId)
        {
            var deckEntity = await _deckRepository.GetByIdAsync(id);
            if (deckEntity == null)
                throw new NotFoundException("Deck", id);

            if (deckEntity.CreatedId == userId)
                return deckEntity;

            if (deckEntity.OwnerClassroomId.HasValue)
            {
                var studentClassrooms = await _classroomRepository.GetStudentClassroomsAsync(userId);
                bool isStudentInClass = studentClassrooms.Any(c => c.Id == deckEntity.OwnerClassroomId.Value);

                if (isStudentInClass)
                    return deckEntity;
            }

            var sharedStudentClassrooms = await _classroomRepository.GetStudentClassroomsAsync(userId);
            bool hasAccessViaShared = sharedStudentClassrooms.Any(c => c.Decks != null && c.Decks.Any(d => d.DeckId == id));

            if (!hasAccessViaShared)
                throw new UnauthorizedAccessException("You do not have access to this deck.");

            return deckEntity;
        }

        public async Task<DeckDto> ForkDeckAsync(Guid deckId, Guid userId)
        {
            var originalDeck = await GetDeckForReadAsync(deckId, userId);

            var forkedDeck = new Deck
            {
                Title = $"{originalDeck.Title} (Copy)",
                Description = originalDeck.Description ?? string.Empty,
                TargetLanguage = originalDeck.TargetLanguage,
                NativeLanguage = originalDeck.NativeLanguage,
                ProficiencyLevel = originalDeck.ProficiencyLevel,
                Tone = originalDeck.Tone,
                DailyNewCardsLimit = originalDeck.DailyNewCardsLimit,
                DailyReviewLimit = originalDeck.DailyReviewLimit,
                CreatedId = userId,
                OwnerClassroomId = null
            };

            var newDeckId = await _deckRepository.CreateAsync(forkedDeck);
            forkedDeck.Id = newDeckId;

            var originalCards = await _cardRepository.GetByDeckIdAsync(deckId);
            foreach (var card in originalCards)
            {
                var clonedCard = new Card
                {
                    DeckId = newDeckId,
                    Front = card.Front,
                    Back = card.Back,
                    GeneratedContext = card.GeneratedContext,
                    ContextTranslation = card.ContextTranslation,
                    ContextReading = card.ContextReading,
                    ImageURL = card.ImageURL,
                    AdditionalMetadata = card.AdditionalMetadata,
                    IsSimplified = card.IsSimplified
                }; 
                await _cardRepository.CreateAsync(clonedCard);
            }

            return MapToDeckDto(forkedDeck, originalCards.Count, 0, 0);
        }

        private async Task<Deck> GetDeckForEditAsync(Guid id, Guid userId)
        {
            var deckEntity = await _deckRepository.GetByIdAsync(id);
            if (deckEntity == null)
                throw new NotFoundException("Deck", id);

            if (deckEntity.CreatedId != userId)
                throw new UnauthorizedAccessException("Only the owner can edit or delete this deck.");

            return deckEntity;
        }

        private static (int New, int Learning, int Review) CalculateStats(int totalCards, List<UserCardProgress> progresses)
        {
            var endOfToday = DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);

            int newCards = Math.Max(0, totalCards - progresses.Count);
            int learningCards = progresses.Count(p => p.IntervalDays == 0 && p.NextReviewAt <= endOfToday);
            int toReview = progresses.Count(p => p.IntervalDays > 0 && p.NextReviewAt <= endOfToday);

            return (newCards, learningCards, toReview);
        }

        private static DeckDto MapToDeckDto(Deck deck, int newCards, int learningCards, int toReview)
        {
            return new DeckDto
            {
                Id = deck.Id,
                Title = deck.Title,
                Description = deck.Description ?? string.Empty,
                ShareCode = deck.ShareCode,
                CreatedAt = deck.CreatedAt,
                CreatedId = deck.CreatedId,
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