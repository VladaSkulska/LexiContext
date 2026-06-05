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
    - JAPANESE/CHINESE: Default to standard polite form (Desu/Masu for Japanese) unless Tone dictates otherwise.
    - SLAVIC LANGUAGES: Ensure perfect case agreement (відмінки).
7. Sensitive Content & Slang: Do not censor words related to conflict, anatomy, or common slang if educational.
8. Edge Cases & Corrections: 
    - If '{0}' contains a typo, lacks an article, or has incorrect capitalization, return the PERFECT dictionary form in 'correctedWord'.
9. Asian Languages Phonetics (CRITICAL - NO EXCEPTIONS): 
    - IF '{1}' IS NOT JAPANESE OR CHINESE: Keep the 'contextReading' field strictly EMPTY "". Do not use ruby tags.
    - NO BRACKETS: NEVER use square brackets like [みず] or [wǒ] anywhere.
    - JAPANESE: Wrap ONLY Kanji in <ruby> tags. Leave Hiragana and Katakana as plain text OUTSIDE the <ruby> tags. 
      CRITICAL: The <rt> tag MUST contain ONLY the reading for the specific Kanji it wraps. DO NOT put okurigana (trailing hiragana) inside <rt>.
      BAD: <ruby>食べる<rt>たべる</rt></ruby> -> GOOD: <ruby>食<rt>た</rt></ruby>べる.
    - CHINESE: Wrap EVERY SINGLE character individually in its OWN <ruby> tag. DO NOT group multiple characters under one <rt> tag.
      BAD: <ruby>谢谢<rt>xièxie</rt></ruby> -> GOOD: <ruby>谢<rt>xiè</rt></ruby><ruby>谢<rt>xiè</rt></ruby>.
10. Word Translation & Register: Provide the translation of the 'correctedWord' into {5} in 'wordTranslation'. STRICT: NO STRESS MARKS in translations.
11. JSON Formatting: Escape all internal double quotes inside JSON string values.

Return ONLY a valid JSON object exactly in this format: 
{{ 
    "generatedContext": "...", 
    "contextTranslation": "...", 
    "contextReading": "",
    "wordTranslation": "...",
    "correctedWord": "..." 
}}
No markdown, no prefixes.
""";

        private const string TranslationPrompt = """
Task: Provide a professional linguistic translation and correction for the word or phrase '{0}' from {1} to {2}.

Strict Rules:
1. Correction & Lemma: If '{0}' has a typo, incorrect capitalization, or is a specific form, find its "dictionary form" (lemma).
2. Language Nuances (Target - {1}):
    - GENDER & PLURALS: If {1} is German/French/Spanish/Italian noun, ALWAYS include definite article and Plural form.
    - VERB VALENCY: If {1} is German/English verb, include its required preposition and case.
    - WORD STRESS: NEVER use acute accent marks (´) in the translation language ({2}). Use ONLY if {1} is Ukrainian.
    - ASIAN PHONETICS (Japanese/Chinese ONLY): Never use square brackets []. 
      JAPANESE: Wrap ONLY Kanji individually in <ruby> tags. Leave Hiragana/Katakana outside. NO okurigana inside <rt>. 
      CHINESE: 1 Character = 1 Ruby Tag. 
      Do NOT output plain text reading. Use <ruby> format ONLY.
    - EUROPEAN LANGUAGES: NO brackets and NO phonetic reading.
3. Translation (Native - {2}): 
    - Provide the most natural translation into {2}. 
    - STRICT: NO STRESS MARKS.
4. Tone: Match the tone of the source word.
5. No Metadata: Return ONLY the final result in the exact format: "CorrectedWord - Translation". 

Output: Just the corrected form and translation separated by a dash.
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
5. Asian Languages Phonetics (CRITICAL): 
    - IF '{2}' IS NOT JAPANESE OR CHINESE: Keep 'contextReading' strictly as "". Do NOT use ruby tags.
    - JAPANESE: Wrap ONLY Kanji in <ruby>. Hiragana stays outside. NO okurigana inside <rt>.
    - CHINESE: Wrap EVERY SINGLE character individually. 1 Character = 1 Ruby Tag. NO BRACKETS.
6. Translation: STRICTLY NO STRESS MARKS in 'contextTranslation'.
7. JSON Formatting: Escape all internal double quotes inside JSON string values.
8. Output: Return ONLY a valid JSON object exactly in this format: 
{{ 
    "generatedContext": "...", 
    "contextTranslation": "...", 
    "contextReading": "", 
    "wordTranslation": "" 
}}
No markdown, no prefixes.
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

Language-Specific Nuances (CRITICAL):
    - IF '{1}' IS NOT JAPANESE AND NOT CHINESE: You MUST NOT use <ruby> tags anywhere. The 'reading' field in vocabulary MUST be completely empty "".
    - IF '{1}' IS JAPANESE OR CHINESE:
      - NO BRACKETS: Never use square brackets [] anywhere.
      - JAPANESE: Wrap ONLY Kanji characters in <ruby> tags in title and content.
        STRICT: Hiragana and Katakana MUST remain as plain text OUTSIDE any <ruby> tag. NEVER wrap a hiragana or katakana character in <ruby>.
        CRITICAL: The <rt> tag MUST contain ONLY the reading for that specific Kanji character. DO NOT include okurigana (trailing hiragana) inside <rt>.
        GOOD: <ruby>食<rt>た</rt></ruby>べる
        BAD: <ruby>は<rt>は</rt></ruby> — hiragana must NEVER be wrapped.
        BAD: <ruby>食べる<rt>たべる</rt></ruby> — okurigana must stay outside.
      - CHINESE: Wrap EVERY SINGLE character individually in its OWN <ruby> tag. 1 Character = 1 Ruby Tag.
        GOOD: <ruby>谢<rt>xiè</rt></ruby><ruby>谢<rt>xiè</rt></ruby>
        BAD: <ruby>谢谢<rt>xièxie</rt></ruby>
      - ALIGNMENT: Place <b> or <i> tags strictly around each individual character's ruby tag or plain text.

Vocabulary Extraction (CRITICAL - STRICT ARRAY LIMIT): 
- The JSON 'vocabulary' array MUST contain EXACTLY {5} items.
- These {5} items MUST be ONLY the NEW words/phrases you introduced (the <b><i> ones).
- COMPLETELY EXCLUDE the User's Target Words ({0}) from this JSON array.
- ASIAN LANGUAGES (Japanese/Chinese): The 'phrase' field MUST contain the <ruby> tags formatted EXACTLY as described above. The 'reading' field MUST contain ONLY plain text phonetics (Pinyin for Chinese, HIRAGANA for Japanese). STRICTLY NO ROMAJI. NO HTML in 'reading'.
- NON-ASIAN LANGUAGES: The 'phrase' field must be plain text. The 'reading' field MUST be strictly "".
- CRITICAL: In the 'phrase' JSON field, enclose the core Target/AI word in <b> tags. Place the <b> tags OUTSIDE the <ruby> elements.

Translation Rules for Vocabulary: 
    Provide the translation matching the story context into '{3}'. The 'translation' field MUST be pure plain text. ABSOLUTELY NO HTML tags and NO STRESS MARKS.

Typos & Gibberish Handling: GUESS the intended word and use the CORRECTED word.
JSON Formatting: Escape ONLY internal double quotes inside JSON string values. NEVER escape single quotes or apostrophes.
Return ONLY a valid JSON object exactly in this format:
{{
    "title": "Title formatted according to language rules",
    "content": "Full story text with <b> for User words...",
    "vocabulary": [
        {{ "phrase": "extracted text with <b> tags", "translation": "translation without formatting", "reading": "plain text reading (Hiragana/Pinyin) or empty" }}
    ]
}}
No markdown, no prefixes.
""";

        private const string FormatAsianWordPrompt = """
Task: Add phonetic reading to the Asian word '{0}' in {1}.
Strict Rules:
1. NO BRACKETS: Never use square brackets [].
2. JAPANESE: Use <ruby> ONLY for Kanji. Leave Hiragana/Katakana as plain text OUTSIDE the <ruby> tags. The <rt> MUST contain ONLY the reading for the specific Kanji. DO NOT put okurigana inside <rt>. (GOOD: <ruby>食<rt>た</rt></ruby>べる).
3. CHINESE: Wrap EVERY SINGLE character individually in its OWN HTML <ruby> tag. 1 Character = 1 Ruby Tag.
4. Return ONLY the formatted string. No translation, no markdown.
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

        public async Task<string> FormatAsianWordAsync(string word, LearningLanguage learningLanguage)
        {
            if (learningLanguage != LearningLanguage.Japanese && learningLanguage != LearningLanguage.Chinese)
                return word;

            var prompt = string.Format(FormatAsianWordPrompt, word, learningLanguage);
            try
            {
                var rawText = await SendGeminiRequestAsync(prompt, _modelLite);
                return rawText.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lite model failed for Formatting. Trying Flash...");
                var rawTextFlash = await SendGeminiRequestAsync(prompt, _model);
                return rawTextFlash.Trim();
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
            int startIndex = rawText.IndexOf('{');
            int endIndex = rawText.LastIndexOf('}');
            if (startIndex >= 0 && endIndex > startIndex)
            {
                return rawText.Substring(startIndex, endIndex - startIndex + 1);
            }
            return rawText;
        }
    }
}