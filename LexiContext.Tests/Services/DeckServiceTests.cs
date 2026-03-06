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
        private readonly Mock<IValidator<CreateDeckDto>> _createDeckValidatorMock;
        private readonly Mock<IValidator<UpdateDeckDto>> _updateDeckValidatorMock;
        private readonly IDeckService _deckService;

        private readonly Guid _testUserId = Guid.NewGuid();

        public DeckServiceTests()
        {
            _deckRepositoryMock = new Mock<IDeckRepository>();
            _createDeckValidatorMock = new Mock<IValidator<CreateDeckDto>>();
            _updateDeckValidatorMock = new Mock<IValidator<UpdateDeckDto>>();

            _deckService = new DeckService(
                _deckRepositoryMock.Object,
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
                IsPublic = true,
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
        }

        [Fact]
        public async Task CreateDeckAsync_ShouldThrowException_WhenDataIsInvalid()
        {
            var invalidDto = new CreateDeckDto
            {
                Title = "",
                TargetLanguage = Domain.Enums.LearningLanguage.Spanish,
                NativeLanguage = Domain.Enums.LearningLanguage.English
            };

            _createDeckValidatorMock
                .Setup(v => v.ValidateAsync(invalidDto, default))
                .ThrowsAsync(new Domain.Exceptions.ValidationException("Title is required"));

            await Assert.ThrowsAsync<Domain.Exceptions.ValidationException>(() =>
                _deckService.CreateDeckAsync(invalidDto, _testUserId));

            _deckRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Deck>()), Times.Never);
        }

        [Fact]
        public async Task GetDeckByIdAsync_ShouldReturnDto_WhenDeckExists()
        {
            var deckId = Guid.NewGuid();
            var existingDeck = new Deck { Id = deckId, Title = "Existing Deck", CreatedId = _testUserId };

            _deckRepositoryMock
                .Setup(r => r.GetByIdAsync(deckId))
                .ReturnsAsync(existingDeck);

            var result = await _deckService.GetDeckByIdAsync(deckId, _testUserId);

            Assert.NotNull(result);
            Assert.Equal(deckId, result.Id);
        }

        [Fact]
        public async Task GetDeckByIdAsync_ShouldThrowNoFoundException_WhenDeckDoesNotExist()
        {
            var nonExistentId = Guid.NewGuid();

            _deckRepositoryMock
                .Setup(r => r.GetByIdAsync(nonExistentId))
                .ReturnsAsync((Deck?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _deckService.GetDeckByIdAsync(nonExistentId, _testUserId));
        }

        [Fact]
        public async Task GetAllDecksAsync_ShouldReturnListDto_WhenDeckListIsNotEmpty()
        {
            var deckList = new List<Deck>
            {
                new Deck { Id = Guid.NewGuid(), Title = "Deck 1", CreatedId = _testUserId },
                new Deck { Id = Guid.NewGuid(), Title = "Deck 2", CreatedId = _testUserId }
            };

            _deckRepositoryMock
                .Setup(r => r.GetAllByUserIdAsync(_testUserId))
                .ReturnsAsync(deckList);

            var result = await _deckService.GetAllDecksAsync(_testUserId);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Deck 1", result[0].Title);
        }

        [Fact]
        public async Task GetAllDecksAsync_ShouldReturnEmptyListDto_WhenNoDecksExist()
        {
            var emptyDeckList = new List<Deck>();

            _deckRepositoryMock
                .Setup(r => r.GetAllByUserIdAsync(_testUserId))
                .ReturnsAsync(emptyDeckList);

            var result = await _deckService.GetAllDecksAsync(_testUserId);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task UpdateDeckAsync_ShoudlReturnUpdatedDto_WhenDeckExistsAndDataIsValid()
        {
            var existingDeckId = Guid.NewGuid();
            var existingDeck = new Deck
            {
                Id = existingDeckId,
                Title = "Old Title",
                Description = "Old Description",
                IsPublic = false,
                CreatedId = _testUserId
            };

            var updateDto = new UpdateDeckDto
            {
                Title = "New Title",
                Description = "New Description",
                IsPublic = true,
            };

            _updateDeckValidatorMock
                .Setup(v => v.ValidateAsync(updateDto, default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            _deckRepositoryMock
                .Setup(r => r.GetByIdAsync(existingDeckId))
                .ReturnsAsync(existingDeck);

            var result = await _deckService.UpdateDeckAsync(existingDeckId, updateDto, _testUserId);

            Assert.NotNull(result);
            Assert.Equal(existingDeckId, result.Id);
            Assert.Equal(updateDto.Title, result.Title);

            _deckRepositoryMock.Verify(r => r.UpdateAsync(existingDeck), Times.Once);
        }

        [Fact]
        public async Task UpdateDeckAsync_ShouldReturnNotFound_WhenDeckDoesNotExist()
        {
            var nonExistingId = Guid.NewGuid();
            var updateDto = new UpdateDeckDto { Title = "New Title" };

            _updateDeckValidatorMock
                .Setup(v => v.ValidateAsync(updateDto, default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            _deckRepositoryMock
                .Setup(r => r.GetByIdAsync(nonExistingId))
                .ReturnsAsync((Deck?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _deckService.UpdateDeckAsync(nonExistingId, updateDto, _testUserId));

            _deckRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Deck>()), Times.Never);
        }

        [Fact]
        public async Task UpdateDeckAsync_ShouldThrowValidationException_WhenDataIsNotValid()
        {
            var invalidDto = new UpdateDeckDto { Title = "" };

            _updateDeckValidatorMock
                .Setup(v => v.ValidateAsync(invalidDto, default))
                .ThrowsAsync(new LexiContext.Domain.Exceptions.ValidationException("Title is required"));

            await Assert.ThrowsAsync<LexiContext.Domain.Exceptions.ValidationException>(() =>
                _deckService.UpdateDeckAsync(Guid.NewGuid(), invalidDto, _testUserId));
        }

        [Fact]
        public async Task DeleteDeckAsync_ShouldDeleteEntity_WhenDeckExists()
        {
            var existingDeckId = Guid.NewGuid();
            var existingDeck = new Deck { Id = existingDeckId, Title = "Deck to be deleted", CreatedId = _testUserId };

            _deckRepositoryMock
                .Setup(r => r.GetByIdAsync(existingDeckId))
                .ReturnsAsync(existingDeck);

            await _deckService.DeleteDeckAsync(existingDeckId, _testUserId);

            _deckRepositoryMock.Verify(r => r.DeleteAsync(existingDeck), Times.Once);
        }

        [Fact]
        public async Task DeleteDeckAsync_ShouldThrowNotFoundException_WhenDeckDoesNotExist()
        {
            var nonExistingId = Guid.NewGuid();

            _deckRepositoryMock
                .Setup(r => r.GetByIdAsync(nonExistingId))
                .ReturnsAsync((Deck?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _deckService.DeleteDeckAsync(nonExistingId, _testUserId));

            _deckRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Deck>()), Times.Never);
        }
    }
}