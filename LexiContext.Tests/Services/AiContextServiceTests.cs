using LexiContext.Application.Models;
using LexiContext.Application.Services;
using LexiContext.Application.Services.Interfaces;
using LexiContext.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;

namespace LexiContext.Tests.Services
{
    public class AiContextServiceTests
    {
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<ILogger<AiContextService>> _loggerMock;

        public AiContextServiceTests()
        {
            _configMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<AiContextService>>();

            _configMock.Setup(c => c["Gemini:ApiKey"]).Returns("fake-api-key");
            _configMock.Setup(c => c["Gemini:Model"]).Returns("gemini-test-model");
            _configMock.Setup(c => c["Gemini:BaseUrl"]).Returns("https://fake-google.com/");
        }

        private HttpClient CreateFakeHttpClient(HttpStatusCode statusCode, string jsonResponse)
        {
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(jsonResponse)
                });

            return new HttpClient(handlerMock.Object);
        }

        [Fact]
        public async Task GetAiContextAsync_WhenApiReturnsValidJsonWithMarkdown_ParsesAndReturnsResult()
        {
            // Arrange
            var fakeGeminiResponse = """
            {
              "candidates": [
                {
                  "content": {
                    "parts": [
                      {
                        "text": "```json\n{ \"generatedContext\": \"We found a bug.\", \"contextTranslation\": \"Ми знайшли баг.\", \"contextReading\": \"\", \"wordTranslation\": \"помилка (баг)\" }\n```"
                      }
                    ]
                  }
                }
              ]
            }
            """;

            var httpClient = CreateFakeHttpClient(HttpStatusCode.OK, fakeGeminiResponse);
            var aiService = new AiContextService(httpClient, _configMock.Object, _loggerMock.Object);

            // Act
            var result = await aiService.GetAiContextAsync(
                "Bug", LearningLanguage.English, LearningLanguage.Ukrainian,
                ProficiencyLevel.Beginner, "IT", AiTone.Neutral);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("We found a bug.", result.GeneratedContext);
            Assert.Equal("Ми знайшли баг.", result.ContextTranslation);
            Assert.Equal("помилка (баг)", result.WordTranslation);
        }

        [Fact]
        public async Task GetAiContextAsync_WhenApiReturnsError_ThrowsInvalidOperationException()
        {
            // Arrange
            var httpClient = CreateFakeHttpClient(HttpStatusCode.InternalServerError, "Internal Error");
            var aiService = new AiContextService(httpClient, _configMock.Object, _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                aiService.GetAiContextAsync("Bug", LearningLanguage.English, LearningLanguage.Ukrainian, ProficiencyLevel.Beginner, "IT", AiTone.Neutral)
            );
        }

        [Fact]
        public async Task TranslateWordAsync_WhenApiReturnsText_ReturnsTrimmedText()
        {
            // Arrange
            var fakeGeminiResponse = """
            {
              "candidates": [
                {
                  "content": {
                    "parts": [
                      {
                        "text": "   помилка (баг)   "
                      }
                    ]
                  }
                }
              ]
            }
            """;

            var httpClient = CreateFakeHttpClient(HttpStatusCode.OK, fakeGeminiResponse);
            var aiService = new AiContextService(httpClient, _configMock.Object, _loggerMock.Object);

            // Act
            var result = await aiService.TranslateWordAsync("Bug", LearningLanguage.English, LearningLanguage.Ukrainian);

            // Assert
            Assert.Equal("помилка (баг)", result);
        }
    }
}