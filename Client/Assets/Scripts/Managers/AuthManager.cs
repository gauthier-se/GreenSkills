using System;
using System.Text;
using System.Threading.Tasks;
using Data;
using UnityEngine;
using UnityEngine.Networking;

namespace Managers
{
    /// <summary>
    /// Manages user authentication against the Go API.
    /// Handles login, registration, token persistence, and auto-login.
    /// Implements the Singleton pattern and persists across scene loads.
    /// </summary>
    public class AuthManager : MonoBehaviour
    {
        public static AuthManager Instance { get; private set; }

        [Header("API Configuration")]
        [SerializeField] private string apiBaseUrl = "http://localhost:8080";

        private const string TOKEN_KEY = "AuthToken";
        private const string USER_JSON_KEY = "AuthUser";

        private string _token;
        private UserResponseDto _currentUser;

        /// <summary>
        /// The current JWT token, or null if not authenticated.
        /// </summary>
        public string Token => _token;

        /// <summary>
        /// The currently authenticated user, or null if not authenticated.
        /// </summary>
        public UserResponseDto CurrentUser => _currentUser;

        /// <summary>
        /// Whether the user is currently authenticated (has a stored token).
        /// </summary>
        public bool IsAuthenticated => !string.IsNullOrEmpty(_token);

        /// <summary>
        /// Event raised when authentication state changes (login or logout).
        /// </summary>
        public event Action<bool> OnAuthStateChanged;

        #region Singleton Setup

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                if (transform.parent != null)
                {
                    transform.SetParent(null);
                    Debug.LogWarning("[AuthManager] Was not at root level. Moved automatically.");
                }

                DontDestroyOnLoad(gameObject);

                LoadTokenFromPrefs();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Attempts to log in with email and password.
        /// On success, stores the JWT token and user data.
        /// </summary>
        /// <param name="email">User email address.</param>
        /// <param name="password">User password.</param>
        /// <returns>Null on success, or an error message string on failure.</returns>
        public async Task<string> LoginAsync(string email, string password)
        {
            var requestDto = new LoginRequestDto
            {
                email = email,
                password = password
            };

            string json = JsonUtility.ToJson(requestDto);
            string url = $"{apiBaseUrl}/api/auth/login";

            Debug.Log($"[AuthManager] Logging in as {email}...");

            return await SendAuthRequestAsync(url, json);
        }

        /// <summary>
        /// Attempts to register a new account.
        /// On success, stores the JWT token and user data (auto-login after register).
        /// </summary>
        /// <param name="email">User email address.</param>
        /// <param name="username">Display username (min 3 characters).</param>
        /// <param name="password">User password (min 8 characters).</param>
        /// <returns>Null on success, or an error message string on failure.</returns>
        public async Task<string> RegisterAsync(string email, string username, string password)
        {
            var requestDto = new RegisterRequestDto
            {
                email = email,
                username = username,
                password = password
            };

            string json = JsonUtility.ToJson(requestDto);
            string url = $"{apiBaseUrl}/api/auth/register";

            Debug.Log($"[AuthManager] Registering {username} ({email})...");

            return await SendAuthRequestAsync(url, json);
        }

        /// <summary>
        /// Logs out the current user. Clears token and user data from memory and PlayerPrefs.
        /// </summary>
        public void Logout()
        {
            Debug.Log("[AuthManager] Logging out...");

            _token = null;
            _currentUser = null;

            PlayerPrefs.DeleteKey(TOKEN_KEY);
            PlayerPrefs.DeleteKey(USER_JSON_KEY);
            PlayerPrefs.Save();

            OnAuthStateChanged?.Invoke(false);
        }

        /// <summary>
        /// Checks if a saved token exists in PlayerPrefs.
        /// Does not validate token expiry (MVP behavior).
        /// </summary>
        /// <returns>True if a token was loaded from PlayerPrefs.</returns>
        public bool TryAutoLogin()
        {
            return IsAuthenticated;
        }

        #endregion

        #region HTTP Helpers

        /// <summary>
        /// Sends a POST request to an auth endpoint and processes the response.
        /// On success, saves the token and user data.
        /// </summary>
        /// <param name="url">Full API URL.</param>
        /// <param name="jsonBody">JSON request body.</param>
        /// <returns>Null on success, or an error message string on failure.</returns>
        private async Task<string> SendAuthRequestAsync(string url, string jsonBody)
        {
            using (var request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError($"[AuthManager] Network error: {request.error}");
                    return "Erreur de connexion au serveur. Vérifiez votre connexion internet.";
                }

                string responseJson = request.downloadHandler.text;

                if (request.result != UnityWebRequest.Result.Success)
                {
                    // Try to parse the error response from the API
                    try
                    {
                        var errorDto = JsonUtility.FromJson<ErrorResponseDto>(responseJson);
                        if (!string.IsNullOrEmpty(errorDto?.error))
                        {
                            Debug.LogWarning($"[AuthManager] Auth error: {errorDto.error}");
                            return TranslateApiError(errorDto.error);
                        }
                    }
                    catch (System.Exception)
                    {
                        // Ignore parse errors, fall through to generic message
                    }

                    Debug.LogError($"[AuthManager] HTTP {request.responseCode}: {request.error}");
                    return "Une erreur est survenue. Veuillez réessayer.";
                }

                // Success — parse the auth response
                try
                {
                    var authResponse = JsonUtility.FromJson<AuthResponseDto>(responseJson);

                    if (authResponse == null || string.IsNullOrEmpty(authResponse.token))
                    {
                        Debug.LogError("[AuthManager] Invalid auth response: missing token");
                        return "Réponse invalide du serveur.";
                    }

                    _token = authResponse.token;
                    _currentUser = authResponse.user;

                    SaveTokenToPrefs();

                    Debug.Log($"[AuthManager] Authenticated as {_currentUser?.username ?? "unknown"}");
                    OnAuthStateChanged?.Invoke(true);

                    return null; // Success
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[AuthManager] Error parsing auth response: {e.Message}");
                    return "Réponse invalide du serveur.";
                }
            }
        }

        /// <summary>
        /// Translates API error messages to user-friendly French messages.
        /// </summary>
        private string TranslateApiError(string apiError)
        {
            return apiError switch
            {
                "invalid email or password" => "Email ou mot de passe incorrect.",
                "email already registered" => "Cette adresse email est déjà utilisée.",
                "username already taken" => "Ce nom d'utilisateur est déjà pris.",
                "email and password are required" => "L'email et le mot de passe sont requis.",
                "internal server error" => "Erreur interne du serveur. Veuillez réessayer.",
                _ => apiError
            };
        }

        #endregion

        #region Token Persistence

        /// <summary>
        /// Saves the current token and user data to PlayerPrefs.
        /// </summary>
        private void SaveTokenToPrefs()
        {
            PlayerPrefs.SetString(TOKEN_KEY, _token);

            if (_currentUser != null)
            {
                string userJson = JsonUtility.ToJson(_currentUser);
                PlayerPrefs.SetString(USER_JSON_KEY, userJson);
            }

            PlayerPrefs.Save();
            Debug.Log("[AuthManager] Token saved to PlayerPrefs.");
        }

        /// <summary>
        /// Loads the token and user data from PlayerPrefs on startup.
        /// </summary>
        private void LoadTokenFromPrefs()
        {
            _token = PlayerPrefs.GetString(TOKEN_KEY, null);

            if (string.IsNullOrEmpty(_token))
            {
                _token = null;
                Debug.Log("[AuthManager] No saved token found.");
                return;
            }

            string userJson = PlayerPrefs.GetString(USER_JSON_KEY, null);

            if (!string.IsNullOrEmpty(userJson))
            {
                try
                {
                    _currentUser = JsonUtility.FromJson<UserResponseDto>(userJson);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[AuthManager] Error loading saved user: {e.Message}");
                    _currentUser = null;
                }
            }

            Debug.Log($"[AuthManager] Token loaded from PlayerPrefs (user: {_currentUser?.username ?? "unknown"}).");
        }

        #endregion
    }
}
