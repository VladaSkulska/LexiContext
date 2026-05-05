using FluentValidation;
using FluentValidation.Results;
using LexiContext.Application.DTOs.Cards.Study;
using LexiContext.Application.Interfaces.Repos;
using LexiContext.Application.Models;
using LexiContext.Application.Services;
using LexiContext.Application.Services.Interfaces;
using LexiContext.Domain.Entities;
using LexiContext.Domain.Enums;
using Moq;
using Xunit;

namespace LexiContext.Tests.Services
{
    public class StudyServiceTests
    {
        private readonly Mock<IUserCardProgressRepository> _progressRepoMock;
        private readonly Mock<ICardRepository> _cardRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<ISpacedRepetitionService> _srsMock;
        private readonly Mock<IValidator<ReviewCardDto>> _validatorMock;
        private readonly Mock<IUserActivityRepository> _activityRepoMock;

        private readonly StudyService _studyService;

        public StudyServiceTests()
        {
            _progressRepoMock = new Mock<IUserCardProgressRepository>();
            _cardRepoMock = new Mock<ICardRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _srsMock = new Mock<ISpacedRepetitionService>();
            _validatorMock = new Mock<IValidator<ReviewCardDto>>();

            // Ініціалізуємо новий мок
            _activityRepoMock = new Mock<IUserActivityRepository>();

            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ReviewCardDto>(), default))
                .ReturnsAsync(new ValidationResult());

            _studyService = new StudyService(
                _progressRepoMock.Object,
                _cardRepoMock.Object,
                _userRepoMock.Object,
                _srsMock.Object,
                _validatorMock.Object,
                _activityRepoMock.Object);
        }

        [Fact]
        public async Task GetDueCardsAsync_ShouldReturnOnlyNewAndDueCards()
        {
            var deckId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var card1 = new Card { Id = Guid.NewGuid(), DeckId = deckId, Front = "New Card" };
            var card2 = new Card { Id = Guid.NewGuid(), DeckId = deckId, Front = "Due Card" };

            _cardRepoMock.Setup(r => r.GetByDeckIdAsync(deckId))
                .ReturnsAsync(new List<Card> { card1, card2 });

            _progressRepoMock.Setup(r => r.GetByDeckIdAsync(userId, deckId))
                .ReturnsAsync(new List<UserCardProgress> {
                    new UserCardProgress { CardId = card2.Id, NextReviewAt = DateTime.UtcNow.AddDays(-1) }
                });

            var result = await _studyService.GetDueCardsAsync(deckId, userId);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task ProcessReviewAsync_ShouldRecordActivity()
        {
            var userId = Guid.NewGuid();
            var dto = new ReviewCardDto { CardId = Guid.NewGuid(), Quality = RecallQuality.Easy };

            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User { Id = userId });
            _srsMock.Setup(s => s.CalculateNextReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<RecallQuality>()))
                .Returns(new SpacedRepetitionResult(1, 2.5, 1, DateTime.UtcNow.AddDays(1)));

            await _studyService.ProcessReviewAsync(userId, dto);

            _activityRepoMock.Verify(r => r.GetByDateAsync(userId, It.IsAny<DateTime>()), Times.Once);
        }
    }
}