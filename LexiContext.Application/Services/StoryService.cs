using LexiContext.Application.DTOs.Stories;
using LexiContext.Application.Interfaces.Repos;
using LexiContext.Application.Models.Ai;
using LexiContext.Application.Services.Interfaces;
using LexiContext.Domain.Entities;
using LexiContext.Domain.Enums;
using LexiContext.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace LexiContext.Application.Services
{
    public class StoryService : IStoryService
    {
        private readonly IStoryRepository _storyRepository;
        private readonly IDeckRepository _deckRepository;
        private readonly ICardRepository _cardRepository;
        private readonly IAiContextService _aiContextService;
        private readonly ILogger<StoryService> _logger;

        private const int MaxUserWords = 10;

        public StoryService(
            IStoryRepository storyRepository,
            IDeckRepository deckRepository,
            ICardRepository cardRepository,
            IAiContextService aiContextService,
            ILogger<StoryService> logger)
        {
            _storyRepository = storyRepository;
            _deckRepository = deckRepository;
            _cardRepository = cardRepository;
            _aiContextService = aiContextService;
            _logger = logger;
        }

        public async Task<StoryDto> GenerateStoryAsync(GenerateStoryDto dto, Guid userId)
        {
            await CheckStoryLimitAsync(userId);

            var deck = await GetDeckOrThrowAsync(dto.DeckId, userId);

            var wordsList = await GetWordsForStoryAsync(dto.DeckId, MaxUserWords);

            // Динамічно визначаємо розмір словника залежно від рівня
            int targetTotalWords = GetTargetVocabularySize(deck.ProficiencyLevel);
            int aiNewWordsCount = Math.Max(5, targetTotalWords - wordsList.Count);

            _logger.LogInformation("Generating story for user {UserId}, Deck: {DeckId}, Genre: {Genre}. User words: {UserWords}, AI words: {AiWords}",
                userId, deck.Id, dto.Genre, wordsList.Count, aiNewWordsCount);

            var aiResult = await _aiContextService.GenerateStoryAsync(
                wordsList, deck.TargetLanguage, deck.NativeLanguage, deck.ProficiencyLevel, dto.Genre, aiNewWordsCount);

            var storyEntity = CreateStoryEntity(aiResult, dto.Genre, deck.Id, userId);
            await _storyRepository.CreateAsync(storyEntity);

            return MapToStoryDto(storyEntity);
        }

        public async Task<List<StoryDto>> GetUserStoriesAsync(Guid userId)
        {
            var stories = await _storyRepository.GetByUserIdAsync(userId);
            return stories.Select(MapToStoryDto).ToList();
        }

        public async Task<StoryDto> GetStoryByIdAsync(Guid id, Guid userId)
        {
            var story = await _storyRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Story", id);

            if (story.CreatedId != userId)
                throw new UnauthorizedAccessException("You don't have access to this story.");

            return MapToStoryDto(story);
        }

        public async Task DeleteStoryAsync(Guid id, Guid userId)
        {
            var story = await _storyRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Story", id);

            if (story.CreatedId != userId)
                throw new UnauthorizedAccessException("You don't have access to this story.");

            await _storyRepository.DeleteAsync(story);
        }

        private async Task CheckStoryLimitAsync(Guid userId)
        {
            var storiesThisWeek = await _storyRepository.CountStoriesInLastWeekAsync(userId);
            if (storiesThisWeek >= 3)
            {
                throw new ValidationException("You have reached the limit of free stories per week (3/3). Try it later or switch to Premium.");
            }
        }

        private async Task<Deck> GetDeckOrThrowAsync(Guid deckId, Guid userId)
        {
            var deck = await _deckRepository.GetByIdAsync(deckId)
                ?? throw new NotFoundException("Deck", deckId);

            if (deck.CreatedId != userId)
                throw new UnauthorizedAccessException("You don't have access to this deck.");

            return deck;
        }

        private async Task<List<string>> GetWordsForStoryAsync(Guid deckId, int limit)
        {
            var cards = await _cardRepository.GetRandomCardsForStoryAsync(deckId, limit);
            if (!cards.Any())
            {
                throw new ValidationException("There are no words in this deck to generate a story. Add at least a few cards.");
            }

            return cards.Select(c => c.Front).ToList();
        }

        private static Story CreateStoryEntity(AiStoryResult aiResult, Domain.Enums.StoryGenre genre, Guid deckId, Guid userId)
        {
            return new Story
            {
                Title = aiResult.Title ?? "Untitled Story",
                Content = aiResult.Content ?? string.Empty,
                Genre = genre,
                DeckId = deckId,
                CreatedId = userId,
                Phrases = aiResult.Vocabulary?.Select(v => new StoryPhrase
                {
                    Phrase = v.Phrase ?? string.Empty,
                    Translation = v.Translation ?? string.Empty,
                    Reading = v.Reading ?? string.Empty
                }).ToList() ?? new List<StoryPhrase>()
            };
        }

        private static StoryDto MapToStoryDto(Story story)
        {
            return new StoryDto
            {
                Id = story.Id,
                Title = story.Title,
                Content = story.Content,
                Genre = story.Genre,
                DeckId = story.DeckId,
                CreatedAt = story.CreatedAt,
                Phrases = story.Phrases.Select(p => new StoryPhraseDto
                {
                    Id = p.Id,
                    Phrase = p.Phrase,
                    Translation = p.Translation,
                    Reading = p.Reading
                }).ToList()
            };
        }

        private static int GetTargetVocabularySize(ProficiencyLevel level)
        {
            return level switch
            {
                ProficiencyLevel.Beginner => 35,
                ProficiencyLevel.Intermediate => 20,
                ProficiencyLevel.Advanced => 12,
                _ => 20
            };
        }
    }
}