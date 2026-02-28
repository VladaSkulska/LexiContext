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
            1. Complexity: Use ONLY vocabulary and grammar strictly at the '{2}' level. If '{0}' is more advanced than '{2}', explain it using basic synonyms. AVOID complex verbs and passive voice for A1-B1 levels."
            2. Context & Theme: The sentence MUST naturally fit the "{3}" theme. Avoid "This is a {0}" style.
            3. Length: The sentence MUST be short and concise, ideally between 5 to 12 words maximum.
            4. Tone & Realism: Ensure the sentence sounds natural, not like a textbook. Tone: {4}.
            5. Accuracy: Translation into {5} must be natural, not literal.
            6. Asian Languages: If {1} is Chinese or Japanese, use appropriate Kanji/Hanzi levels for '{2}'.
            7. Safety: No politics, religion, violence, or NSFW topics. Keep it educational.
            8. Edge Cases (Incomplete Words, Particles, Typos): 
               - If '{0}' is a grammatical particle, suffix, prefix, or an incomplete grammar structure (especially in {1} like Chinese or Japanese), explain its grammatical function briefly in the translation and provide a sentence showing how it attaches to a word.
               - If '{0}' contains a clear typo, assume the most likely correct word, use the correct word in the example sentence, but do not apologize or write "you made a typo". Just provide the correct context.
            9. Phonetic Reading: 
            - If {1} is Japanese, provide Romaji in 'contextReading'.
            - If {1} is Chinese, provide Pinyin with tone marks in 'contextReading'.
            - For ANY OTHER language (like German, Spanish, French, English, etc.), leave 'contextReading' strictly as an empty string "". Do NOT provide transliteration.
            10. Word Translation: Provide the direct translation of '{0}' into {5} in the 'wordTranslation' field. 
            - CRITICAL: If '{0}' is typically translated using a foreign loanword or technical slang in {5} (e.g., 'bug' -> 'баг', 'feature' -> 'фіча'), you MUST provide the native, widely understood word FIRST, followed by the slang in parentheses. 
            - Example: "помилка (баг)", "особливість (фіча)".
            
            Return ONLY a valid JSON object exactly in this format: 
            {{ 
                "generatedContext": "...", 
                "contextTranslation": "...", 
                "contextReading": "...",
                "wordTranslation": "..." 
            }}
            No markdown, no prefixes.
            """;

        private const string TranslationPrompt = """
            Task: Translate the word or phrase '{0}' from {1} to {2}.

            Strict Rules:
            1. Accuracy & Jargon: If '{0}' is slang, an idiom, or a technical term, provide the most natural equivalent in {2}. If the common translation is a loanword/anglicism (like 'bug' -> 'баг'), strictly provide the native descriptive word first, then the slang in parentheses. Example output: "помилка (баг)".
            2. Multiple Meanings: If the word has several distinct common meanings, provide the 2 most frequent translations separated by a comma (e.g., "залишати, покидати").
            3. Typos & Fragments: If '{0}' contains a typo or is an incomplete word/grammatical fragment, assume the most likely intended meaning and translate it.
            4. Tone: Match the tone of the source word (e.g., formal for formal, informal for informal).
            5. Safety: Strictly avoid NSFW, toxic, or offensive translations. Keep it educational.
            6. No Metadata: Return ONLY the translated text. No "Translation:", no quotes, no explanations, no pronunciation.

            Output: Just the translated text.
            """;

        private const string SimplifyPrompt = """
            Task: Rewrite the following sentence to be MUCH simpler.
            Target Word to KEEP: "{0}"
            Original Sentence: "{1}"
            Target Language (for rewritten sentence): {2}.
            Target Translation Language (for contextTranslation): {3}.

            Rules:
            1. Focus: You MUST keep the target word "{0}" in the new sentence. Do NOT change or remove it.
            2. Vocabulary & Grammar: Adapt strictly to the {4} level. Keep the sentence very short (5-10 words maximum). AVOID complex verbs and passive voice.
            3. Meaning: Keep the core meaning of the original sentence, but express it using simple synonyms appropriate for the {4} level.
            4. Phonetic Reading: 
               - If {2} is Japanese, provide Romaji in 'contextReading'.
               - If {2} is Chinese, provide Pinyin with tone marks in 'contextReading'.
               - For ANY OTHER language, leave 'contextReading' strictly as an empty string "". DO NOT write transliteration for the {3} translation.
            5. Output: Return ONLY a valid JSON object exactly in this format: 
            {{ 
                "generatedContext": "...", 
                "contextTranslation": "...", 
                "contextReading": "",
                "wordTranslation": "" 
            }}
            No markdown, no prefixes.
            """;

        public AiContextService(HttpClient httpClient, IConfiguration config, ILogger<AiContextService> logger)
        {
            _httpClient = httpClient;
            _aiApiKey = config["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini Api key is missing");
            _model = config["Gemini:Model"] ?? "gemini-2.5-flash";
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

                var cleanJson = ExtractJsonFromRawText(rawText!);

                var result = JsonSerializer.Deserialize<AiContextResult>(cleanJson, new JsonSerializerOptions
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

        public async Task<string> TranslateWordAsync(string word, LearningLanguage learningLanguage, LearningLanguage nativeLanguage)
        {
            var endpoint = $"{_baseUrl}{_model}:generateContent?key={_aiApiKey}";
            var prompt = string.Format(TranslationPrompt, word, learningLanguage, nativeLanguage);

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
                var translatedText = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrWhiteSpace(translatedText))
                {
                    throw new InvalidOperationException("AI returned empty translation string.");
                }

                return translatedText.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform fast translation for word: {Word}", word);
                throw new InvalidOperationException("Fast translation failed.", ex);
            }
        }

        public async Task<AiContextResult> SimplifyContextAsync(
            string word,
            string originalContext,
            LearningLanguage learningLanguage,
            LearningLanguage nativeLanguage,
            ProficiencyLevel simplerLevel)
        {
            var endpoint = $"{_baseUrl}{_model}:generateContent?key={_aiApiKey}";

            var prompt = string.Format(SimplifyPrompt, word, originalContext, learningLanguage, nativeLanguage, simplerLevel);
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

                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Google API Error during simplification: {Error}", jsonResponse);
                    throw new InvalidOperationException($"Google API error: {response.StatusCode}");
                }

                using var doc = JsonDocument.Parse(jsonResponse);
                var rawText = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                var cleanJson = ExtractJsonFromRawText(rawText!);

                var result = JsonSerializer.Deserialize<AiContextResult>(cleanJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result ?? throw new Exception("AI returned empty context during simplification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to simplify context: '{OriginalContext}'", originalContext);
                throw new Domain.Exceptions.AiTranslationException("Failed to simplify sentence. AI temporarily unavailable.", ex);
            }
        }

        private string ExtractJsonFromRawText(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText))
            {
                return string.Empty;
            }

            int startIndex = rawText.IndexOf('{');
            int endIndex = rawText.LastIndexOf('}');

            if (startIndex >= 0 && endIndex >= startIndex)
            {
                return rawText.Substring(startIndex, endIndex - startIndex + 1);
            }

            return rawText;
        }
    }
}