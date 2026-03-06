using LexiContext.Application.Models.Ai;
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
            1. Complexity: Use ONLY vocabulary and grammar strictly at the '{2}' level. If '{0}' is more advanced than '{2}', explain it using basic synonyms. AVOID complex verbs and passive voice for A1-B1 levels.
            2. Context & Theme: The sentence MUST naturally fit the "{3}" theme. Avoid "This is a {0}" style.
            3. Length: The sentence MUST be short and concise, ideally between 5 to 12 words maximum.
            4. Tone & Realism: Ensure the sentence sounds natural, not like a textbook. Tone: {4}.
            5. Accuracy: Translation into {5} must be natural, not literal.
            6. Language-Specific Nuances (CRITICAL):
               - WORD STRESS (UA, ES, IT): If {1} is Ukrainian, Spanish, or Italian, ALWAYS use an acute accent mark (´) over the stressed vowel in BOTH the 'correctedWord' and 'generatedContext' fields (e.g., "зáмок", "calendário").
               - NOUNS & PLURALS (German, French, Spanish, etc.): If '{0}' is a noun, you MUST include its definite article (der/die/das, el/la). For GERMAN, also include the Plural form in 'correctedWord' (e.g., User inputs "buch" -> return "das Buch (die Bücher)").
               - VERB VALENCY (DE, EN): If '{0}' is a German or English verb, include its required preposition and case in 'correctedWord' (e.g., "warten (auf + Akk)", "depend (on + obj)").
               - JAPANESE/CHINESE: Default to standard polite form (Desu/Masu for Japanese) unless Tone dictates otherwise. Use natural phrasing, not robotic translations.
               - SLAVIC LANGUAGES: Ensure perfect case agreement (відмінки) in the sentence.
            7. Sensitive Content & Slang: Do not censor words related to conflict, anatomy, or common slang/profanity if they are part of natural language learning. Provide a neutral, educational context. If a word is rude or offensive, explicitly label it in the 'wordTranslation' field.
            8. Edge Cases & Corrections: 
               - If '{0}' is a grammatical particle, DO NOT use dry linguistic terms only. Provide a friendly explanation in 'wordTranslation'.
               - If '{0}' contains a typo, lacks an article, or has incorrect capitalization, return the PERFECT dictionary form in 'correctedWord'.
            9. Phonetic Reading (CRITICAL FOR ASIAN LANGUAGES): 
               - If {1} is Japanese, provide the reading in 'contextReading' using a WORD-BY-WORD format with brackets and HIRAGANA: "Word[hiragana]". Example: "私[わたし] は[わ] 先生[せんせい] です[です]". DO NOT use Romaji.
               - If {1} is Chinese, provide Pinyin in 'contextReading' using a WORD-BY-WORD format with brackets: "Hanzi[pinyin]". Example: "我[wǒ] 在[zài] 学校[xuéxiào]".
               - For ANY OTHER language, leave 'contextReading' strictly as "".
            10. Word Translation & Register: Provide the translation of the 'correctedWord' into {5} in 'wordTranslation'. Note specific registers (formal/informal).
            11. JSON Formatting: Escape all internal double quotes inside JSON string values (e.g., use \" if quoting something).

            Return ONLY a valid JSON object exactly in this format: 
            {{ 
                "generatedContext": "...", 
                "contextTranslation": "...", 
                "contextReading": "...",
                "wordTranslation": "...",
                "correctedWord": "..." 
            }}
            No markdown, no prefixes.
            """;

        private const string TranslationPrompt = """
            Task: Provide a professional linguistic translation and correction for the word or phrase '{0}' from {1} to {2}.

            Strict Rules:
            1. Correction & Lemma: If '{0}' has a typo, incorrect capitalization, or is a specific form (like a plural or conjugated verb), find its "dictionary form" (lemma).
            2. Language Nuances (Target - {1}):
               - GENDER & PLURALS: If {1} is German, French, Spanish, or Italian and the word is a noun, ALWAYS include the definite article and Plural form (e.g., "der Zug (die Züge)", "la casa (las casas)").
               - VERB VALENCY: If {1} is German or English and the word is a verb, include its required preposition and case (e.g., "warten (auf + Akk)", "depend (on + obj)").
               - WORD STRESS (UA, ES, IT): For Ukrainian, Spanish, or Italian, ALWAYS use an acute accent mark (´) over the stressed vowel (e.g., "зáмок", "inglés").
               - JAPANESE/CHINESE: Provide the word in its native script with its reading in brackets: "Word[hiragana/pinyin]". Example: "先生[せんせい]", "学校[xuéxiào]".
            3. Translation (Native - {2}): 
               - Provide the most natural translation into {2}. 
               - If it's a technical term/slang, use the native word first, then slang in parentheses: "помилка (баг)".
            4. Tone: Match the tone of the source word.
            5. No Metadata: Return ONLY the final result in the exact format: "CorrectedWord - Translation". 
               Example Output: "der Zug (die Züge) - потяг" or "wárten (auf + Akk) - чекáти".
    
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
            2. Vocabulary & Grammar: Adapt strictly to the {4} level. Keep the sentence very short (5-10 words maximum). AVOID complex verbs and passive voice.
            3. Meaning: Keep the core meaning of the original sentence, but express it using simple synonyms appropriate for the {4} level.
            4. Word Stress: If {2} is Ukrainian, Spanish, or Italian, ALWAYS use an acute accent mark (´) over the stressed vowel in the new 'generatedContext'.
            5. Phonetic Reading: 
               - If {2} is Japanese, provide Hiragana reading in 'contextReading' in bracket format: "Word[hiragana]".
               - If {2} is Chinese, provide Pinyin in 'contextReading' in bracket format: "Hanzi[pinyin]".
               - For ANY OTHER language, leave 'contextReading' strictly as "".
            6. JSON Formatting: Escape all internal double quotes inside JSON string values.
            7. Output: Return ONLY a valid JSON object exactly in this format: 
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
            Think of graded readers like André Klein's "Café in Berlin". The story MUST be interesting, with natural conversational dialogue and everyday situations. Keep sentences relatively short. AVOID dry, robotic textbook language. Adapt grammar strictly to the '{2}' level.

            Strict Rules:
            1. User's Words: You MUST include ALL the provided Target Words. You may naturally conjugate or decline them. Enclose the word OR the short useful phrase containing it in bold tags: <b>phrase or word</b>.
            2. AI Suggested Words: Introduce EXACTLY {5} NEW, highly useful words, idioms, or colloquial phrases that fit the story naturally. Enclose these new items in bold and italic tags: <b><i>phrase or word</i></b>.
            3. Length: Keep the story around 150-250 words. Divide it into highly readable paragraphs.
            4. Language-Specific Nuances & Formatting:
               - WORD STRESS (UA, ES, IT): If {1} is Ukrainian, Spanish, or Italian, ALWAYS use an acute accent mark (´) over the stressed vowel in ALL text within the 'content' field.
               - GERMAN/FRENCH/SPANISH: Ensure perfect noun gender and case agreement in the story.
               - JAPANESE/CHINESE: If {1} is Japanese, wrap ALL kanji characters in <ruby> tags with HIRAGANA (e.g., <ruby>先生<rt>せんせい</rt></ruby>). DO NOT use Romaji. If Chinese, use <ruby> tags with Pinyin. Use standard polite form (Desu/Masu) unless characters are close friends.
            5. Vocabulary Extraction (PRIORITIZE PHRASES): 
               - ALWAYS try to extract a SHORT USEFUL PHRASE or collocation (2-4 words) rather than just a standalone word. For example, if the word is "Zug" (train), extract "auf den Zug warten" (wait for the train).
               - GERMAN NOUNS: Always include the definite article and Plural form in the extraction (e.g., "das <b>Buch</b> (die Bücher)").
               - VERB VALENCY: If the extracted word is a German/English verb, include its preposition + case in the 'translation' field.
               - CRITICAL: In the 'phrase' JSON field, enclose the core Target/AI word in <b> tags to highlight it within the extracted phrase (e.g., "auf den <b>Zug</b> warten").
               - ASIAN PHONETICS: For Japanese/Chinese, the 'reading' field MUST use the word-by-word bracket format. For Japanese use Hiragana (e.g., "学校[がっこう] の[の] 先生[せんせい]"). For Chinese use Pinyin (e.g., "写了[xiěle] 代码[dàimǎ]").
            6. Translation Rules for Vocabulary:
               - Provide the translation of the extracted word/phrase in {3} matching the EXACT context of the story.
            7. Typos & Gibberish Handling: If any of the User's words contains a typo or is complete nonsense, GUESS the intended word, use the CORRECTED word in the story, and explicitly explain this in the 'translation' field.
            8. JSON Formatting: Escape all internal double quotes inside JSON string values (e.g., use \" for dialogue inside the "content" field).
            
            Return ONLY a valid JSON object exactly in this format:
            {{
                "title": "Engaging Title in {1}",
                "content": "Full story text with <b>, <i>, and <ruby> tags...",
                "vocabulary": [
                    {{ "phrase": "extracted text with <b> tags", "translation": "translation", "reading": "..." }}
                ]
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

        public async Task<AiContextResult> GetAiContextAsync(string word, LearningLanguage learningLanguage, LearningLanguage nativeLanguage, ProficiencyLevel level, string deckContext, AiTone tone)
        {
            try
            {
                var prompt = string.Format(UniversalPrompt, word, learningLanguage, level, deckContext, tone, nativeLanguage);
                return await ExecuteAndDeserializeGeminiRequestAsync<AiContextResult>(prompt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate AI context for word: {Word}. Target Level: {Level}", word, level);
                throw new InvalidOperationException("Failed to generate AI context. Card creation aborted.", ex);
            }
        }

        public async Task<string> TranslateWordAsync(string word, LearningLanguage learningLanguage, LearningLanguage nativeLanguage)
        {
            try
            {
                var prompt = string.Format(TranslationPrompt, word, learningLanguage, nativeLanguage);
                var rawText = await SendGeminiRequestAsync(prompt);
                return rawText.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform fast translation for word: {Word}", word);
                throw new InvalidOperationException("Fast translation failed.", ex);
            }
        }

        public async Task<AiContextResult> SimplifyContextAsync(string word, string originalContext, LearningLanguage learningLanguage, LearningLanguage nativeLanguage, ProficiencyLevel simplerLevel)
        {
            try
            {
                var prompt = string.Format(SimplifyPrompt, word, originalContext, learningLanguage, nativeLanguage, simplerLevel);
                return await ExecuteAndDeserializeGeminiRequestAsync<AiContextResult>(prompt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to simplify context: '{OriginalContext}'", originalContext);
                throw new Domain.Exceptions.AiTranslationException("Failed to simplify sentence. AI temporarily unavailable.", ex);
            }
        }

        public async Task<AiStoryResult> GenerateStoryAsync(List<string> targetWords, LearningLanguage learningLanguage, LearningLanguage nativeLanguage, ProficiencyLevel level, StoryGenre genre, int aiNewWordsCount)
        {
            var wordsString = string.Join(", ", targetWords);
            try
            {
                var prompt = string.Format(StoryPrompt, wordsString, learningLanguage, level, nativeLanguage, genre, aiNewWordsCount);
                return await ExecuteAndDeserializeGeminiRequestAsync<AiStoryResult>(prompt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate story for words: {Words}", wordsString);
                throw new InvalidOperationException("Failed to generate AI story.", ex);
            }
        }
        

        private async Task<string> SendGeminiRequestAsync(string prompt)
        {
            var endpoint = $"{_baseUrl}{_model}:generateContent?key={_aiApiKey}";
            var requestBody = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContext = await response.Content.ReadAsStringAsync();
                _logger.LogError("Google API Error: {StatusCode}. Details: {Error}", response.StatusCode, errorContext);
                throw new InvalidOperationException($"Google API error: {response.StatusCode}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);

            var rawText = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(rawText))
                throw new Exception("AI returned empty text response.");

            return rawText;
        }

        private async Task<T> ExecuteAndDeserializeGeminiRequestAsync<T>(string prompt)
        {
            var rawText = await SendGeminiRequestAsync(prompt);
            var cleanJson = ExtractJsonFromRawText(rawText);

            var result = JsonSerializer.Deserialize<T>(cleanJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result ?? throw new Exception($"AI returned invalid or empty JSON for type {typeof(T).Name}");
        }

        private string ExtractJsonFromRawText(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText)) return string.Empty;

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