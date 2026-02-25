using LexiContext.Application.Models;
using LexiContext.Application.Services.Interfaces;
using LexiContext.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LexiContext.Application.Services
{
    public class AiContextService : IAiContextService
    {
        private readonly HttpClient _httpClient;
        private readonly string _aiApiKey;
        private readonly string _model;
        private readonly string _baseUrl;
        private readonly ILogger<AiContextService> _logger;

        private const string UniversalPrompt = """
            Task: Generate a high-quality example sentence for the word '{0}' in {1}.
            User Level: {2}.
            Theme/Context: {3}.
            Tone: {4}.
            Target Translation Language: {5}.

            Strict Rules:
            1. Complexity: Adapt grammar and vocabulary strictly to the '{2}' level of {1}. 
               - For Beginners: use only high-frequency basic words and simple structures. Use simple words to explain complex terms.
               - For Advanced: use nuanced, natural, and sophisticated language.
            2. Context & Theme: Create a meaningful, realistic sentence where the word's usage is clear. Avoid "This is a {0}" style. The sentence MUST naturally fit the "{3}" theme.
            3. Tone & Realism: Ensure the sentence sounds natural, not like a textbook. The tone should be {4}.
            4. Accuracy: Ensure the translation into {5} is natural, not literal.
            5. Asian Languages: If {1} is Chinese or Japanese, use appropriate Kanji/Hanzi levels for '{2}'.
            6. Safety: Strictly prohibit any content related to politics, religion, violence, discrimination, or NSFW topics. Keep it safe and educational.

            Return ONLY a valid JSON object: 
            {{ "generatedContext": "...", "contextTranslation": "..." }}
            No markdown, no prefixes.
            """;

        public AiContextService(HttpClient httpClient, IConfiguration config, ILogger<AiContextService> logger)
        {
            _httpClient = httpClient;
            _aiApiKey = config["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini Api key is missing");
            _model = config["Gemini:Model"] ?? "gemini-1.5-flash";
            _baseUrl = config["Gemini:BaseUrl"] ?? throw new ArgumentNullException("Gemini Base URL is missing");
            _logger = logger;
        }

        public async Task<AiContextResult> GetAiContextAsync(string word, 
            LearningLanguage learningLanguage, 
            LearningLanguage nativeLanguage,
            ProficiencyLevel level,
            string deckContext,
            AiTone tone)
        {
            var endpoint = $"{_baseUrl}{_model}:generateContent?key={_aiApiKey}";

            var prompt = string.Format(UniversalPrompt, 
                word, 
                learningLanguage, 
                level, 
                deckContext, 
                tone, 
                nativeLanguage);
            
            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(jsonResponse);
                var rawText = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                var result = JsonSerializer.Deserialize<AiContextResult>(rawText!, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result ?? throw new Exception("AI returned empty context");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate AI context for word: {Word}. Target Level: {Level}", word, level);

                throw new InvalidOperationException("Failed to generate AI context. Card creation aborted.", ex);
            }
        }
    }
}
