using FluentValidation;
using LexiContext.Application.DTOs.Decks;
using LexiContext.Application.Interfaces;
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
            // ARRANGE

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
                .Setup(r => r.CreateAsync(It.IsAny<Domain.Entities.Deck>()))
                .ReturnsAsync(id);

            // ACT

            var result = await _deckService.CreateDeckAsync(dto);

            // ASSERT

            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal(dto.Title, result.Title);
        }

        [Fact]
        public async Task CreateDeckAsync_ShouldThrowException_WhenDataIsInvalid()
        {
            // ARRANGE

            var invalidDto = new CreateDeckDto
            {
                Title = "",
                Description = "A sample deck for testing",
                IsPublic = true,
                TargetLanguage = Domain.Enums.LearningLanguage.Spanish,
                NativeLanguage = Domain.Enums.LearningLanguage.English
            };

            var validationFailure = new FluentValidation.Results.ValidationFailure("Title", "Title is required");
            var validationResult = new FluentValidation.Results.ValidationResult(new[] { validationFailure });

            _createDeckValidatorMock
                .Setup(v => v.ValidateAsync(invalidDto, default))
                .ReturnsAsync(validationResult);

            // ACT & ASSERT

            await Assert.ThrowsAsync<Domain.Exceptions.ValidationException>(() => _deckService.CreateDeckAsync(invalidDto));

            _deckRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Domain.Entities.Deck>()), Times.Never);
        }

        [Fact]
        public async Task GetDeckByIdAsync_ShouldReturnDto_WhenDeckExists()
        {
            // ARRANGE

            var deckId = Guid.NewGuid();
            var existingDeck = new Deck { Id = deckId, Title = "Existing Deck" };

            _deckRepositoryMock
                .Setup(r => r.GetByIdAsync(deckId))
                .ReturnsAsync(existingDeck);

            // ACT

            var result = await _deckService.GetDeckByIdAsync(deckId);

            // ASSERT

            Assert.NotNull(result);
            Assert.Equal(deckId, result.Id);
        }

        [Fact]
        public async Task GetDeckByIdAsync_ShouldThrowNoFoundException_WhenDeckDoesNotExist()
        {
            // ARRANGE

            var nonExistentId = Guid.NewGuid();

            _deckRepositoryMock
                .Setup(r => r.GetByIdAsync(nonExistentId))
                .ReturnsAsync((Deck)null);

            // ACT & ASSERT

            await Assert.ThrowsAsync<NotFoundException>(() => _deckService.GetDeckByIdAsync(nonExistentId));
        }

        [Fact]
        public async Task GetAllDecksAsync_ShouldReturnListDto_WhenDeckListIsNotEmpty()
        {
            // ARRANGE

            var deckList = new List<Deck>
            {
                new Deck { Id = Guid.NewGuid(), Title = "Deck 1" },
                new Deck { Id = Guid.NewGuid(), Title = "Deck 2" }
            };

            _deckRepositoryMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(deckList);

            // ACT

            var result = await _deckService.GetAllDecksAsync();

            // ASSERT

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Deck 1", result[0].Title);
        }

        [Fact]
        public async Task GetAllDecksAsync_ShouldReturnEmptyListDto_WhenNoDecksExist()
        {
            // ARRANGE

            var emptyDeckList = new List<Deck>();

            _deckRepositoryMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(emptyDeckList);

            // ACT

            var result = await _deckService.GetAllDecksAsync();

            // ASSERT

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task UpdateDeckAsync_ShoudlReturnUpdatedDto_WhenDeckExistsAndDataIsValid()
        {
            // ARRANGE

            var existingDeckId = Guid.NewGuid();
            var existingDeck = new Deck
            {
                Id = existingDeckId,
                Title = "Old Title",
                Description = "Old Description",
                IsPublic = false,
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

            // ACT

            var result = await _deckService.UpdateDeckAsync(existingDeckId, updateDto);

            // ASSERT

            Assert.NotNull(result);
            Assert.Equal(existingDeckId, result.Id);
            Assert.Equal(updateDto.Title, result.Title);

            _deckRepositoryMock.Verify(r => r.UpdateAsync(existingDeck), Times.Once);
        }

        [Fact]
        public async Task UpdateDeckAsync_ShouldReturnNotFound_WhenDeckDoesNotExist()
        {
            // ARRANGE

            var nonExistingId = Guid.NewGuid();

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
                .Setup(r => r.GetByIdAsync(nonExistingId))
                .ReturnsAsync((Deck)null);

            // ACT & ASSERT

            await Assert.ThrowsAsync<NotFoundException>(() => _deckService.UpdateDeckAsync(nonExistingId, updateDto));

            _deckRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Deck>()), Times.Never);
        }

        [Fact]
        public async Task UpdateDeckAsync_ShouldThrowValidationException_WhenDataIsNotValid()
        {
            // ARRANGE

            var invalidDto = new UpdateDeckDto
            {
                Title = "",
                Description = "New Description",
                IsPublic = true,
            };

            var validationFailure = new FluentValidation.Results.ValidationFailure("Title", "Title is required");
            var validaitonResult = new FluentValidation.Results.ValidationResult(new[] { validationFailure });

            _updateDeckValidatorMock
                .Setup(v => v.ValidateAsync(invalidDto, default))
                .ReturnsAsync(validaitonResult);

            // ACT & ASSERT

            await Assert.ThrowsAsync<Domain.Exceptions.ValidationException>(() =>
            _deckService.UpdateDeckAsync(Guid.NewGuid(), invalidDto));
        }

        [Fact]
        public async Task DeleteDeckAsync_ShouldDeleteEntity_WhenDeckExists()
        {
            // ARRANGE

            var existingDeckId = Guid.NewGuid();
            var existingDeck = new Deck
            {
                Id = existingDeckId,
                Title = "Deck to be deleted"
            };

            _deckRepositoryMock
                .Setup(r => r.GetByIdAsync(existingDeckId))
                .ReturnsAsync(existingDeck);

            // ACT

            await _deckService.DeleteDeckAsync(existingDeckId);

            // ASSERT

            _deckRepositoryMock.Verify(r => r.DeleteAsync(existingDeck), Times.Once);
        }

        [Fact]
        public async Task DeleteDeckAsync_ShouldThrowNotFoundException_WhenDeckDoesNotExist()
        {
            // ARRANGE

            var nonExistingId = Guid.NewGuid();
            var nonExistingDeck = null as Deck;

            _deckRepositoryMock
                .Setup(r => r.GetByIdAsync(nonExistingId))
                .ReturnsAsync(nonExistingDeck);

            // ACT & ASSERT

            await Assert.ThrowsAsync<NotFoundException>(() => _deckService.DeleteDeckAsync(nonExistingId));

            _deckRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Deck>()), Times.Never);
        }
    }
}
