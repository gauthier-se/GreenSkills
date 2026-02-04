using System.Collections.Generic;
using UnityEngine;

namespace Data.Exercises
{
    /// <summary>
    /// Factory class responsible for creating exercise data objects from DTOs.
    /// Handles the conversion from JSON-deserialized data to Unity ScriptableObjects.
    /// </summary>
    public static class ExerciseFactory
    {
        /// <summary>
        /// Creates the appropriate exercise data object based on the exercise type.
        /// </summary>
        /// <param name="dto">The generic DTO containing exercise data</param>
        /// <returns>A BaseExerciseData subclass instance, or null if type is unknown</returns>
        public static BaseExerciseData CreateFromDto(GenericExerciseDto dto)
        {
            if (dto == null)
            {
                Debug.LogError("ExerciseFactory: Received null DTO");
                return null;
            }

            // Parse exercise type (default to Quiz for backward compatibility)
            ExerciseType type = ParseExerciseType(dto.exerciseType);

            BaseExerciseData exercise = type switch
            {
                ExerciseType.Quiz => CreateQuizExercise(dto),
                ExerciseType.TrueFalse => CreateTrueFalseExercise(dto),
                ExerciseType.FillInBlank => CreateFillInBlankExercise(dto),
                ExerciseType.Sorting => CreateSortingExercise(dto),
                ExerciseType.Matching => CreateMatchingExercise(dto),
                _ => CreateQuizExercise(dto) // Default fallback
            };

            // Set common properties
            if (exercise != null)
            {
                exercise.explanation = dto.explanation;
                exercise.difficulty = (Difficulty)dto.difficulty;
                exercise.category = (Category)dto.category;

                if (!string.IsNullOrEmpty(dto.imageName))
                {
                    exercise.image = Resources.Load<Sprite>(dto.imageName);
                    if (exercise.image == null)
                    {
                        Debug.LogWarning($"Image '{dto.imageName}' not found in Resources");
                    }
                }
            }

            return exercise;
        }

        /// <summary>
        /// Creates a list of exercise data objects from an array of DTOs.
        /// </summary>
        public static List<BaseExerciseData> CreateFromDtos(GenericExerciseDto[] dtos)
        {
            var exercises = new List<BaseExerciseData>();

            if (dtos == null || dtos.Length == 0)
            {
                Debug.LogWarning("ExerciseFactory: No exercises to convert");
                return exercises;
            }

            foreach (var dto in dtos)
            {
                var exercise = CreateFromDto(dto);
                if (exercise != null)
                {
                    exercises.Add(exercise);
                }
            }

            return exercises;
        }

        /// <summary>
        /// Parses a string to ExerciseType enum.
        /// </summary>
        private static ExerciseType ParseExerciseType(string typeString)
        {
            if (string.IsNullOrEmpty(typeString))
            {
                return ExerciseType.Quiz; // Default for backward compatibility
            }

            return typeString.ToLower() switch
            {
                "quiz" => ExerciseType.Quiz,
                "truefalse" or "true_false" or "true-false" => ExerciseType.TrueFalse,
                "fillinblank" or "fill_in_blank" or "fill-in-blank" or "fillintheblank" => ExerciseType.FillInBlank,
                "sorting" or "sort" or "categorize" => ExerciseType.Sorting,
                "matching" or "match" or "connect" => ExerciseType.Matching,
                _ => ExerciseType.Quiz
            };
        }

        /// <summary>
        /// Creates a Quiz exercise from DTO.
        /// </summary>
        private static QuizExerciseData CreateQuizExercise(GenericExerciseDto dto)
        {
            var exercise = ScriptableObject.CreateInstance<QuizExerciseData>();
            exercise.questionText = dto.questionText;
            exercise.options = dto.options != null ? new List<string>(dto.options) : new List<string>();
            exercise.correctOptionIndex = dto.correctOptionIndex;
            return exercise;
        }

        /// <summary>
        /// Creates a TrueFalse exercise from DTO.
        /// </summary>
        private static TrueFalseExerciseData CreateTrueFalseExercise(GenericExerciseDto dto)
        {
            var exercise = ScriptableObject.CreateInstance<TrueFalseExerciseData>();
            exercise.statement = dto.statement;
            exercise.isTrue = dto.isTrue;
            return exercise;
        }

        /// <summary>
        /// Creates a FillInBlank exercise from DTO.
        /// </summary>
        private static FillInBlankExerciseData CreateFillInBlankExercise(GenericExerciseDto dto)
        {
            var exercise = ScriptableObject.CreateInstance<FillInBlankExerciseData>();
            exercise.sentenceWithBlanks = dto.sentenceWithBlanks;
            exercise.correctAnswers = dto.correctAnswers != null ? new List<string>(dto.correctAnswers) : new List<string>();
            exercise.wordOptions = dto.wordOptions != null ? new List<string>(dto.wordOptions) : new List<string>();
            exercise.caseSensitive = dto.caseSensitive;
            return exercise;
        }

        /// <summary>
        /// Creates a Sorting exercise from DTO.
        /// </summary>
        private static SortingExerciseData CreateSortingExercise(GenericExerciseDto dto)
        {
            var exercise = ScriptableObject.CreateInstance<SortingExerciseData>();
            exercise.instruction = dto.instruction;

            // Convert categories
            exercise.categories = new List<SortingCategory>();
            if (dto.categories != null)
            {
                foreach (var catDto in dto.categories)
                {
                    var category = new SortingCategory
                    {
                        categoryName = catDto.categoryName,
                        categoryColor = ParseColor(catDto.categoryColor)
                    };

                    if (!string.IsNullOrEmpty(catDto.categoryIconName))
                    {
                        category.categoryIcon = Resources.Load<Sprite>(catDto.categoryIconName);
                    }

                    exercise.categories.Add(category);
                }
            }

            // Convert items
            exercise.items = new List<SortableItem>();
            if (dto.items != null)
            {
                foreach (var itemDto in dto.items)
                {
                    var item = new SortableItem
                    {
                        itemName = itemDto.itemName,
                        correctCategoryIndex = itemDto.correctCategoryIndex
                    };

                    if (!string.IsNullOrEmpty(itemDto.itemSpriteName))
                    {
                        item.itemSprite = Resources.Load<Sprite>(itemDto.itemSpriteName);
                    }

                    exercise.items.Add(item);
                }
            }

            return exercise;
        }

        /// <summary>
        /// Creates a Matching exercise from DTO.
        /// </summary>
        private static MatchingExerciseData CreateMatchingExercise(GenericExerciseDto dto)
        {
            var exercise = ScriptableObject.CreateInstance<MatchingExerciseData>();
            exercise.instruction = dto.instruction;
            exercise.leftColumnHeader = dto.leftColumnHeader ?? "Actions";
            exercise.rightColumnHeader = dto.rightColumnHeader ?? "Impacts";
            exercise.shuffleRightColumn = dto.shuffleRightColumn;

            // Convert pairs
            exercise.pairs = new List<MatchPair>();
            if (dto.pairs != null)
            {
                foreach (var pairDto in dto.pairs)
                {
                    var pair = new MatchPair
                    {
                        leftItem = pairDto.leftItem,
                        rightItem = pairDto.rightItem
                    };

                    if (!string.IsNullOrEmpty(pairDto.leftSpriteName))
                    {
                        pair.leftSprite = Resources.Load<Sprite>(pairDto.leftSpriteName);
                    }

                    if (!string.IsNullOrEmpty(pairDto.rightSpriteName))
                    {
                        pair.rightSprite = Resources.Load<Sprite>(pairDto.rightSpriteName);
                    }

                    exercise.pairs.Add(pair);
                }
            }

            return exercise;
        }

        /// <summary>
        /// Parses a hex color string to Unity Color.
        /// </summary>
        private static Color ParseColor(string hexColor)
        {
            if (string.IsNullOrEmpty(hexColor))
            {
                return Color.white;
            }

            if (ColorUtility.TryParseHtmlString(hexColor, out Color color))
            {
                return color;
            }

            return Color.white;
        }
    }
}
