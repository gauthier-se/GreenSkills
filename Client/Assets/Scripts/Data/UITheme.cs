using System;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "GreenSkills/UI Theme")]
    public class UITheme : ScriptableObject
    {
        [Header("Brand / Primary")]
        public Color primary = HexColor("#1d7a4f");
        public Color primaryLight = HexColor("#2aa86e");
        public Color primaryDark = HexColor("#0f5435");
        public Color primarySubtle = HexColor("#e4f8ea");

        [Header("Backgrounds")]
        public Color bgApp = HexColor("#f2f7f4");
        public Color bgSurface = HexColor("#e4f8ea");
        public Color bgCard = HexColor("#ffffff");
        public Color bgDark = HexColor("#0a2e1c");
        public Color bgDarkSurface = HexColor("#132b1e");
        public Color bgQuestionZone = HexColor("#0f3d24");

        [Header("Borders")]
        public Color borderDefault = HexColor("#d9e6da");
        public Color borderStrong = HexColor("#a8cfbc");
        public Color borderBrand = HexColor("#1d7a4f");
        public Color borderNeutral = HexColor("#e2e2da");

        [Header("Category Colors")]
        public Color itAzur = HexColor("#1b6fa8");
        public Color itAzurSubtle = HexColor("#ebf3f8");
        public Color solaire = HexColor("#e8a22b");
        public Color solaireSubtle = HexColor("#fdf3e6");
        public Color rseCyan = HexColor("#3d8b8b");
        public Color rseCyanSubtle = HexColor("#ebf6f7");
        public Color gouvViolet = HexColor("#9b59b6");
        public Color gouvVioletSubtle = HexColor("#f6eef8");

        [Header("Semantic / Feedback")]
        public Color success = HexColor("#2aa86e");
        public Color successBg = HexColor("#ebf7f2");
        public Color warning = HexColor("#e8a22b");
        public Color warningBg = HexColor("#fdf3e6");
        public Color error = HexColor("#d94f3d");
        public Color errorBg = HexColor("#fceeec");

        [Header("Text")]
        public Color textPrimary = HexColor("#1a1a18");
        public Color textSecondary = HexColor("#4a5758");
        public Color textMuted = HexColor("#7a8f86");
        public Color textOnDark = HexColor("#ffffff");
        public Color textOnDarkMuted = HexColor("#cbe6d4");

        [Header("Neutrals")]
        public Color neutral50 = HexColor("#f5f5f5");
        public Color neutral100 = HexColor("#e2e2da");
        public Color neutral300 = HexColor("#afafab");
        public Color neutral500 = HexColor("#6e7470");
        public Color neutral900 = HexColor("#2c2c2a");

        /// <summary>
        /// Returns the category color for the given Category enum value.
        /// </summary>
        public Color GetCategoryColor(Category category)
        {
            return category switch
            {
                Category.Environment => primary,
                Category.Social => rseCyan,
                Category.Governance => gouvViolet,
                Category.Economy => solaire,
                _ => primary
            };
        }

        /// <summary>
        /// Returns the subtle (background) variant of a category color.
        /// </summary>
        public Color GetCategorySubtleColor(Category category)
        {
            return category switch
            {
                Category.Environment => primarySubtle,
                Category.Social => rseCyanSubtle,
                Category.Governance => gouvVioletSubtle,
                Category.Economy => solaireSubtle,
                _ => primarySubtle
            };
        }

        static Color HexColor(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }
    }
}
