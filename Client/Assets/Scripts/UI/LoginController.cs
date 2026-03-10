using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Controls the login screen UI.
    /// Handles email/password input, login button, error display,
    /// and navigation to the register panel.
    /// </summary>
    public class LoginController : MonoBehaviour
    {
        [Header("Input Fields")]
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private TMP_InputField passwordInput;

        [Header("Buttons")]
        [SerializeField] private Button loginButton;
        [SerializeField] private Button goToRegisterButton;

        [Header("Feedback")]
        [SerializeField] private TextMeshProUGUI errorText;

        [Header("Panel References")]
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private GameObject registerPanel;

        private bool _isLoading;

        private void Awake()
        {
            if (loginButton != null)
            {
                loginButton.onClick.AddListener(OnLoginClick);
            }

            if (goToRegisterButton != null)
            {
                goToRegisterButton.onClick.AddListener(OnGoToRegisterClick);
            }
        }

        private void OnEnable()
        {
            ClearError();
            ClearInputs();
        }

        /// <summary>
        /// Handles the login button click.
        /// Validates input, calls AuthManager, and navigates on success.
        /// </summary>
        private async void OnLoginClick()
        {
            if (_isLoading)
            {
                return;
            }

            ClearError();

            string email = emailInput?.text?.Trim();
            string password = passwordInput?.text;

            // Client-side validation
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowError("Veuillez remplir tous les champs.");
                return;
            }

            if (!email.Contains("@"))
            {
                ShowError("Veuillez entrer une adresse email valide.");
                return;
            }

            SetLoading(true);

            string error = await AuthManager.Instance.LoginAsync(email, password);

            SetLoading(false);

            if (error != null)
            {
                ShowError(error);
                return;
            }

            Debug.Log("[LoginController] Login successful, navigating to main menu.");
            NavigateToMainMenu();
        }

        /// <summary>
        /// Switches to the register panel.
        /// </summary>
        private void OnGoToRegisterClick()
        {
            if (loginPanel != null)
            {
                loginPanel.SetActive(false);
            }

            if (registerPanel != null)
            {
                registerPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Navigates to the main menu scene after successful authentication.
        /// </summary>
        private void NavigateToMainMenu()
        {
            if (GameManager.Instance?.sceneManager != null)
            {
                GameManager.Instance.sceneManager.LoadScene("MainMenu");
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        }

        /// <summary>
        /// Shows an error message to the user.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        private void ShowError(string message)
        {
            if (errorText != null)
            {
                errorText.text = message;
                errorText.gameObject.SetActive(true);
            }

            Debug.LogWarning($"[LoginController] {message}");
        }

        /// <summary>
        /// Clears the error message.
        /// </summary>
        private void ClearError()
        {
            if (errorText != null)
            {
                errorText.text = "";
                errorText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Clears the input fields.
        /// </summary>
        private void ClearInputs()
        {
            if (emailInput != null)
            {
                emailInput.text = "";
            }

            if (passwordInput != null)
            {
                passwordInput.text = "";
            }
        }

        /// <summary>
        /// Sets the loading state. Disables interactions while loading.
        /// </summary>
        /// <param name="loading">Whether the login request is in progress.</param>
        private void SetLoading(bool loading)
        {
            _isLoading = loading;

            if (loginButton != null)
            {
                loginButton.interactable = !loading;
            }

            if (emailInput != null)
            {
                emailInput.interactable = !loading;
            }

            if (passwordInput != null)
            {
                passwordInput.interactable = !loading;
            }

            // Update button text to show loading state
            var buttonText = loginButton?.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = loading ? "Connexion..." : "Se connecter";
            }
        }

        private void OnDestroy()
        {
            if (loginButton != null)
            {
                loginButton.onClick.RemoveListener(OnLoginClick);
            }

            if (goToRegisterButton != null)
            {
                goToRegisterButton.onClick.RemoveListener(OnGoToRegisterClick);
            }
        }
    }
}
