using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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

            canvasGroup.alpha = from;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                canvasGroup.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            canvasGroup.alpha = to;
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
