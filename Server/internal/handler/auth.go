package handler

import (
	"context"
	"encoding/json"
	"log/slog"
	"net/http"
	"strings"
	"time"

	"github.com/golang-jwt/jwt/v5"
	"github.com/jackc/pgx/v5"
	"github.com/jackc/pgx/v5/pgtype"
	"golang.org/x/crypto/bcrypt"

	"github.com/gauthier-se/GreenSkills-API/internal/db"
)

// AuthStore defines the database operations required by auth handlers.
// This interface enables unit testing with mock implementations.
type AuthStore interface {
	CreateUser(ctx context.Context, arg db.CreateUserParams) (db.User, error)
	GetUserByEmail(ctx context.Context, email string) (db.User, error)
	UserExistsByEmail(ctx context.Context, email string) (bool, error)
	UserExistsByUsername(ctx context.Context, username string) (bool, error)
}

// AuthHandler holds the dependencies for authentication endpoints.
type AuthHandler struct {
	store     AuthStore
	jwtSecret []byte
	jwtExpiry time.Duration
}

// NewAuthHandler creates a new AuthHandler with the provided dependencies.
func NewAuthHandler(store AuthStore, jwtSecret string, jwtExpiry time.Duration) *AuthHandler {
	return &AuthHandler{
		store:     store,
		jwtSecret: []byte(jwtSecret),
		jwtExpiry: jwtExpiry,
	}
}

// RegisterRequest represents the request body for user registration.
type RegisterRequest struct {
	Email    string `json:"email"`
	Username string `json:"username"`
	Password string `json:"password"`
}

// LoginRequest represents the request body for user login.
type LoginRequest struct {
	Email    string `json:"email"`
	Password string `json:"password"`
}

// AuthResponse represents the response body for successful authentication.
type AuthResponse struct {
	Token string       `json:"token"`
	User  UserResponse `json:"user"`
}

// UserResponse represents user data returned in auth responses.
// Sensitive fields (password_hash) are never included.
type UserResponse struct {
	ID        string `json:"id"`
	Email     string `json:"email"`
	Username  string `json:"username"`
	CreatedAt string `json:"createdAt"`
}

// Register handles POST /api/auth/register.
// It creates a new user account with a bcrypt-hashed password and returns a JWT.
func (h *AuthHandler) Register() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		var req RegisterRequest
		if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
			writeError(w, http.StatusBadRequest, "invalid request body")
			return
		}

		// Validate required fields
		if err := validateRegisterRequest(req); err != "" {
			writeError(w, http.StatusBadRequest, err)
			return
		}

		ctx := r.Context()

		// Check if email is already taken
		emailExists, err := h.store.UserExistsByEmail(ctx, req.Email)
		if err != nil {
			slog.Error("[AuthHandler] Failed to check email existence", "error", err)
			writeError(w, http.StatusInternalServerError, "internal server error")
			return
		}
		if emailExists {
			writeError(w, http.StatusConflict, "email already registered")
			return
		}

		// Check if username is already taken
		usernameExists, err := h.store.UserExistsByUsername(ctx, req.Username)
		if err != nil {
			slog.Error("[AuthHandler] Failed to check username existence", "error", err)
			writeError(w, http.StatusInternalServerError, "internal server error")
			return
		}
		if usernameExists {
			writeError(w, http.StatusConflict, "username already taken")
			return
		}

		// Hash the password
		hashedPassword, err := bcrypt.GenerateFromPassword([]byte(req.Password), bcrypt.DefaultCost)
		if err != nil {
			slog.Error("[AuthHandler] Failed to hash password", "error", err)
			writeError(w, http.StatusInternalServerError, "internal server error")
			return
		}

		// Create the user
		user, err := h.store.CreateUser(ctx, db.CreateUserParams{
			Email:        req.Email,
			Username:     req.Username,
			PasswordHash: string(hashedPassword),
		})
		if err != nil {
			slog.Error("[AuthHandler] Failed to create user", "error", err)
			writeError(w, http.StatusInternalServerError, "internal server error")
			return
		}

		// Generate JWT
		token, err := h.generateToken(user.ID)
		if err != nil {
			slog.Error("[AuthHandler] Failed to generate token", "error", err)
			writeError(w, http.StatusInternalServerError, "internal server error")
			return
		}

		writeJSON(w, http.StatusCreated, AuthResponse{
			Token: token,
			User:  toUserResponse(user),
		})
	}
}

// Login handles POST /api/auth/login.
// It authenticates a user by email/password and returns a JWT.
func (h *AuthHandler) Login() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		var req LoginRequest
		if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
			writeError(w, http.StatusBadRequest, "invalid request body")
			return
		}

		// Validate required fields
		if req.Email == "" || req.Password == "" {
			writeError(w, http.StatusBadRequest, "email and password are required")
			return
		}

		ctx := r.Context()

		// Look up the user by email
		user, err := h.store.GetUserByEmail(ctx, req.Email)
		if err != nil {
			if err == pgx.ErrNoRows {
				writeError(w, http.StatusUnauthorized, "invalid email or password")
				return
			}
			slog.Error("[AuthHandler] Failed to look up user", "error", err)
			writeError(w, http.StatusInternalServerError, "internal server error")
			return
		}

		// Compare passwords
		if err := bcrypt.CompareHashAndPassword([]byte(user.PasswordHash), []byte(req.Password)); err != nil {
			writeError(w, http.StatusUnauthorized, "invalid email or password")
			return
		}

		// Generate JWT
		token, err := h.generateToken(user.ID)
		if err != nil {
			slog.Error("[AuthHandler] Failed to generate token", "error", err)
			writeError(w, http.StatusInternalServerError, "internal server error")
			return
		}

		writeJSON(w, http.StatusOK, AuthResponse{
			Token: token,
			User:  toUserResponse(user),
		})
	}
}

// generateToken creates a signed JWT with the user's ID as subject.
func (h *AuthHandler) generateToken(userID pgtype.UUID) (string, error) {
	now := time.Now()
	claims := jwt.RegisteredClaims{
		Subject:   uuidToString(userID),
		IssuedAt:  jwt.NewNumericDate(now),
		ExpiresAt: jwt.NewNumericDate(now.Add(h.jwtExpiry)),
		Issuer:    "greenskills-api",
	}

	token := jwt.NewWithClaims(jwt.SigningMethodHS256, claims)
	return token.SignedString(h.jwtSecret)
}

// toUserResponse converts a db.User to a UserResponse, excluding sensitive fields.
func toUserResponse(u db.User) UserResponse {
	return UserResponse{
		ID:        uuidToString(u.ID),
		Email:     u.Email,
		Username:  u.Username,
		CreatedAt: u.CreatedAt.Time.Format(time.RFC3339),
	}
}

// uuidToString converts a pgtype.UUID to its string representation.
func uuidToString(u pgtype.UUID) string {
	if !u.Valid {
		return ""
	}
	b := u.Bytes
	return strings.Join([]string{
		hex(b[0:4]),
		hex(b[4:6]),
		hex(b[6:8]),
		hex(b[8:10]),
		hex(b[10:16]),
	}, "-")
}

func hex(b []byte) string {
	const hextable = "0123456789abcdef"
	s := make([]byte, len(b)*2)
	for i, v := range b {
		s[i*2] = hextable[v>>4]
		s[i*2+1] = hextable[v&0x0f]
	}
	return string(s)
}

// validateRegisterRequest returns an error message if the request is invalid,
// or an empty string if valid.
func validateRegisterRequest(req RegisterRequest) string {
	if req.Email == "" {
		return "email is required"
	}
	if req.Username == "" {
		return "username is required"
	}
	if req.Password == "" {
		return "password is required"
	}
	if len(req.Password) < 8 {
		return "password must be at least 8 characters"
	}
	if !strings.Contains(req.Email, "@") {
		return "invalid email format"
	}
	if len(req.Username) < 3 {
		return "username must be at least 3 characters"
	}
	return ""
}
