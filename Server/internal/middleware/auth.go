package middleware

import (
	"context"
	"encoding/json"
	"net/http"
	"strings"

	"github.com/golang-jwt/jwt/v5"
)

// contextKey is an unexported type for context keys in this package.
type contextKey string

// UserIDKey is the context key for the authenticated user's ID.
const UserIDKey contextKey = "userID"

// Auth returns a middleware that validates JWT tokens from the Authorization header.
// If valid, the user's ID (from the "sub" claim) is stored in the request context.
// If invalid or missing, it responds with 401 Unauthorized.
func Auth(jwtSecret string) func(http.Handler) http.Handler {
	secret := []byte(jwtSecret)

	return func(next http.Handler) http.Handler {
		return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
			authHeader := r.Header.Get("Authorization")
			if authHeader == "" {
				writeAuthError(w, "missing authorization header")
				return
			}

			// Expect "Bearer <token>"
			parts := strings.SplitN(authHeader, " ", 2)
			if len(parts) != 2 || !strings.EqualFold(parts[0], "bearer") {
				writeAuthError(w, "invalid authorization header format")
				return
			}

			tokenString := parts[1]

			token, err := jwt.Parse(tokenString, func(t *jwt.Token) (any, error) {
				// Ensure the signing method is HMAC
				if _, ok := t.Method.(*jwt.SigningMethodHMAC); !ok {
					return nil, jwt.ErrSignatureInvalid
				}
				return secret, nil
			})
			if err != nil || !token.Valid {
				writeAuthError(w, "invalid or expired token")
				return
			}

			// Extract the subject (user ID) from claims
			subject, err := token.Claims.GetSubject()
			if err != nil || subject == "" {
				writeAuthError(w, "invalid token claims")
				return
			}

			// Store the user ID in the request context
			ctx := context.WithValue(r.Context(), UserIDKey, subject)
			next.ServeHTTP(w, r.WithContext(ctx))
		})
	}
}

// GetUserID extracts the authenticated user's ID from the request context.
// Returns an empty string if not found (should never happen on protected routes).
func GetUserID(ctx context.Context) string {
	if id, ok := ctx.Value(UserIDKey).(string); ok {
		return id
	}
	return ""
}

// writeAuthError writes a 401 JSON error response.
func writeAuthError(w http.ResponseWriter, message string) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusUnauthorized)
	json.NewEncoder(w).Encode(map[string]string{"error": message})
}
