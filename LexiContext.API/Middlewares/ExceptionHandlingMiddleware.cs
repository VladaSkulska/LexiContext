using LexiContext.Domain.Exceptions;
using LexiContext.API.Models;
using System.Text.Json;

namespace LexiContext.API.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        public readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred during request processing.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = exception switch
            {
                NotFoundException => new ErrorDetails
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = exception.Message
                },

                ValidationException => new ErrorDetails
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = exception.Message
                },

                ArgumentException => new ErrorDetails
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = exception.Message
                },

                AiTranslationException => new ErrorDetails
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "The AI service is temporarily unavailable or unable to process the word. Please enter the translation manually."
                },

                InvalidOperationException e when e.Message == "AI_503" || (e.InnerException != null && e.InnerException.Message == "AI_503") => new ErrorDetails
                {
                    StatusCode = StatusCodes.Status503ServiceUnavailable,
                    Message = "Сервери ШІ зараз дуже перевантажені. Ми намагалися до них достукатися, але безуспішно. Будь ласка, спробуйте ще раз за хвилину."
                },

                InvalidOperationException e when e.Message == "AI_429" || (e.InnerException != null && e.InnerException.Message == "AI_429") => new ErrorDetails
                {
                    StatusCode = StatusCodes.Status429TooManyRequests,
                    Message = "Ви генеруєте надто швидко! Ліміт запитів до ШІ тимчасово вичерпано. Зробіть коротку паузу."
                },

                InvalidOperationException e when e.Message.Contains("AI") || e.Message.Contains("translation") => new ErrorDetails
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "AI magic didn't work. Please fill in the fields manually."
                },

                _ => new ErrorDetails
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred."
                }
            };

            context.Response.StatusCode = response.StatusCode;

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}