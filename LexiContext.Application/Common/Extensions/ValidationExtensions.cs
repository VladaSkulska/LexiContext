using FluentValidation;
namespace LexiContext.Application.Common.Extensions
{
    public static class ValidationExtensions
    {
        public static async Task ValidateAndThrowCustomAsync<T>(
            this IValidator<T> validator,
            T instance)
        {
            var result = await validator.ValidateAsync(instance);

            if (!result.IsValid)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));

                throw new Domain.Exceptions.ValidationException(errorMessage);
            }
        }
    }
}
