using FluentValidation;
using LexiContext.Application.DTOs.Cards.Study;

namespace LexiContext.Application.Validators
{
    public class ReviewCardDtoValidator : AbstractValidator<ReviewCardDto>
    {
        public ReviewCardDtoValidator()
        {
            RuleFor(x => x.CardId)
                .NotEmpty().WithMessage("CardId is required.");

            RuleFor(x => x.Quality)
                .IsInEnum().WithMessage("Quality must be a valid value: 1 (Fail), 2 (Hard), or 3 (Easy).");
        }
    }
}
