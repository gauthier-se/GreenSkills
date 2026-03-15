using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Manages the two-zone auth layout: dark branding zone + light form zone.
    /// Applies UITheme colors and updates the section label for Login/Register.
    /// </summary>
    public class AuthLayoutController : MonoBehaviour
    {
        [Header("Zone Backgrounds")]
        [SerializeField] private Image brandingZoneBackground;
        [SerializeField] private Image formZoneBackground;

        [Header("Decorative")]
        [SerializeField] private Image waveTransition;
        [SerializeField] private Image dotPatternOverlay;

        [Header("Section Label")]
        [SerializeField] private TextMeshProUGUI sectionLabel;

        [Header("Theme")]
        [SerializeField] private UITheme theme;

        private void Awake()
        {
            ApplyTheme();
        }

        /// <summary>
        /// Applies theme colors to zone backgrounds and decorative elements.
        /// </summary>
        public void ApplyTheme()
        {
            if (theme == null) return;

            if (brandingZoneBackground != null)
                brandingZoneBackground.color = theme.bgDark;

            if (formZoneBackground != null)
                formZoneBackground.color = theme.bgApp;

            if (waveTransition != null)
                waveTransition.color = theme.bgApp;

            if (dotPatternOverlay != null)
                dotPatternOverlay.color = new Color(
                    theme.primary.r, theme.primary.g, theme.primary.b, 0.03f);

            if (sectionLabel != null)
                sectionLabel.color = theme.textSecondary;
        }

        /// <summary>
        /// Updates the section label text (e.g. "CONNEXION" or "INSCRIPTION").
        /// </summary>
        public void SetSectionLabel(string label)
        {
            if (sectionLabel != null)
                sectionLabel.text = label;
        }
    }
}
