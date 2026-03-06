using FluentValidation;
using FluentValidation.Results;
using LexiContext.Application.DTOs.Cards;
using LexiContext.Application.Interfaces.Repos;
using LexiContext.Application.Models;
using LexiContext.Application.Models.Ai;
using LexiContext.Application.Services;
using LexiContext.Application.Services.Interfaces;
using LexiContext.Domain.Entities;
using LexiContext.Domain.Enums;
using LexiContext.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LexiContext.Tests.Services
{
    public class CardServiceTests
    {
        private readonly Mock<ICardRepository> _cardRepoMock;
        private readonly Mock<IDeckRepository> _deckRepoMock;
        private readonly Mock<IAiContextService> _aiServiceMock;
        private readonly Mock<IValidator<CreateCardDto>> _createValidatorMock;
        private readonly Mock<IValidator<UpdateCardDto>> _updateValidatorMock;
        private readonly Mock<ILogger<CardService>> _loggerMock;
        private readonly CardService _cardService;

        private readonly Guid _testUserId = Guid.NewGuid();

        public CardServiceTests()
        {
            _cardRepoMock = new Mock<ICardRepository>();
            _deckRepoMock = new Mock<IDeckRepository>();
            _aiServiceMock = new Mock<IAiContextService>();
            _createValidatorMock = new Mock<IValidator<CreateCardDto>>();
            _updateValidatorMock = new Mock<IValidator<UpdateCardDto>>();
            _loggerMock = new Mock<ILogger<CardService>>();

            _createValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateCardDto>(), default))
                .ReturnsAsync(new ValidationResult());
            _updateValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateCardDto>(), default))
                .ReturnsAsync(new ValidationResult());

            _cardService = new CardService(
                _cardRepoMock.Object,
                _deckRepoMock.Object,
                _updateValidatorMock.Object,
                _createValidatorMock.Object,
                _aiServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task SimplifyCardAsync_WhenCardAlreadySimplified_ThrowsValidationException()
        {
            var cardId = Guid.NewGuid();
            var deckId = Guid.NewGuid();

            var existingCard = new Card
            {
                Id = cardId,
                DeckId = deckId,
                IsSimplified = true,
                GeneratedContext = "Some text"
            };

            var existingDeck = new Deck { Id = deckId, CreatedId = _testUserId };

            _cardRepoMock.Setup(repo => repo.GetByIdAsync(cardId)).ReturnsAsync(existingCard);
            _deckRepoMock.Setup(repo => repo.GetByIdAsync(deckId)).ReturnsAsync(existingDeck);

            var exception = await Assert.ThrowsAsync<LexiContext.Domain.Exceptions.ValidationException>(
                () => _cardService.SimplifyCardAsync(cardId, _testUserId)
            );

            Assert.Contains("This sentence has already been simplified", exception.Message);
        }

        [Fact]
        public async Task SimplifyCardAsync_WhenValidCard_SimplifiesAndUpdatesDatabase()
        {
            var cardId = Guid.NewGuid();
            var deckId = Guid.NewGuid();

            var existingCard = new Card
            {
                Id = cardId,
                DeckId = deckId,
                Front = "Bug",
                GeneratedContext = "A very long and complex sentence.",
                IsSimplified = false
            };

            var existingDeck = new Deck
            {
                Id = deckId,
                ProficiencyLevel = ProficiencyLevel.Intermediate,
                TargetLanguage = LearningLanguage.English,
                NativeLanguage = LearningLanguage.Ukrainian,
                CreatedId = _testUserId
            };

            var aiResult = new AiContextResult(
                "A simple bug.",
                "Простий баг.",
                "",
                "баг",
                ""
            );

            _cardRepoMock.Setup(repo => repo.GetByIdAsync(cardId)).ReturnsAsync(existingCard);
            _deckRepoMock.Setup(repo => repo.GetByIdAsync(deckId)).ReturnsAsync(existingDeck);

            _aiServiceMock.Setup(ai => ai.SimplifyContextAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LearningLanguage>(),
                It.IsAny<LearningLanguage>(), It.IsAny<ProficiencyLevel>()
            )).ReturnsAsync(aiResult);

            var result = await _cardService.SimplifyCardAsync(cardId, _testUserId);

            Assert.NotNull(result);
            Assert.True(result.IsSimplified);
            Assert.Equal("A simple bug.", result.GeneratedContext);

            _cardRepoMock.Verify(repo => repo.UpdateAsync(It.Is<Card>(c => c.IsSimplified == true && c.GeneratedContext == "A simple bug.")), Times.Once);
        }

        [Fact]
        public async Task UpdateCardAsync_WhenGeneratingNewAiContext_ResetsIsSimplifiedFlag()
        {
            var cardId = Guid.NewGuid();
            var deckId = Guid.NewGuid();

            var existingCard = new Card
            {
                Id = cardId,
                DeckId = deckId,
                Front = "Old Word",
                IsSimplified = true
            };

            var existingDeck = new Deck { Id = deckId, CreatedId = _testUserId };
            var updateDto = new UpdateCardDto { Front = "New Word", GenerateAiContext = true };

            var aiResult = new AiContextResult(
                "New complex context",
                "Новий контекст",
                "",
                "Нове слово",
                ""
            );

            _cardRepoMock.Setup(repo => repo.GetByIdAsync(cardId)).ReturnsAsync(existingCard);
            _deckRepoMock.Setup(repo => repo.GetByIdAsync(deckId)).ReturnsAsync(existingDeck);
            _aiServiceMock.Setup(ai => ai.GetAiContextAsync(It.IsAny<string>(), It.IsAny<LearningLanguage>(), It.IsAny<LearningLanguage>(), It.IsAny<ProficiencyLevel>(), It.IsAny<string>(), It.IsAny<AiTone>()))
                .ReturnsAsync(aiResult);

            // 👈 Передаємо _testUserId
            var result = await _cardService.UpdateCardAsync(cardId, updateDto, _testUserId);

            Assert.False(result.IsSimplified);
            _cardRepoMock.Verify(repo => repo.UpdateAsync(It.Is<Card>(c => c.IsSimplified == false)), Times.Once);
        }

        [Fact]
        public async Task CreateCardAsync_ShouldThrowValidationException_WhenDataIsInvalid()
        {
            var invalidDto = new CreateCardDto
            {
                Front = "",
                DeckId = Guid.NewGuid()
            };

            _createValidatorMock
                .Setup(v => v.ValidateAsync(invalidDto, default))
                .ThrowsAsync(new Domain.Exceptions.ValidationException("Front is required"));

            await Assert.ThrowsAsync<LexiContext.Domain.Exceptions.ValidationException>(() =>
                _cardService.CreateCardAsync(invalidDto, _testUserId));

            _cardRepoMock.Verify(r => r.CreateAsync(It.IsAny<Card>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCardAsync_ShouldThrowValidationException_WhenDataIsInvalid()
        {
            var invalidDto = new UpdateCardDto { Front = "" };

            _updateValidatorMock
                .Setup(v => v.ValidateAsync(invalidDto, default))
                .ThrowsAsync(new LexiContext.Domain.Exceptions.ValidationException("Front is required"));

            await Assert.ThrowsAsync<LexiContext.Domain.Exceptions.ValidationException>(() =>
                _cardService.UpdateCardAsync(Guid.NewGuid(), invalidDto, _testUserId));

            _cardRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
    }
}