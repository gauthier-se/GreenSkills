using System.Collections.Generic;

namespace Data.Exercises
{
    /// <summary>
    /// Base Data Transfer Object for deserializing exercise data from API/JSON.
    /// Contains common fields shared by all exercise types.
    /// </summary>
    [System.Serializable]
    public class BaseExerciseDto
    {
        public string exerciseType;
        public string explanation;
        public int difficulty;
        public int category;
        public string imageName;
    }

    /// <summary>
    /// DTO for Quiz (Multiple Choice) exercises.
    /// </summary>
    [System.Serializable]
    public class QuizExerciseDto : BaseExerciseDto
    {
        public string questionText;
        public string[] options;
        public int correctOptionIndex;
    }

    /// <summary>
    /// DTO for True/False exercises.
    /// </summary>
    [System.Serializable]
    public class TrueFalseExerciseDto : BaseExerciseDto
    {
        public string statement;
        public bool isTrue;
    }

    /// <summary>
    /// DTO for Fill in the Blank exercises.
    /// </summary>
    [System.Serializable]
    public class FillInBlankExerciseDto : BaseExerciseDto
    {
        public string sentenceWithBlanks;
        public string[] correctAnswers;
        public string[] wordOptions;
        public bool caseSensitive;
    }

    /// <summary>
    /// DTO for sortable items in Sorting exercises.
    /// </summary>
    [System.Serializable]
    public class SortableItemDto
    {
        public string itemName;
        public string itemSpriteName;
        public int correctCategoryIndex;
    }

    /// <summary>
    /// DTO for sorting categories.
    /// </summary>
    [System.Serializable]
    public class SortingCategoryDto
    {
        public string categoryName;
        public string categoryIconName;
        public string categoryColor; // Hex color string
    }

    /// <summary>
    /// DTO for Sorting exercises.
    /// </summary>
    [System.Serializable]
    public class SortingExerciseDto : BaseExerciseDto
    {
        public string instruction;
        public SortingCategoryDto[] categories;
        public SortableItemDto[] items;
    }

    /// <summary>
    /// DTO for match pairs in Matching exercises.
    /// </summary>
    [System.Serializable]
    public class MatchPairDto
    {
        public string leftItem;
        public string rightItem;
        public string leftSpriteName;
        public string rightSpriteName;
    }

    /// <summary>
    /// DTO for Matching exercises.
    /// </summary>
    [System.Serializable]
    public class MatchingExerciseDto : BaseExerciseDto
    {
        public string instruction;
        public string leftColumnHeader;
        public string rightColumnHeader;
        public MatchPairDto[] pairs;
        public bool shuffleRightColumn;
    }

    /// <summary>
    /// Generic exercise DTO that can hold any exercise type.
    /// Uses a discriminator field to determine the actual type.
    /// </summary>
    [System.Serializable]
    public class GenericExerciseDto
    {
        // Common fields
        public string exerciseType;
        public string explanation;
        public int difficulty;
        public int category;
        public string imageName;

        // Quiz fields
        public string questionText;
        public string[] options;
        public int correctOptionIndex;

        // TrueFalse fields
        public string statement;
        public bool isTrue;

        // FillInBlank fields
        public string sentenceWithBlanks;
        public string[] correctAnswers;
        public string[] wordOptions;
        public bool caseSensitive;

        // Sorting fields
        public string instruction;
        public SortingCategoryDto[] categories;
        public SortableItemDto[] items;

        // Matching fields
        public string leftColumnHeader;
        public string rightColumnHeader;
        public MatchPairDto[] pairs;
        public bool shuffleRightColumn;
    }

    /// <summary>
    /// DTO for a level containing multiple exercises.
    /// </summary>
    [System.Serializable]
    public class LevelWithExercisesDto
    {
        public int levelId;
        public string theme;
        public GenericExerciseDto[] exercises;
    }

    /// <summary>
    /// Wrapper for deserializing multiple levels from JSON.
    /// </summary>
    [System.Serializable]
    public class AllLevelsWithExercisesDto
    {
        public LevelWithExercisesDto[] levels;
    }
}
