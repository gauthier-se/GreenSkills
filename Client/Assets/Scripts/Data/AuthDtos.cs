namespace Data
{
    /// <summary>
    /// Request body for POST /api/auth/login.
    /// </summary>
    [System.Serializable]
    public class LoginRequestDto
    {
        public string email;
        public string password;
    }

    /// <summary>
    /// Request body for POST /api/auth/register.
    /// </summary>
    [System.Serializable]
    public class RegisterRequestDto
    {
        public string email;
        public string username;
        public string password;
    }

    /// <summary>
    /// Response body from POST /api/auth/login and POST /api/auth/register.
    /// Contains the JWT token and user information.
    /// </summary>
    [System.Serializable]
    public class AuthResponseDto
    {
        public string token;
        public UserResponseDto user;
    }

    /// <summary>
    /// User information returned from auth endpoints.
    /// </summary>
    [System.Serializable]
    public class UserResponseDto
    {
        public string id;
        public string email;
        public string username;
        public string createdAt;
    }

    /// <summary>
    /// Error response from the API.
    /// </summary>
    [System.Serializable]
    public class ErrorResponseDto
    {
        public string error;
    }
}
