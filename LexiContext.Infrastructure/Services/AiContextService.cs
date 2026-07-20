using LexiContext.Application.Models.Ai;
using LexiContext.Application.Services.Interfaces;
using LexiContext.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Polly;
using Polly.Retry;

namespace LexiContext.Application.Services
{
    public class AiContextService : IAiContextService
    {
        private readonly HttpClient _httpClient;
        private readonly string _aiApiKey;
        private readonly string _baseUrl;
        private readonly string _model;
        private readonly string _modelLite;
        private readonly ILogger<AiContextService> _logger;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

        #region Prompts 
        private const string UniversalPrompt = """
Task: Generate a high-quality example sentence for the word '{0}' in {1}.
User Level: {2}.
Theme/Context: {3}.
Tone: {4}.
Target Translation Language: {5}.

Strict Rules:
1. Complexity: Use ONLY vocabulary and grammar strictly at the '{2}' level. If '{0}' is more advanced than '{2}', explain it using basic synonyms. AVOID complex verbs and passive voice for A1-B1 levels.
2. Context & Theme: The sentence MUST naturally fit the "{3}" theme. Avoid "This is a {0}" style.
3. Length: The sentence MUST be short and concise, ideally between 5 to 12 words maximum.
4. Tone & Realism: Ensure the sentence sounds natural, not like a textbook. Tone: {4}.
5. Accuracy: Translation into {5} must be natural, not literal.
6. Language-Specific Nuances (CRITICAL):
    - WORD STRESS: NEVER use acute accent marks (´) in the translation language ({5}). Use them ONLY if the learning language ({1}) is Ukrainian.
    - NOUNS & PLURALS (German, French, Spanish, etc.): If '{0}' is a noun, you MUST include its definite article (der/die/das, el/la). For GERMAN, also include the Plural form in 'correctedWord'.
    - VERB VALENCY (DE, EN): If '{0}' is a verb, include its required preposition and case in 'correctedWord'.
    - SLAVIC LANGUAGES: Ensure perfect case agreement (відмінки).
7. Sensitive Content & Slang: Do not censor words related to conflict, anatomy, or common slang if educational.
8. Edge Cases & Corrections: 
    - If '{0}' contains a typo, lacks an article, or has incorrect capitalization, return the PERFECT dictionary form in 'correctedWord'.
9. Word Translation & Register: Provide the translation of the 'correctedWord' into {5} in 'wordTranslation'. STRICT: NO STRESS MARKS in translations.
10. JSON Formatting: Escape all internal double quotes inside JSON string values.

Return ONLY a valid JSON object exactly in this format. DO NOT use markdown code blocks:
{{ 
    "generatedContext": "...", 
    "contextTranslation": "...", 
    "wordTranslation": "...",
    "correctedWord": "..." 
}}
""";

        private const string TranslationPrompt = """
Task: Provide a professional linguistic translation and correction for the word or phrase '{0}' from {1} to {2}.

CRITICAL OUTPUT RULE:
You MUST return ONLY a single plain text string in this exact format:
[Word] - [Translation]

Rules for [Word] ({1}):
1. Correction: Fix typos and use dictionary form (lemma).
2. Grammar: If {1} is German/French/Spanish, ALWAYS include definite article and plural. If verb, include preposition.

Rules for [Translation] ({2}):
1. Natural translation without stress marks (´) unless {1} is Ukrainian.

Examples of exact expected output:
das Buch, -⸚er - book
la casa, -s - house

Return NOTHING ELSE. No JSON, no markdown formatting, no explanations.
""";

        private const string SimplifyPrompt = """
Task: Rewrite the following sentence to be MUCH simpler.
Target Word to KEEP: "{0}"
Original Sentence: "{1}"
Target Language (for rewritten sentence): {2}.
Target Translation Language (for contextTranslation): {3}.

Rules:
1. Focus: You MUST keep the target word "{0}" in the new sentence. Do NOT change or remove it.
2. Vocabulary & Grammar: Adapt strictly to the {4} level. Keep the sentence very short (5-10 words). AVOID complex verbs.
3. Meaning: Keep the core meaning of the original sentence.
4. Word Stress: NEVER use acute accent marks (´) in the translation language ({3}).
5. Translation: STRICTLY NO STRESS MARKS in 'contextTranslation'.
6. JSON Formatting: Escape all internal double quotes inside JSON string values.
7. Output: Return ONLY a valid JSON object exactly in this format. DO NOT use markdown code blocks:
{{ 
    "generatedContext": "...", 
    "contextTranslation": "...", 
    "wordTranslation": "" 
}}
""";

        private const string StoryPrompt = """
        Task: Write a short, engaging {4} (Genre) in {1}.
        Proficiency Level: {2}.
        User's Target Words to include: {0}.
        Native Language (for vocabulary translation): {3}.
        Number of NEW useful words/phrases AI must introduce: {5}.

        Style & Inspiration:
        The story MUST be interesting, with natural conversational dialogue and everyday situations. Keep sentences relatively short. Adapt grammar strictly to the '{2}' level.

        Strict Rules for Formatting:
        1. Plain Text Default: The vast majority of the story MUST be regular, unformatted text.
        2. User's Words: You MUST include ALL the provided Target Words ({0}). Enclose ONLY these exact words in bold tags: <b>word</b>.
        3. AI Suggested Words: Introduce EXACTLY {5} NEW, highly useful words, idioms, or colloquial phrases. Enclose ONLY these {5} new items in bold and italic tags: <b><i>word</i></b>.

        Vocabulary Extraction (CRITICAL - STRICT ARRAY LIMIT): 
        - The JSON 'vocabulary' array MUST contain EXACTLY {5} items.
        - These {5} items MUST be ONLY the NEW words/phrases you introduced (the <b><i> ones).
        - COMPLETELY EXCLUDE the User's Target Words ({0}) from this JSON array.

        Translation Rules for Vocabulary: 
            Provide the translation matching the story context into '{3}'. The 'translation' field MUST be pure plain text. ABSOLUTELY NO HTML tags and NO STRESS MARKS.

        Typos & Gibberish Handling: GUESS the intended word and use the CORRECTED word.
        JSON Formatting: Escape ONLY internal double quotes inside JSON string values. NEVER escape single quotes or apostrophes.

        Return ONLY a valid JSON object exactly in this format. DO NOT use markdown code blocks like ```json. Output raw JSON only:
        {{
            "title": "Title formatted according to language rules",
            "content": "Full story text with <b> for User words...",
            "vocabulary": [
                {{ "phrase": "extracted text with <b> tags", "translation": "translation without formatting" }}
            ]
        }}
        """;
        #endregion

        public AiContextService(HttpClient httpClient, IConfiguration config, ILogger<AiContextService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _aiApiKey = config["Gemini:ApiKey"] ?? throw new ArgumentNullException("ApiKey is missing");
            _baseUrl = config["Gemini:BaseUrl"] ?? throw new ArgumentNullException("BaseUrl is missing");

            _model = config["Gemini:Model"] ?? "gemini-2.5-flash";
            _modelLite = config["Gemini:ModelLite"] ?? "gemini-2.5-flash-lite";

            _retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r =>
                    r.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                    r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        _logger.LogWarning("Google API returned {StatusCode}. Retrying in {Delay}s (Attempt {RetryCount})...",
                            outcome.Result.StatusCode, timespan.TotalSeconds, retryAttempt);
                    });
        }

        public async Task<AiContextResult> GetAiContextAsync(string word, LearningLanguage learningLanguage, LearningLanguage nativeLanguage, ProficiencyLevel level, string deckContext, AiTone tone)
        {
            var prompt = string.Format(UniversalPrompt, word, learningLanguage, level, deckContext, tone, nativeLanguage);
            try
            {
                return await ExecuteAndDeserializeGeminiRequestAsync<AiContextResult>(prompt, _model);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Flash model failed for Context. Trying Lite model as fallback...");
                return await ExecuteAndDeserializeGeminiRequestAsync<AiContextResult>(prompt, _modelLite);
            }
        }

        public async Task<string> TranslateWordAsync(string word, LearningLanguage learningLanguage, LearningLanguage nativeLanguage)
        {
            var prompt = string.Format(TranslationPrompt, word, learningLanguage, nativeLanguage);
            try
            {
                var rawText = await SendGeminiRequestAsync(prompt, _modelLite);
                return rawText.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lite model failed for Translation. Trying Flash model...");
                var rawTextFlash = await SendGeminiRequestAsync(prompt, _model);
                return rawTextFlash.Trim();
            }
        }

        public async Task<AiContextResult> SimplifyContextAsync(string word, string originalContext, LearningLanguage learningLanguage, LearningLanguage nativeLanguage, ProficiencyLevel simplerLevel)
        {
            var prompt = string.Format(SimplifyPrompt, word, originalContext, learningLanguage, nativeLanguage, simplerLevel);
            try
            {
                return await ExecuteAndDeserializeGeminiRequestAsync<AiContextResult>(prompt, _model);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Flash model failed for Simplify. Trying Lite...");
                return await ExecuteAndDeserializeGeminiRequestAsync<AiContextResult>(prompt, _modelLite);
            }
        }

        public async Task<AiStoryResult> GenerateStoryAsync(List<string> targetWords, LearningLanguage learningLanguage, LearningLanguage nativeLanguage, ProficiencyLevel level, StoryGenre genre, int aiNewWordsCount)
        {
            var wordsString = string.Join(", ", targetWords);
            var prompt = string.Format(StoryPrompt, wordsString, learningLanguage, level, nativeLanguage, genre, aiNewWordsCount);

            try
            {
                return await ExecuteAndDeserializeGeminiRequestAsync<AiStoryResult>(prompt, _modelLite, maxOutputTokens: 16000, disableThinking: false);
            }
            catch (InvalidOperationException ex) when (ex.Message == "AI_429")
            {
                _logger.LogWarning("Lite model quota exceeded for Story. Trying Flash as fallback...");
                try
                {
                    return await ExecuteAndDeserializeGeminiRequestAsync<AiStoryResult>(prompt, _model, maxOutputTokens: 16000, disableThinking: false);
                }
                catch (Exception liteEx)
                {
                    _logger.LogError(liteEx, "Flash model also failed to generate story.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical failure generating story with Lite model.");
                throw;
            }
        }

        private async Task<string> SendGeminiRequestAsync(
            string prompt,
            string targetModel,
            int maxOutputTokens = 8192,
            bool disableThinking = false)
        {
            var endpoint = $"{_baseUrl}{targetModel}:generateContent?key={_aiApiKey}";

            object requestBody;
            if (disableThinking)
            {
                requestBody = new
                {
                    contents = new[] { new { parts = new[] { new { text = prompt } } } },
                    generationConfig = new
                    {
                        maxOutputTokens = maxOutputTokens,
                        thinkingConfig = new { thinkingBudget = 0 }
                    }
                };
            }
            else
            {
                requestBody = new
                {
                    contents = new[] { new { parts = new[] { new { text = prompt } } } },
                    generationConfig = new
                    {
                        maxOutputTokens = maxOutputTokens
                    }
                };
            }

            var jsonContent = JsonSerializer.Serialize(requestBody);

            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                return await _httpClient.PostAsync(endpoint, content);
            });

            if (!response.IsSuccessStatusCode)
            {
                var errorContext = await response.Content.ReadAsStringAsync();
                _logger.LogError("Google API Error: {StatusCode}. Model: {Model}. Details: {Error}", response.StatusCode, targetModel, errorContext);

                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    throw new InvalidOperationException("AI_503");

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    throw new InvalidOperationException("AI_429");

                throw new InvalidOperationException($"Google API error: {response.StatusCode}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);

            return doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? throw new Exception("Empty AI response.");
        }

        private async Task<T> ExecuteAndDeserializeGeminiRequestAsync<T>(
            string prompt,
            string targetModel,
            int maxOutputTokens = 8192,
            bool disableThinking = false)
        {
            var rawText = await SendGeminiRequestAsync(prompt, targetModel, maxOutputTokens, disableThinking);
            var cleanJson = ExtractJsonFromRawText(rawText);

            return JsonSerializer.Deserialize<T>(cleanJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new Exception("Failed to deserialize AI response.");
        }

        private string ExtractJsonFromRawText(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText)) return string.Empty;

            var text = rawText.Trim();

            if (text.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
                text = text.Substring(7);
            else if (text.StartsWith("```"))
                text = text.Substring(3);

            if (text.EndsWith("```"))
                text = text.Substring(0, text.Length - 3);

            text = text.Trim();

            int startIndex = text.IndexOf('{');
            int endIndex = text.LastIndexOf('}');

            if (startIndex >= 0 && endIndex > startIndex)
            {
                return text.Substring(startIndex, endIndex - startIndex + 1);
            }

            return text;
        }
    }
}