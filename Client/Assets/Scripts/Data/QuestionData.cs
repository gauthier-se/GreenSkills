using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// Represents the difficulty level of a question.
    /// </summary>
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    /// <summary>
    /// Categorizes questions by RSE (Corporate Social Responsibility) topics.
    /// </summary>
    public enum Category
    {
        // RSE - Fundamentals
        RseBasics,
        SustainabilityPrinciples,
        CorporateResponsibility,

        // Environment
        Environment,
        ClimateChange,
        Biodiversity,
        NaturalResources,

        // Company Environmental Impact
        CarbonFootprint,
        EnergyConsumption,
        WasteManagement,
        LifeCycleAssessment,

        // Energy & Digital Responsibility
        RenewableEnergy,
        EnergyEfficiency,
        GreenIT,
        DigitalSobriety,

        // Mobility
        SustainableMobility,
        BusinessTravel,
        RemoteWork,

        // Circular Economy
        CircularEconomy,
        EcoDesign,
        Recycling,
        WasteReduction,

        // Social Responsibility
        QualityOfWorkLife,
        HealthAndSafety,
        DiversityAndInclusion,
        DisabilityInclusion,

        // Governance & Ethics
        Governance,
        Ethics,
        AntiCorruption,
        Transparency,

        // Regulations & Standards
        Regulations,
        ISO26000,
        CSRD,
        DutyOfVigilance,

        // Responsible Purchasing
        ResponsiblePurchasing,
        SupplyChainResponsibility,

        // Measurement & Reporting
        RseIndicators,
        NonFinancialReporting,
        ESG,

        // Daily Best Practices
        EcoGestures,
        DigitalBestPractices,
        OfficeSustainability,

        // Awareness & Greenwashing
        Greenwashing,
        Misconceptions
    }

    /// <summary>
    /// ScriptableObject containing data for a single quiz question.
    /// </summary>
    [CreateAssetMenu(fileName = "NewQuestion", menuName = "RSE/Question")]
    public class QuestionData : ScriptableObject
    {
        [Tooltip("Unique identifier for the question")]
        public int id;

        [TextArea(3, 10)]
        [Tooltip("The question text displayed to the player")]
        public string questionText;

        [Tooltip("List of answer options")]
        public List<string> options;

        [TextArea(3, 10)]
        [Tooltip("Explanation shown after answering")]
        public string explanation;

        [Tooltip("Index of the correct answer (0-based)")]
        public int correctOptionIndex;

        [Tooltip("Optional image to display with the question")]
        public Sprite image;

        [Tooltip("Difficulty level of the question")]
        public Difficulty difficulty;

        [Tooltip("RSE category this question belongs to")]
        public Category category;
    }
}
