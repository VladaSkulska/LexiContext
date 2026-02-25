using LexiContext.Application.Services;

namespace LexiContext.Tests.Services
{
    public class SpacedRepetitionServiceTests
    {
        private readonly SpacedRepetitionService _service;
        public SpacedRepetitionServiceTests()
        {
            _service = new SpacedRepetitionService();
        }

        [Fact]
        public void CalculateNextReview_ShouldResetProgress_OnFail()
        {
            // Arrange
            int currentRepetitions = 5;
            int currentIntervalDays = 14;
            double currentEaseFactor = 2.5;

            // Act
            var result = _service.CalculateNextReview(currentRepetitions,
                currentIntervalDays,
                currentEaseFactor,
                Domain.Enums.RecallQuality.Fail);

            // Assert
            Assert.Equal(0, result.Repetitions);
            Assert.Equal(1, result.IntervalDays);
            Assert.True(result.EaseFactor < currentEaseFactor);
        }

        [Theory]
        [InlineData(0, 0, 1)]
        [InlineData(1, 1, 3)]
        public void CalculateNextReview_ShouldSetFixedIntervals_ForFirstTwoSuccess(int currentRepetitions,
            int currentIntervalDays, double expectedInterval)
        {
            // Arrange
            double currentEaseFactor = 2.5;

            // Act
            var result = _service.CalculateNextReview(currentRepetitions,
                currentIntervalDays,
                currentEaseFactor,
                Domain.Enums.RecallQuality.Easy);

            // Assert

            Assert.Equal(currentRepetitions + 1, result.Repetitions);
            Assert.Equal(expectedInterval, result.IntervalDays);
        }

        [Fact]
        public void CalculateNextReview_ShouldNotExceedMaxInterval()
        {
            // Arrange
            int currentRepetitions = 14;
            int currentIntervalDays = 3000;
            double currentEaseFactor = 2.5;

            // Act
            var result = _service.CalculateNextReview(currentRepetitions,
                currentIntervalDays,
                currentEaseFactor,
                Domain.Enums.RecallQuality.Easy);

            // Assert
            Assert.Equal(result.IntervalDays, 3650);
        }

        [Fact]
        public void CalculateNextReview_ShouldNotGoBelowMinEaseFactor()
        {
            // Arrange
            int currentRepetitions = 5;
            int currentIntervalDays = 14;
            double currentEaseFactor = 1.3;

            // Act
            var result = _service.CalculateNextReview(currentRepetitions,
                currentIntervalDays,
                currentEaseFactor,
                Domain.Enums.RecallQuality.Fail);

            // Assert
            Assert.Equal(1.3, result.EaseFactor);
        }

        [Fact]
        public void CalculateNextReview_ShouldCalculateCorrectly_AfterThirdSuccess() 
        {
            // Arrange
            int currentRepetitions = 2;
            int currentIntervalDays = 3;
            double currentEaseFactor = 2.5;

            // Act
            var result = _service.CalculateNextReview(currentRepetitions,
                currentIntervalDays,
                currentEaseFactor,
                Domain.Enums.RecallQuality.Easy);

            // Assert
            Assert.Equal(currentRepetitions + 1, result.Repetitions);
            Assert.Equal((int)(currentIntervalDays * result.EaseFactor), result.IntervalDays);
        }
    }
}
