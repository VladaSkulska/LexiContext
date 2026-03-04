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

        // 1. Додаємо поле для моку валідатора
        private readonly Mock<IValidator<ReviewCardDto>> _validatorMock;

        private readonly StudyService _studyService;

        public StudyServiceTests()
        {
            _progressRepoMock = new Mock<IUserCardProgressRepository>();
            _cardRepoMock = new Mock<ICardRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _srsMock = new Mock<ISpacedRepetitionService>();

            // 2. Ініціалізуємо мок валідатора
            _validatorMock = new Mock<IValidator<ReviewCardDto>>();

            // 3. Налаштовуємо мок так, щоб він завжди казав, що DTO валідне (бо ми тестуємо не валідацію, а логіку сервісу)
            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ReviewCardDto>(), default))
                .ReturnsAsync(new ValidationResult());

            // 4. Передаємо валідатор 5-м параметром у конструктор
            _studyService = new StudyService(
                _progressRepoMock.Object,
                _cardRepoMock.Object,
                _userRepoMock.Object,
                _srsMock.Object,
                _validatorMock.Object);
        }

        [Fact]
        public async Task GetDueCardsAsync_ShouldReturnOnlyNewAndDueCards()
        {
            // Arrange
            var deckId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var card1 = new Card { Id = Guid.NewGuid(), DeckId = deckId, Front = "New Card" };
            var card2 = new Card { Id = Guid.NewGuid(), DeckId = deckId, Front = "Due Card" };
            var card3 = new Card { Id = Guid.NewGuid(), DeckId = deckId, Front = "Future Card" };

            _cardRepoMock.Setup(r => r.GetByDeckIdAsync(deckId))
                .ReturnsAsync(new List<Card> { card1, card2, card3 });

            var progressList = new List<UserCardProgress>
            {
                new UserCardProgress { CardId = card2.Id, NextReviewAt = DateTime.UtcNow.AddDays(-1) }, // Вчора
                new UserCardProgress { CardId = card3.Id, NextReviewAt = DateTime.UtcNow.AddDays(5) }   // Через 5 днів
            };

            _progressRepoMock.Setup(r => r.GetByDeckIdAsync(userId, deckId))
                .ReturnsAsync(progressList);

            // Act
            var result = await _studyService.GetDueCardsAsync(deckId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, c => c.CardId == card1.Id && c.IsNew == true);
            Assert.Contains(result, c => c.CardId == card2.Id && c.IsNew == false);
            Assert.DoesNotContain(result, c => c.CardId == card3.Id);
        }

        [Fact]
        public async Task ProcessReviewAsync_WhenStudyingConsecutiveDays_ShouldIncreaseStreak()
        {
            // Arrange: 
            var userId = Guid.NewGuid();
            var cardId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                CurrentStreak = 5,
                LastStudyDate = DateTime.UtcNow.AddDays(-1)
            };

            var dto = new ReviewCardDto { CardId = cardId, Quality = RecallQuality.Easy };

            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _progressRepoMock.Setup(r => r.GetByCardIdAsync(userId, cardId)).ReturnsAsync((UserCardProgress?)null);

            _srsMock.Setup(s => s.CalculateNextReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<RecallQuality>()))
                .Returns(new SpacedRepetitionResult(1, 2.5, 1, DateTime.UtcNow.AddDays(1)));

            // Act
            await _studyService.ProcessReviewAsync(userId, dto);

            // Assert
            Assert.Equal(6, user.CurrentStreak);
            _userRepoMock.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task ProcessReviewAsync_WhenMissedDays_ShouldResetStreakToOne()
        {
            // Arrange: 
            var userId = Guid.NewGuid();
            var cardId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                CurrentStreak = 10,
                LastStudyDate = DateTime.UtcNow.AddDays(-3)
            };

            var dto = new ReviewCardDto { CardId = cardId, Quality = RecallQuality.Hard };

            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _progressRepoMock.Setup(r => r.GetByCardIdAsync(userId, cardId)).ReturnsAsync((UserCardProgress?)null);

            _srsMock.Setup(s => s.CalculateNextReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<RecallQuality>()))
                .Returns(new SpacedRepetitionResult(1, 2.5, 1, DateTime.UtcNow.AddDays(1)));

            // Act
            await _studyService.ProcessReviewAsync(userId, dto);

            // Assert
            Assert.Equal(1, user.CurrentStreak);
            _userRepoMock.Verify(r => r.UpdateAsync(user), Times.Once);
        }
    }
}