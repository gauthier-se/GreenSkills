using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Data;
using Managers;

namespace UI
{
    /// <summary>
    /// Controls the settings panel accessible from the main menu.
    /// Provides audio settings, account info, and logout functionality.
    /// </summary>
    public class SettingsPanelController : MonoBehaviour
    {
        [Header("Panel References")]
        [Tooltip("Root GameObject for the entire settings panel (overlay + content)")]
        [SerializeField] private GameObject panelRoot;

        [Tooltip("CanvasGroup used for fade-in/out animation")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Audio Settings")]
        [Tooltip("Slider controlling music volume (0 to 1)")]
        [SerializeField] private Slider musicVolumeSlider;

        [Tooltip("Slider controlling SFX volume (0 to 1)")]
        [SerializeField] private Slider sfxVolumeSlider;

        [Tooltip("Toggle to mute/unmute all audio")]
        [SerializeField] private Toggle muteToggle;

        [Header("Account Info")]
        [Tooltip("Text displaying the current user's username")]
        [SerializeField] private TMP_Text usernameText;

        [Tooltip("Text displaying the current user's email")]
        [SerializeField] private TMP_Text emailText;

        [Header("Actions")]
        [Tooltip("Button to log out and return to the login screen")]
        [SerializeField] private Button logoutButton;

        [Tooltip("Button to close the settings panel")]
        [SerializeField] private Button closeButton;

        [Header("App Info")]
        [Tooltip("Text displaying the application version")]
        [SerializeField] private TMP_Text versionText;

        [Header("Animation Settings")]
        [Tooltip("Duration of the fade-in/out animation in seconds")]
        [SerializeField] private float fadeDuration = 0.3f;

        [Header("Theme")]
        [SerializeField] private UITheme theme;

        [Header("Theme — Overlay & Panel")]
        [SerializeField] private Image overlayBackground;
        [SerializeField] private Image panelBackground;
        [SerializeField] private Image panelHeaderBar;
        [SerializeField] private RectTransform panelTransform;

        [Header("Theme — Slider Styling")]
        [SerializeField] private Image musicSliderFill;
        [SerializeField] private Image musicSliderHandle;
        [SerializeField] private Image musicSliderBackground;
        [SerializeField] private Image sfxSliderFill;
        [SerializeField] private Image sfxSliderHandle;
        [SerializeField] private Image sfxSliderBackground;

        [Header("Theme — Toggle Styling")]
        [SerializeField] private Image muteCheckmark;
        [SerializeField] private Image muteToggleBackground;

        [Header("Theme — Text Elements")]
        [SerializeField] private TMP_Text panelTitleText;
        [SerializeField] private TMP_Text musicLabel;
        [SerializeField] private TMP_Text sfxLabel;
        [SerializeField] private TMP_Text muteLabel;
        [SerializeField] private TMP_Text accountSectionTitle;

        /// <summary>
        /// Event fired when the settings panel is dismissed.
        /// </summary>
        public event Action OnPanelDismissed;

        private Coroutine _fadeCoroutine;

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }

            if (logoutButton != null)
            {
                logoutButton.onClick.AddListener(OnLogoutClicked);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            }

            if (muteToggle != null)
            {
                muteToggle.onValueChanged.AddListener(OnMuteToggleChanged);
            }

            ApplyTheme();
            HideImmediate();
        }

        /// <summary>
        /// Shows the settings panel with current values and a fade-in animation.
        /// </summary>
        public void Show()
        {
            if (panelRoot == null)
            {
                Debug.LogError("[SettingsPanelController] panelRoot is null!");
                return;
            }

            PopulateAudioSettings();
            PopulateAccountInfo();
            PopulateVersion();

            panelRoot.SetActive(true);
            FadeIn();

            Debug.Log("[SettingsPanelController] Settings panel opened");
        }

        /// <summary>
        /// Hides the settings panel with a fade-out animation.
        /// </summary>
        public void Hide()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(FadeOutAndHide());
        }

        /// <summary>
        /// Checks whether the panel is currently visible.
        /// </summary>
        public bool IsVisible()
        {
            return panelRoot != null && panelRoot.activeSelf;
        }

        #region UI Callbacks

        private void OnMusicVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicVolume(value);
            }
        }

        private void OnSfxVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSfxVolume(value);
            }
        }

        private void OnMuteToggleChanged(bool isMuted)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMuted(isMuted);
            }
        }

        private void OnLogoutClicked()
        {
            Debug.Log("[SettingsPanelController] Logout clicked");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.Logout();
            }
        }

        private void OnCloseClicked()
        {
            Debug.Log("[SettingsPanelController] Close clicked, dismissing panel");
            Hide();
        }

        #endregion

        #region Data Population

        private void PopulateAudioSettings()
        {
            if (AudioManager.Instance == null) return;

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.MusicVolume);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.SfxVolume);
            }

            if (muteToggle != null)
            {
                muteToggle.SetIsOnWithoutNotify(AudioManager.Instance.IsMuted);
            }
        }

        private void PopulateAccountInfo()
        {
            if (AuthManager.Instance == null) return;

            var user = AuthManager.Instance.CurrentUser;
            if (user == null) return;

            if (usernameText != null)
            {
                usernameText.text = user.username ?? "";
            }

            if (emailText != null)
            {
                emailText.text = user.email ?? "";
            }
        }

        private void PopulateVersion()
        {
            if (versionText != null)
            {
                versionText.text = $"v{Application.version}";
            }
        }

        #endregion

        #region Theme

        /// <summary>
        /// Applies theme colors to all panel elements.
        /// </summary>
        private void ApplyTheme()
        {
            if (theme == null) return;

            // Overlay — dark scrim
            if (overlayBackground != null)
                overlayBackground.color = new Color(
                    theme.bgDark.r, theme.bgDark.g, theme.bgDark.b, 0.6f);

            // Panel card
            if (panelBackground != null)
                panelBackground.color = theme.bgDarkSurface;

            // Header accent bar
            if (panelHeaderBar != null)
                panelHeaderBar.color = new Color(
                    theme.primary.r, theme.primary.g, theme.primary.b, 0.3f);

            // Title
            if (panelTitleText != null)
                panelTitleText.color = theme.textOnDark;

            // Labels
            if (musicLabel != null)
                musicLabel.color = theme.textOnDarkMuted;

            if (sfxLabel != null)
                sfxLabel.color = theme.textOnDarkMuted;

            if (muteLabel != null)
                muteLabel.color = theme.textOnDarkMuted;

            // Slider fills — brand green
            if (musicSliderFill != null)
                musicSliderFill.color = theme.primaryLight;

            if (sfxSliderFill != null)
                sfxSliderFill.color = theme.primaryLight;

            // Slider handles — white
            if (musicSliderHandle != null)
                musicSliderHandle.color = theme.textOnDark;

            if (sfxSliderHandle != null)
                sfxSliderHandle.color = theme.textOnDark;

            // Slider backgrounds — subtle track
            Color sliderTrack = new Color(
                theme.neutral500.r, theme.neutral500.g, theme.neutral500.b, 0.4f);

            if (musicSliderBackground != null)
                musicSliderBackground.color = sliderTrack;

            if (sfxSliderBackground != null)
                sfxSliderBackground.color = sliderTrack;

            // Mute toggle
            if (muteCheckmark != null)
                muteCheckmark.color = theme.primaryLight;

            if (muteToggleBackground != null)
                muteToggleBackground.color = new Color(
                    theme.neutral500.r, theme.neutral500.g, theme.neutral500.b, 0.5f);

            // Account section
            if (accountSectionTitle != null)
                accountSectionTitle.color = theme.textOnDark;

            if (usernameText != null)
                usernameText.color = theme.textOnDark;

            if (emailText != null)
                emailText.color = theme.textOnDarkMuted;

            // Logout button — red
            if (logoutButton != null)
            {
                var colors = logoutButton.colors;
                colors.normalColor = theme.error;
                colors.highlightedColor = theme.error;
                colors.pressedColor = new Color(
                    theme.error.r * 0.8f, theme.error.g * 0.8f, theme.error.b * 0.8f, 1f);
                colors.disabledColor = theme.neutral300;
                logoutButton.colors = colors;
            }

            // Close button — subtle
            if (closeButton != null)
            {
                var colors = closeButton.colors;
                colors.normalColor = new Color(
                    theme.textOnDarkMuted.r, theme.textOnDarkMuted.g, theme.textOnDarkMuted.b, 0.5f);
                colors.highlightedColor = theme.textOnDarkMuted;
                colors.pressedColor = theme.textOnDarkMuted;
                colors.disabledColor = theme.neutral300;
                closeButton.colors = colors;
            }

            // Version text — very subtle
            if (versionText != null)
                versionText.color = new Color(
                    theme.textOnDarkMuted.r, theme.textOnDarkMuted.g, theme.textOnDarkMuted.b, 0.35f);
        }

        #endregion

        #region Animation

        private void FadeIn()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            if (canvasGroup != null)
            {
                _fadeCoroutine = StartCoroutine(FadeCanvasGroup(0f, 1f, fadeDuration));
            }
        }

        private IEnumerator FadeOutAndHide()
        {
            if (canvasGroup != null)
            {
                yield return FadeCanvasGroup(1f, 0f, fadeDuration);
            }

            HideImmediate();
            OnPanelDismissed?.Invoke();
        }

        private IEnumerator FadeCanvasGroup(float from, float to, float duration)
        {
            if (canvasGroup == null)
            {
                yield break;
            }

            bool isFadingIn = to > from;
            float scaleFrom = isFadingIn ? 0.95f : 1f;
            float scaleTo = isFadingIn ? 1f : 0.95f;

            canvasGroup.alpha = from;
            if (panelTransform != null)
                panelTransform.localScale = Vector3.one * scaleFrom;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                canvasGroup.alpha = Mathf.Lerp(from, to, t);

                if (panelTransform != null)
                    panelTransform.localScale = Vector3.one * Mathf.Lerp(scaleFrom, scaleTo, t);

                yield return null;
            }

            canvasGroup.alpha = to;
            if (panelTransform != null)
                panelTransform.localScale = Vector3.one * scaleTo;
        }

        private void HideImmediate()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }

        #endregion

        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseClicked);
            }

            if (logoutButton != null)
            {
                logoutButton.onClick.RemoveListener(OnLogoutClicked);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            }

            if (muteToggle != null)
            {
                muteToggle.onValueChanged.RemoveListener(OnMuteToggleChanged);
            }

            OnPanelDismissed = null;
        }
    }
}
