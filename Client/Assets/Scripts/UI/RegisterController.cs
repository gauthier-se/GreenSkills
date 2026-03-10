using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Controls the register screen UI.
    /// Handles username/email/password input, registration button, error display,
    /// client-side validation, and navigation back to the login panel.
    /// </summary>
    public class RegisterController : MonoBehaviour
    {
        [Header("Input Fields")]
        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private TMP_InputField passwordInput;

        [Header("Buttons")]
        [SerializeField] private Button registerButton;
        [SerializeField] private Button goToLoginButton;

        [Header("Feedback")]
        [SerializeField] private TextMeshProUGUI errorText;

        [Header("Panel References")]
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private GameObject registerPanel;

        private const int MIN_USERNAME_LENGTH = 3;
        private const int MIN_PASSWORD_LENGTH = 8;

        private bool _isLoading;

        private void Awake()
        {
            if (registerButton != null)
            {
                registerButton.onClick.AddListener(OnRegisterClick);
            }

            if (goToLoginButton != null)
            {
                goToLoginButton.onClick.AddListener(OnGoToLoginClick);
            }
        }

        private void OnEnable()
        {
            ClearError();
            ClearInputs();
        }

        /// <summary>
        /// Handles the register button click.
        /// Validates input, calls AuthManager, and navigates on success.
        /// </summary>
        private async void OnRegisterClick()
        {
            if (_isLoading)
            {
                return;
            }

            ClearError();

            string username = usernameInput?.text?.Trim();
            string email = emailInput?.text?.Trim();
            string password = passwordInput?.text;

            // Client-side validation (mirrors server rules)
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowError("Veuillez remplir tous les champs.");
                return;
            }

            if (username.Length < MIN_USERNAME_LENGTH)
            {
                ShowError($"Le nom d'utilisateur doit contenir au moins {MIN_USERNAME_LENGTH} caractères.");
                return;
            }

            if (!email.Contains("@"))
            {
                ShowError("Veuillez entrer une adresse email valide.");
                return;
            }

            if (password.Length < MIN_PASSWORD_LENGTH)
            {
                ShowError($"Le mot de passe doit contenir au moins {MIN_PASSWORD_LENGTH} caractères.");
                return;
            }

            SetLoading(true);

            string error = await AuthManager.Instance.RegisterAsync(email, username, password);

            SetLoading(false);

            if (error != null)
            {
                ShowError(error);
                return;
            }

            Debug.Log("[RegisterController] Registration successful, navigating to main menu.");
            NavigateToMainMenu();
        }

        /// <summary>
        /// Switches back to the login panel.
        /// </summary>
        private void OnGoToLoginClick()
        {
            if (registerPanel != null)
            {
                registerPanel.SetActive(false);
            }

            if (loginPanel != null)
            {
                loginPanel.SetActive(true);
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

            Debug.LogWarning($"[RegisterController] {message}");
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
            if (usernameInput != null)
            {
                usernameInput.text = "";
            }

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
        /// <param name="loading">Whether the register request is in progress.</param>
        private void SetLoading(bool loading)
        {
            _isLoading = loading;

            if (registerButton != null)
            {
                registerButton.interactable = !loading;
            }

            if (usernameInput != null)
            {
                usernameInput.interactable = !loading;
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
            var buttonText = registerButton?.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = loading ? "Inscription..." : "S'inscrire";
            }
        }

        private void OnDestroy()
        {
            if (registerButton != null)
            {
                registerButton.onClick.RemoveListener(OnRegisterClick);
            }

            if (goToLoginButton != null)
            {
                goToLoginButton.onClick.RemoveListener(OnGoToLoginClick);
            }
        }
    }
}
