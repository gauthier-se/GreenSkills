using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

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

    [CreateAssetMenu(fileName = "NewQuestion", menuName = "RSE/Question")]
    public class QuestionData : ScriptableObject
    {
        public int id;
        [TextArea(3, 10)]
        public string questionText;
        public List<string> options;
        [TextArea(3, 10)]
        public string explanation;
        public int correctOptionIndex;
        public Sprite image;
        public Difficulty difficulty;
        public Category category;
    }
}