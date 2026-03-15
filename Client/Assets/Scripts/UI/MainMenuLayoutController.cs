using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Manages the two-zone main menu layout: dark header zone + light levels zone.
    /// Applies UITheme colors to backgrounds, arc transition, and decorative elements.
    /// </summary>
    public class MainMenuLayoutController : MonoBehaviour
    {
        [Header("Zone Backgrounds")]
        [SerializeField] private Image headerZoneBackground;
        [SerializeField] private Image levelsZoneBackground;

        [Header("Arc Transition")]
        [SerializeField] private Image arcTransition;
        [SerializeField] private Image separatorLine;

        [Header("Decorative")]
        [SerializeField] private Image headerGlow;
        [SerializeField] private Image dotPatternOverlay;

        [Header("Section Labels")]
        [SerializeField] private TextMeshProUGUI statsLabel;
        [SerializeField] private TextMeshProUGUI levelsLabel;

        [Header("Scroll Hint")]
        [SerializeField] private Image[] scrollArrows;

        [Header("Theme")]
        [SerializeField] private UITheme theme;

        private void Awake()
        {
            ApplyTheme();
        }

        /// <summary>
        /// Applies theme colors to all layout elements.
        /// </summary>
        public void ApplyTheme()
        {
            if (theme == null) return;

            if (headerZoneBackground != null)
                headerZoneBackground.color = theme.bgDark;

            if (levelsZoneBackground != null)
                levelsZoneBackground.color = theme.bgApp;

            if (arcTransition != null)
                arcTransition.color = theme.bgDark;

            if (separatorLine != null)
                separatorLine.color = new Color(
                    theme.primary.r, theme.primary.g, theme.primary.b, 0.3f);

            if (headerGlow != null)
                headerGlow.color = new Color(
                    theme.primary.r, theme.primary.g, theme.primary.b, 0.12f);

            if (dotPatternOverlay != null)
                dotPatternOverlay.color = new Color(
                    theme.primary.r, theme.primary.g, theme.primary.b, 0.03f);

            if (statsLabel != null)
                statsLabel.color = new Color(
                    theme.textOnDarkMuted.r, theme.textOnDarkMuted.g, theme.textOnDarkMuted.b, 0.35f);

            if (levelsLabel != null)
                levelsLabel.color = new Color(
                    theme.primary.r, theme.primary.g, theme.primary.b, 0.4f);

            if (scrollArrows != null)
            {
                float[] opacities = { 0.3f, 0.15f, 0.075f };
                for (int i = 0; i < scrollArrows.Length && i < opacities.Length; i++)
                {
                    if (scrollArrows[i] != null)
                        scrollArrows[i].color = new Color(
                            theme.primary.r, theme.primary.g, theme.primary.b, opacities[i]);
                }
            }
        }
    }
}
