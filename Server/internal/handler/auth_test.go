package handler

import (
	"bytes"
	"context"
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"testing"
	"time"

	"github.com/jackc/pgx/v5"
	"github.com/jackc/pgx/v5/pgtype"
	"golang.org/x/crypto/bcrypt"

	"github.com/gauthier-se/GreenSkills-API/internal/db"
)

// mockAuthStore implements AuthStore for unit testing.
type mockAuthStore struct {
	users          map[string]db.User // keyed by email
	emailExists    map[string]bool
	usernameExists map[string]bool
	createErr      error
}

func newMockAuthStore() *mockAuthStore {
	return &mockAuthStore{
		users:          make(map[string]db.User),
		emailExists:    make(map[string]bool),
		usernameExists: make(map[string]bool),
	}
}

func (m *mockAuthStore) CreateUser(_ context.Context, arg db.CreateUserParams) (db.User, error) {
	if m.createErr != nil {
		return db.User{}, m.createErr
	}
	user := db.User{
		ID:           pgtype.UUID{Bytes: [16]byte{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16}, Valid: true},
		Email:        arg.Email,
		Username:     arg.Username,
		PasswordHash: arg.PasswordHash,
		CreatedAt:    pgtype.Timestamptz{Time: time.Now(), Valid: true},
		UpdatedAt:    pgtype.Timestamptz{Time: time.Now(), Valid: true},
	}
	m.users[arg.Email] = user
	m.emailExists[arg.Email] = true
	m.usernameExists[arg.Username] = true
	return user, nil
}

func (m *mockAuthStore) GetUserByEmail(_ context.Context, email string) (db.User, error) {
	user, ok := m.users[email]
	if !ok {
		return db.User{}, pgx.ErrNoRows
	}
	return user, nil
}

func (m *mockAuthStore) UserExistsByEmail(_ context.Context, email string) (bool, error) {
	return m.emailExists[email], nil
}

func (m *mockAuthStore) UserExistsByUsername(_ context.Context, username string) (bool, error) {
	return m.usernameExists[username], nil
}

// addUser is a helper to seed a user directly into the mock store with a hashed password.
func (m *mockAuthStore) addUser(email, username, password string) {
	hash, _ := bcrypt.GenerateFromPassword([]byte(password), bcrypt.MinCost)
	user := db.User{
		ID:           pgtype.UUID{Bytes: [16]byte{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16}, Valid: true},
		Email:        email,
		Username:     username,
		PasswordHash: string(hash),
		CreatedAt:    pgtype.Timestamptz{Time: time.Now(), Valid: true},
		UpdatedAt:    pgtype.Timestamptz{Time: time.Now(), Valid: true},
	}
	m.users[email] = user
	m.emailExists[email] = true
	m.usernameExists[username] = true
}

const testJWTSecret = "test-secret-key-for-testing-only"
const testJWTExpiry = 24 * time.Hour

func newTestAuthHandler(store *mockAuthStore) *AuthHandler {
	return NewAuthHandler(store, testJWTSecret, testJWTExpiry)
}

// --- Register Tests ---

func TestRegister_Success(t *testing.T) {
	store := newMockAuthStore()
	h := newTestAuthHandler(store)

	body := RegisterRequest{
		Email:    "test@example.com",
		Username: "testuser",
		Password: "password123",
	}
	jsonBody, _ := json.Marshal(body)

	req := httptest.NewRequest(http.MethodPost, "/api/auth/register", bytes.NewReader(jsonBody))
	rec := httptest.NewRecorder()

	h.Register().ServeHTTP(rec, req)

	if rec.Code != http.StatusCreated {
		t.Errorf("expected status %d, got %d", http.StatusCreated, rec.Code)
	}

	var resp AuthResponse
	if err := json.NewDecoder(rec.Body).Decode(&resp); err != nil {
		t.Fatalf("failed to decode response: %v", err)
	}

	if resp.Token == "" {
		t.Error("expected non-empty token")
	}

	if resp.User.Email != "test@example.com" {
		t.Errorf("expected email 'test@example.com', got %q", resp.User.Email)
	}

	if resp.User.Username != "testuser" {
		t.Errorf("expected username 'testuser', got %q", resp.User.Username)
	}

	if resp.User.ID == "" {
		t.Error("expected non-empty user ID")
	}
}

func TestRegister_MissingEmail(t *testing.T) {
	store := newMockAuthStore()
	h := newTestAuthHandler(store)

	body := RegisterRequest{
		Username: "testuser",
		Password: "password123",
	}
	jsonBody, _ := json.Marshal(body)

	req := httptest.NewRequest(http.MethodPost, "/api/auth/register", bytes.NewReader(jsonBody))
	rec := httptest.NewRecorder()

	h.Register().ServeHTTP(rec, req)

	if rec.Code != http.StatusBadRequest {
		t.Errorf("expected status %d, got %d", http.StatusBadRequest, rec.Code)
	}

	var resp ErrorResponse
	json.NewDecoder(rec.Body).Decode(&resp)
	if resp.Error != "email is required" {
		t.Errorf("expected error 'email is required', got %q", resp.Error)
	}
}

func TestRegister_MissingUsername(t *testing.T) {
	store := newMockAuthStore()
	h := newTestAuthHandler(store)

	body := RegisterRequest{
		Email:    "test@example.com",
		Password: "password123",
	}
	jsonBody, _ := json.Marshal(body)

	req := httptest.NewRequest(http.MethodPost, "/api/auth/register", bytes.NewReader(jsonBody))
	rec := httptest.NewRecorder()

	h.Register().ServeHTTP(rec, req)

	if rec.Code != http.StatusBadRequest {
		t.Errorf("expected status %d, got %d", http.StatusBadRequest, rec.Code)
	}
}

func TestRegister_ShortPassword(t *testing.T) {
	store := newMockAuthStore()
	h := newTestAuthHandler(store)

	body := RegisterRequest{
		Email:    "test@example.com",
		Username: "testuser",
		Password: "short",
	}
	jsonBody, _ := json.Marshal(body)

	req := httptest.NewRequest(http.MethodPost, "/api/auth/register", bytes.NewReader(jsonBody))
	rec := httptest.NewRecorder()

	h.Register().ServeHTTP(rec, req)

	if rec.Code != http.StatusBadRequest {
		t.Errorf("expected status %d, got %d", http.StatusBadRequest, rec.Code)
	}

	var resp ErrorResponse
	json.NewDecoder(rec.Body).Decode(&resp)
	if resp.Error != "password must be at least 8 characters" {
		t.Errorf("expected password length error, got %q", resp.Error)
	}
}

func TestRegister_InvalidEmail(t *testing.T) {
	store := newMockAuthStore()
	h := newTestAuthHandler(store)

	body := RegisterRequest{
		Email:    "not-an-email",
		Username: "testuser",
		Password: "password123",
	}
	jsonBody, _ := json.Marshal(body)

	req := httptest.NewRequest(http.MethodPost, "/api/auth/register", bytes.NewReader(jsonBody))
	rec := httptest.NewRecorder()

	h.Register().ServeHTTP(rec, req)

	if rec.Code != http.StatusBadRequest {
		t.Errorf("expected status %d, got %d", http.StatusBadRequest, rec.Code)
	}
}

func TestRegister_DuplicateEmail(t *testing.T) {
	store := newMockAuthStore()
	store.addUser("existing@example.com", "existing", "password123")
	h := newTestAuthHandler(store)

	body := RegisterRequest{
		Email:    "existing@example.com",
		Username: "newuser",
		Password: "password123",
	}
	jsonBody, _ := json.Marshal(body)

	req := httptest.NewRequest(http.MethodPost, "/api/auth/register", bytes.NewReader(jsonBody))
	rec := httptest.NewRecorder()

	h.Register().ServeHTTP(rec, req)

	if rec.Code != http.StatusConflict {
		t.Errorf("expected status %d, got %d", http.StatusConflict, rec.Code)
	}

	var resp ErrorResponse
	json.NewDecoder(rec.Body).Decode(&resp)
	if resp.Error != "email already registered" {
		t.Errorf("expected 'email already registered', got %q", resp.Error)
	}
}

func TestRegister_DuplicateUsername(t *testing.T) {
	store := newMockAuthStore()
	store.addUser("existing@example.com", "existinguser", "password123")
	h := newTestAuthHandler(store)

	body := RegisterRequest{
		Email:    "new@example.com",
		Username: "existinguser",
		Password: "password123",
	}
	jsonBody, _ := json.Marshal(body)

	req := httptest.NewRequest(http.MethodPost, "/api/auth/register", bytes.NewReader(jsonBody))
	rec := httptest.NewRecorder()

	h.Register().ServeHTTP(rec, req)

	if rec.Code != http.StatusConflict {
		t.Errorf("expected status %d, got %d", http.StatusConflict, rec.Code)
	}

	var resp ErrorResponse
	json.NewDecoder(rec.Body).Decode(&resp)
	if resp.Error != "username already taken" {
		t.Errorf("expected 'username already taken', got %q", resp.Error)
	}
}

func TestRegister_InvalidJSON(t *testing.T) {
	store := newMockAuthStore()
	h := newTestAuthHandler(store)

	req := httptest.NewRequest(http.MethodPost, "/api/auth/register", bytes.NewReader([]byte("not json")))
	rec := httptest.NewRecorder()

	h.Register().ServeHTTP(rec, req)

	if rec.Code != http.StatusBadRequest {
		t.Errorf("expected status %d, got %d", http.StatusBadRequest, rec.Code)
	}
}

// --- Login Tests ---

func TestLogin_Success(t *testing.T) {
	store := newMockAuthStore()
	store.addUser("test@example.com", "testuser", "password123")
	h := newTestAuthHandler(store)

	body := LoginRequest{
		Email:    "test@example.com",
		Password: "password123",
	}
	jsonBody, _ := json.Marshal(body)

	req := httptest.NewRequest(http.MethodPost, "/api/auth/login", bytes.NewReader(jsonBody))
	rec := httptest.NewRecorder()

	h.Login().ServeHTTP(rec, req)

	if rec.Code != http.StatusOK {
		t.Errorf("expected status %d, got %d", http.StatusOK, rec.Code)
	}

	var resp AuthResponse
	if err := json.NewDecoder(rec.Body).Decode(&resp); err != nil {
		t.Fatalf("failed to decode response: %v", err)
	}

	if resp.Token == "" {
		t.Error("expected non-empty token")
	}

	if resp.User.Email != "test@example.com" {
		t.Errorf("expected email 'test@example.com', got %q", resp.User.Email)
	}
}

func TestLogin_WrongPassword(t *testing.T) {
	store := newMockAuthStore()
	store.addUser("test@example.com", "testuser", "password123")
	h := newTestAuthHandler(store)

	body := LoginRequest{
		Email:    "test@example.com",
		Password: "wrongpassword",
	}
	jsonBody, _ := json.Marshal(body)

	req := httptest.NewRequest(http.MethodPost, "/api/auth/login", bytes.NewReader(jsonBody))
	rec := httptest.NewRecorder()

	h.Login().ServeHTTP(rec, req)

	if rec.Code != http.StatusUnauthorized {
		t.Errorf("expected status %d, got %d", http.StatusUnauthorized, rec.Code)
	}

	var resp ErrorResponse
	json.NewDecoder(rec.Body).Decode(&resp)
	if resp.Error != "invalid email or password" {
		t.Errorf("expected 'invalid email or password', got %q", resp.Error)
	}
}

func TestLogin_NonexistentUser(t *testing.T) {
	store := newMockAuthStore()
	h := newTestAuthHandler(store)

	body := LoginRequest{
		Email:    "nobody@example.com",
		Password: "password123",
	}
	jsonBody, _ := json.Marshal(body)

	req := httptest.NewRequest(http.MethodPost, "/api/auth/login", bytes.NewReader(jsonBody))
	rec := httptest.NewRecorder()

	h.Login().ServeHTTP(rec, req)

	if rec.Code != http.StatusUnauthorized {
		t.Errorf("expected status %d, got %d", http.StatusUnauthorized, rec.Code)
	}
}

func TestLogin_MissingFields(t *testing.T) {
	store := newMockAuthStore()
	h := newTestAuthHandler(store)

	body := LoginRequest{
		Email: "test@example.com",
	}
	jsonBody, _ := json.Marshal(body)

	req := httptest.NewRequest(http.MethodPost, "/api/auth/login", bytes.NewReader(jsonBody))
	rec := httptest.NewRecorder()

	h.Login().ServeHTTP(rec, req)

	if rec.Code != http.StatusBadRequest {
		t.Errorf("expected status %d, got %d", http.StatusBadRequest, rec.Code)
	}
}

func TestLogin_InvalidJSON(t *testing.T) {
	store := newMockAuthStore()
	h := newTestAuthHandler(store)

	req := httptest.NewRequest(http.MethodPost, "/api/auth/login", bytes.NewReader([]byte("{bad")))
	rec := httptest.NewRecorder()

	h.Login().ServeHTTP(rec, req)

	if rec.Code != http.StatusBadRequest {
		t.Errorf("expected status %d, got %d", http.StatusBadRequest, rec.Code)
	}
}

// --- Validation Tests ---

func TestValidateRegisterRequest(t *testing.T) {
	tests := []struct {
		name    string
		req     RegisterRequest
		wantErr string
	}{
		{
			name:    "valid request",
			req:     RegisterRequest{Email: "user@example.com", Username: "testuser", Password: "password123"},
			wantErr: "",
		},
		{
			name:    "empty email",
			req:     RegisterRequest{Username: "testuser", Password: "password123"},
			wantErr: "email is required",
		},
		{
			name:    "empty username",
			req:     RegisterRequest{Email: "user@example.com", Password: "password123"},
			wantErr: "username is required",
		},
		{
			name:    "empty password",
			req:     RegisterRequest{Email: "user@example.com", Username: "testuser"},
			wantErr: "password is required",
		},
		{
			name:    "short password",
			req:     RegisterRequest{Email: "user@example.com", Username: "testuser", Password: "short"},
			wantErr: "password must be at least 8 characters",
		},
		{
			name:    "invalid email",
			req:     RegisterRequest{Email: "not-an-email", Username: "testuser", Password: "password123"},
			wantErr: "invalid email format",
		},
		{
			name:    "short username",
			req:     RegisterRequest{Email: "user@example.com", Username: "ab", Password: "password123"},
			wantErr: "username must be at least 3 characters",
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			got := validateRegisterRequest(tt.req)
			if got != tt.wantErr {
				t.Errorf("validateRegisterRequest() = %q, want %q", got, tt.wantErr)
			}
		})
	}
}

// --- UUID Helper Test ---

func TestUuidToString(t *testing.T) {
	id := pgtype.UUID{
		Bytes: [16]byte{0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10},
		Valid: true,
	}

	got := uuidToString(id)
	want := "01020304-0506-0708-090a-0b0c0d0e0f10"

	if got != want {
		t.Errorf("uuidToString() = %q, want %q", got, want)
	}
}

func TestUuidToString_Invalid(t *testing.T) {
	id := pgtype.UUID{Valid: false}
	got := uuidToString(id)
	if got != "" {
		t.Errorf("expected empty string for invalid UUID, got %q", got)
	}
}
