namespace Data.Exercises
{
    /// <summary>
    /// Defines the different types of mini-games/exercises available in the game.
    /// Each type has its own UI panel and validation logic.
    /// </summary>
    public enum ExerciseType
    {
        /// <summary>
        /// Multiple Choice Quiz - Select one correct answer from multiple options.
        /// </summary>
        Quiz,

        /// <summary>
        /// True/False - Binary choice for a given statement.
        /// </summary>
        TrueFalse,

        /// <summary>
        /// Fill in the Blank - Complete a sentence with missing words.
        /// </summary>
        FillInBlank,

        /// <summary>
        /// Sorting/Categorization - Drag and drop items into correct categories.
        /// </summary>
        Sorting,

        /// <summary>
        /// Matching - Connect related items from two columns.
        /// </summary>
        Matching
    }
}
