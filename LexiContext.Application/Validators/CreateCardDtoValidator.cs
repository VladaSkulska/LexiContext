using FluentValidation;
using LexiContext.Application.DTOs.Cards;

namespace LexiContext.Application.Validators
{
    public class CreateCardDtoValidator : AbstractValidator<CreateCardDto>
    {
        public CreateCardDtoValidator() 
        { 
            RuleFor(x => x.Front)
                .NotEmpty().WithMessage("Front side (word) cannot be empty.")
                .MaximumLength(200).WithMessage("Word is too long.");

            RuleFor(x => x.Back)
                .NotEmpty()
                .Unless(x => x.GenerateAiContext)
                .WithMessage("Back side (translation) cannot be empty if AI generation is disabled.");

            RuleFor(x => x.DeckId)
                .NotEmpty().WithMessage("DeckId is required.");
        }
    }
}
