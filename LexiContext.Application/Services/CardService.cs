using FluentValidation;
using LexiContext.Application.DTOs.Cards;
using LexiContext.Application.Interfaces;
using LexiContext.Application.Services.Interfaces;
using LexiContext.Domain.Entities;
using LexiContext.Domain.Exceptions;

namespace LexiContext.Application.Services
{
    public class CardService : ICardService
    {
        private readonly ICardRepository _cardRepository;
        private readonly IDeckRepository _deckRepository;

        private readonly IValidator<CreateCardDto> _createCardValidator;
        private readonly IValidator<UpdateCardDto> _updateCardValidator;    

        public CardService(ICardRepository cardRepository,
            IDeckRepository deckRepository,
            IValidator<UpdateCardDto> updateCardValidator,
            IValidator<CreateCardDto> createCardValidator)
        {
            _cardRepository = cardRepository;
            _deckRepository = deckRepository;
            _updateCardValidator = updateCardValidator;
            _createCardValidator = createCardValidator;
        }
        public async Task<CardDto> CreateCardAsync(CreateCardDto dto)
        {
            var validationResult = await _createCardValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new Domain.Exceptions.ValidationException($"Card creation failed: {errors}");
            }

            var deckExists = await _deckRepository.GetByIdAsync(dto.DeckId);
            if (deckExists == null)
            {
                throw new NotFoundException("Deck", dto.DeckId);
            }

            Card card = new Card
            {
                DeckId = dto.DeckId,
                Front = dto.Front,
                Back = dto.Back,

                GeneratedContext = dto.GeneratedContext,
                ContextTranslation = dto.ContextTranslation,
                ImageURL = dto.ImageURL,
                AdditionalMetadata = dto.AdditionalMetadata
            };

            var createdCardId = await _cardRepository.CreateAsync(card);
            card.Id = createdCardId;

            return MapToCardDto(card);
        }

        public async Task<CardDto> GetCardByIdAsync(Guid id)
        {
            var existingCard = await _cardRepository.GetByIdAsync(id);
            if (existingCard == null)
            {
                throw new NotFoundException("Card", id);
            }

            return MapToCardDto(existingCard);
        }

        public async Task<List<CardDto>> GetCardsByDeckIdAsync(Guid deckId)
        {
            var cards = await _cardRepository.GetByDeckIdAsync(deckId);

            return cards.Select(MapToCardDto).ToList();
        }

        public async Task<CardDto> UpdateCardAsync(Guid id, UpdateCardDto dto)
        {
            var validationResult = _updateCardValidator.Validate(dto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new Domain.Exceptions.ValidationException($"Card update failed: {errors}");
            }

            var existingCard = await _cardRepository.GetByIdAsync(id);
            if (existingCard == null)
            {
                throw new NotFoundException("Card", id);
            }

            existingCard.Front = dto.Front;
            existingCard.Back = dto.Back;

            existingCard.GeneratedContext = dto.GeneratedContext;
            existingCard.ContextTranslation = dto.ContextTranslation;
            existingCard.ImageURL = dto.ImageURL;
            existingCard.AdditionalMetadata = dto.AdditionalMetadata;

            existingCard.UpdatedAt = DateTime.UtcNow;

            await _cardRepository.UpdateAsync(existingCard);

            return MapToCardDto(existingCard);
        }

        public async Task DeleteCardAsync(Guid id)
        {
            var existingCard = await _cardRepository.GetByIdAsync(id);

            if (existingCard == null)
            {
                throw new NotFoundException("Card", id);
            }

            await _cardRepository.DeleteAsync(existingCard);
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
                ImageURL = card.ImageURL,
                AdditionalMetadata = card.AdditionalMetadata
            };
        }
    }
}
