using ValidationException = LexiContext.Domain.Exceptions.ValidationException;
using LexiContext.API.Models;
using LexiContext.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
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
