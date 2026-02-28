using FluentValidation;
using LexiContext.Application.DTOs.Decks;

namespace LexiContext.Application.Validators
{
    public class UpdateDeckValidator : AbstractValidator<UpdateDeckDto>
    {
        public UpdateDeckValidator() 
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Deck Title can't be null.")
                .MaximumLength(100).WithMessage("Deck Title is too long (max 100 symbols).");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Deck description is too long (max 500 symbols).");

            RuleFor(x => x.ProficiencyLevel)
                .IsInEnum().WithMessage("Invalid proficiency level.");

            RuleFor(x => x.Tone)
                .IsInEnum().WithMessage("Invalid AI tone.");
        }
    }
}
