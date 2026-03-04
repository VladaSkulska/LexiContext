using FluentValidation;
using LexiContext.Application.Common.Extensions;
using LexiContext.Application.DTOs.Cards.Study;
using LexiContext.Application.Interfaces.Repos;
using LexiContext.Application.Models;
using LexiContext.Application.Services.Interfaces;
using LexiContext.Domain.Entities;

namespace LexiContext.Application.Services
{
    public class StudyService : IStudyService
    {
        private readonly IUserCardProgressRepository _progressRepository;
        private readonly ICardRepository _cardRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISpacedRepetitionService _srsService;
        private readonly IValidator<ReviewCardDto> _reviewCardValidator;

        public StudyService(
            IUserCardProgressRepository progressRepository,
            ICardRepository cardRepository,
            IUserRepository userRepository,
            ISpacedRepetitionService srsService,
            IValidator<ReviewCardDto> reviewCardValidator)
        {
            _progressRepository = progressRepository;
            _cardRepository = cardRepository;
            _userRepository = userRepository;
            _srsService = srsService;
            _reviewCardValidator = reviewCardValidator;
        }

        public async Task<List<DueCardDto>> GetDueCardsAsync(Guid deckId, Guid userId)
        {
            var allCards = await _cardRepository.GetByDeckIdAsync(deckId);
            var userProgress = await _progressRepository.GetByDeckIdAsync(userId, deckId);

            var dueCards = new List<DueCardDto>();
            var now = DateTime.UtcNow;

            foreach (var card in allCards)
            {
                var progress = userProgress.FirstOrDefault(p => p.CardId == card.Id);

                if (progress == null || progress.NextReviewAt <= now)
                {
                    dueCards.Add(new DueCardDto
                    {
                        CardId = card.Id,
                        Front = card.Front,
                        Back = card.Back,
                        GeneratedContext = card.GeneratedContext ?? string.Empty,
                        ContextTranslation = card.ContextTranslation ?? string.Empty,
                        ContextReading = card.ContextReading ?? string.Empty,
                        ImageURL = card.ImageURL ?? string.Empty,
                        IsNew = progress == null
                    });
                }
            }

            return dueCards;
        }

        public async Task ProcessReviewAsync(Guid userId, ReviewCardDto dto)
        {
            await _reviewCardValidator.ValidateAndThrowCustomAsync(dto);

            var (progress, isNew) = await GetOrCreateProgressAsync(userId, dto.CardId);

            var srsResult = _srsService.CalculateNextReview(
                progress.Repetitions,
                progress.IntervalDays,
                progress.EaseFactor,
                dto.Quality
            );

            ApplySrsResultToProgress(progress, srsResult);

            if (isNew)
                await _progressRepository.CreateAsync(progress);
            else
                await _progressRepository.UpdateAsync(progress);

            await UpdateUserGamificationAsync(userId);
        }

        private async Task<(UserCardProgress Progress, bool IsNew)> GetOrCreateProgressAsync(Guid userId, Guid cardId)
        {
            var progress = await _progressRepository.GetByCardIdAsync(userId, cardId);
            if (progress != null)
                return (progress, false);

            progress = new UserCardProgress
            {
                UserId = userId,
                CardId = cardId,
                Repetitions = 0,
                IntervalDays = 0,
                EaseFactor = 2.5,
                NextReviewAt = DateTime.UtcNow
            };

            return (progress, true);
        }

        private static void ApplySrsResultToProgress(UserCardProgress progress, SpacedRepetitionResult srsResult)
        {
            progress.Repetitions = srsResult.Repetitions;
            progress.IntervalDays = srsResult.IntervalDays;
            progress.EaseFactor = srsResult.EaseFactor;
            progress.NextReviewAt = srsResult.NextReviewAt;
            progress.UpdatedAt = DateTime.UtcNow;
        }

        private async Task UpdateUserGamificationAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return;

            var today = DateTime.UtcNow.Date;
            var lastStudy = user.LastStudyDate?.Date;

            if (lastStudy == today)
            {
                user.LastStudyDate = DateTime.UtcNow;
            }
            else if (lastStudy == today.AddDays(-1))
            {
                user.CurrentStreak++;
                user.LastStudyDate = DateTime.UtcNow;
            }
            else
            {
                user.CurrentStreak = 1;
                user.LastStudyDate = DateTime.UtcNow;
            }

            await _userRepository.UpdateAsync(user);
        }
    }
}
