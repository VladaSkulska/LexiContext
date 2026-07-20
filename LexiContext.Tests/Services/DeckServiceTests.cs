using FluentValidation;
using LexiContext.Application.DTOs.Decks;
using LexiContext.Application.Interfaces.Repos;
using LexiContext.Application.Services;
using LexiContext.Application.Services.Interfaces;
using LexiContext.Domain.Entities;
using LexiContext.Domain.Exceptions;
using Moq;

namespace LexiContext.Tests.Services
{
    public class DeckServiceTests
    {
        private readonly Mock<IDeckRepository> _deckRepositoryMock;
        private readonly Mock<ICardRepository> _cardRepositoryMock;
        private readonly Mock<IUserCardProgressRepository> _progressRepositoryMock;

        private readonly Mock<IValidator<CreateDeckDto>> _createDeckValidatorMock;
        private readonly Mock<IValidator<UpdateDeckDto>> _updateDeckValidatorMock;
        private readonly IDeckService _deckService;
        private readonly Mock<IClassroomRepository> _classroomRepositoryMock;

        private readonly Guid _testUserId = Guid.NewGuid();

        public DeckServiceTests()
        {
            _deckRepositoryMock = new Mock<IDeckRepository>();
            _cardRepositoryMock = new Mock<ICardRepository>();
            _progressRepositoryMock = new Mock<IUserCardProgressRepository>();

            _classroomRepositoryMock = new Mock<IClassroomRepository>();

            _createDeckValidatorMock = new Mock<IValidator<CreateDeckDto>>();
            _updateDeckValidatorMock = new Mock<IValidator<UpdateDeckDto>>();


            _deckService = new DeckService(
                _deckRepositoryMock.Object,
                _cardRepositoryMock.Object,
                _progressRepositoryMock.Object,
                _classroomRepositoryMock.Object,
                _createDeckValidatorMock.Object,
                _updateDeckValidatorMock.Object);
        }

        [Fact]
        public async Task CreateDeckAsync_ShouldReturnDto_WhenDataIsValid()
        {
            var dto = new CreateDeckDto
            {
                Title = "Sample Deck",
                Description = "A sample deck for testing",
                TargetLanguage = Domain.Enums.LearningLanguage.Spanish,
                NativeLanguage = Domain.Enums.LearningLanguage.English
            };

            _createDeckValidatorMock
                .Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            var id = Guid.NewGuid();
            _deckRepositoryMock
                .Setup(r => r.CreateAsync(It.IsAny<Deck>()))
                .ReturnsAsync(id);

            var result = await _deckService.CreateDeckAsync(dto, _testUserId);

            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal(dto.Title, result.Title);
            Assert.Equal(0, result.NewCards);
        }

        [Fact]
        public async Task GetDeckByIdAsync_ShouldReturnDto_WhenDeckExists()
        {
            var deckId = Guid.NewGuid();
            var existingDeck = new Deck { Id = deckId, Title = "Existing Deck", CreatedId = _testUserId };

            _deckRepositoryMock
                .Setup(r => r.GetByIdAsync(deckId))
                .ReturnsAsync(existingDeck);

            _cardRepositoryMock.Setup(r => r.GetByDeckIdAsync(deckId)).ReturnsAsync(new List<Card>());
            _progressRepositoryMock.Setup(r => r.GetByDeckIdAsync(_testUserId, deckId)).ReturnsAsync(new List<UserCardProgress>());

            var result = await _deckService.GetDeckByIdAsync(deckId, _testUserId);

            Assert.NotNull(result);
            Assert.Equal(deckId, result.Id);
        }

        [Fact]
        public async Task GetAllDecksAsync_ShouldReturnListDto_WhenDeckListIsNotEmpty()
        {
            var deckId1 = Guid.NewGuid();
            var deckId2 = Guid.NewGuid();
            var deckList = new List<Deck>
            {
                new Deck { Id = deckId1, Title = "Deck 1", CreatedId = _testUserId },
                new Deck { Id = deckId2, Title = "Deck 2", CreatedId = _testUserId }
            };

            _deckRepositoryMock
                .Setup(r => r.GetAllByUserIdAsync(_testUserId))
                .ReturnsAsync(deckList);

            _cardRepositoryMock.Setup(r => r.GetByDeckIdAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Card>());
            _progressRepositoryMock.Setup(r => r.GetByDeckIdAsync(_testUserId, It.IsAny<Guid>())).ReturnsAsync(new List<UserCardProgress>());

            var result = await _deckService.GetAllDecksAsync(_testUserId);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Deck 1", result[0].Title);
        }

        [Fact]
        public async Task UpdateDeckAsync_ShoudlReturnUpdatedDto_WhenDeckExistsAndDataIsValid()
        {
            var existingDeckId = Guid.NewGuid();
            var existingDeck = new Deck
            {
                Id = existingDeckId,
                Title = "Old Title",
                CreatedId = _testUserId
            };

            var updateDto = new UpdateDeckDto { Title = "New Title" };

            _updateDeckValidatorMock
                .Setup(v => v.ValidateAsync(updateDto, default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            _deckRepositoryMock
                .Setup(r => r.GetByIdAsync(existingDeckId))
                .ReturnsAsync(existingDeck);

            _cardRepositoryMock.Setup(r => r.GetByDeckIdAsync(existingDeckId)).ReturnsAsync(new List<Card>());
            _progressRepositoryMock.Setup(r => r.GetByDeckIdAsync(_testUserId, existingDeckId)).ReturnsAsync(new List<UserCardProgress>());

            var result = await _deckService.UpdateDeckAsync(existingDeckId, updateDto, _testUserId);

            Assert.NotNull(result);
            Assert.Equal(updateDto.Title, result.Title);
            _deckRepositoryMock.Verify(r => r.UpdateAsync(existingDeck), Times.Once);
        }

        [Fact]
        public async Task DeleteDeckAsync_ShouldDeleteEntity_WhenDeckExists()
        {
            var existingDeckId = Guid.NewGuid();
            var existingDeck = new Deck { Id = existingDeckId, Title = "To Delete", CreatedId = _testUserId };

            _deckRepositoryMock
                .Setup(r => r.GetByIdAsync(existingDeckId))
                .ReturnsAsync(existingDeck);

            await _deckService.DeleteDeckAsync(existingDeckId, _testUserId);

            _deckRepositoryMock.Verify(r => r.DeleteAsync(existingDeck), Times.Once);
        }

        [Fact]
        public async Task GetDeckByIdAsync_ShouldThrowNoFoundException_WhenDeckDoesNotExist()
        {
            var nonExistentId = Guid.NewGuid();
            _deckRepositoryMock.Setup(r => r.GetByIdAsync(nonExistentId)).ReturnsAsync((Deck?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _deckService.GetDeckByIdAsync(nonExistentId, _testUserId));
        }
    }
}