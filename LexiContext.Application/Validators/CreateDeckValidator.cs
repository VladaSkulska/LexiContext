using FluentValidation;
using LexiContext.Application.DTOs.Decks;

namespace LexiContext.Application.Validators
{
    public class CreateDeckValidator : AbstractValidator<CreateDeckDto>
    {
        public CreateDeckValidator() 
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Deck Title can't be null.")
                .MaximumLength(100).WithMessage("Deck Title is too long (max 100 symbols).");
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Deck description is too long (max 500 symbols).");
            RuleFor(x => x.TargetLanguage)
                .IsInEnum().WithMessage("Invalid target language.");
            RuleFor(x => x.NativeLanguage)
                .IsInEnum().WithMessage("Invalid native language.")
                .NotEqual(x => x.TargetLanguage).WithMessage("Native and Target languages cannot be the same.");
        }
    }
}
