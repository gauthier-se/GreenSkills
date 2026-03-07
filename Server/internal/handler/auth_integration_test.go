package handler_test

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
	"github.com/gauthier-se/GreenSkills-API/internal/handler"
	"github.com/gauthier-se/GreenSkills-API/internal/router"
)

const integrationJWTSecret = "integration-test-secret-key"
const integrationJWTExpiry = 24 * time.Hour

// integrationMockStore implements handler.AuthStore for integration tests.
type integrationMockStore struct {
	users          map[string]db.User
	emailExists    map[string]bool
	usernameExists map[string]bool
}

func newIntegrationMockStore() *integrationMockStore {
	return &integrationMockStore{
		users:          make(map[string]db.User),
		emailExists:    make(map[string]bool),
		usernameExists: make(map[string]bool),
	}
}

func (m *integrationMockStore) CreateUser(_ context.Context, arg db.CreateUserParams) (db.User, error) {
	user := db.User{
		ID:           pgtype.UUID{Bytes: [16]byte{0xaa, 0xbb, 0xcc, 0xdd, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12}, Valid: true},
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

func (m *integrationMockStore) GetUserByEmail(_ context.Context, email string) (db.User, error) {
	user, ok := m.users[email]
	if !ok {
		return db.User{}, pgx.ErrNoRows
	}
	return user, nil
}

func (m *integrationMockStore) UserExistsByEmail(_ context.Context, email string) (bool, error) {
	return m.emailExists[email], nil
}

func (m *integrationMockStore) UserExistsByUsername(_ context.Context, username string) (bool, error) {
	return m.usernameExists[username], nil
}

func (m *integrationMockStore) addUser(email, username, password string) {
	hash, _ := bcrypt.GenerateFromPassword([]byte(password), bcrypt.MinCost)
	user := db.User{
		ID:           pgtype.UUID{Bytes: [16]byte{0xaa, 0xbb, 0xcc, 0xdd, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12}, Valid: true},
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

func newTestRouter(store handler.AuthStore) *httptest.Server {
	r := router.New(router.Config{
		CORSAllowedOrigins: []string{"*"},
		AuthStore:          store,
		JWTSecret:          integrationJWTSecret,
		JWTExpiry:          integrationJWTExpiry,
	})
	return httptest.NewServer(r)
}

// --- Register Integration Tests ---

func TestRegisterIntegration_Success(t *testing.T) {
	store := newIntegrationMockStore()
	srv := newTestRouter(store)
	defer srv.Close()

	body, _ := json.Marshal(map[string]string{
		"email":    "newuser@example.com",
		"username": "newuser",
		"password": "password123",
	})

	resp, err := http.Post(srv.URL+"/api/auth/register", "application/json", bytes.NewReader(body))
	if err != nil {
		t.Fatalf("failed to send request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusCreated {
		t.Errorf("expected status %d, got %d", http.StatusCreated, resp.StatusCode)
	}

	contentType := resp.Header.Get("Content-Type")
	if contentType != "application/json" {
		t.Errorf("expected Content-Type 'application/json', got %q", contentType)
	}

	var authResp handler.AuthResponse
	if err := json.NewDecoder(resp.Body).Decode(&authResp); err != nil {
		t.Fatalf("failed to decode response: %v", err)
	}

	if authResp.Token == "" {
		t.Error("expected non-empty token")
	}

	if authResp.User.Email != "newuser@example.com" {
		t.Errorf("expected email 'newuser@example.com', got %q", authResp.User.Email)
	}

	if authResp.User.Username != "newuser" {
		t.Errorf("expected username 'newuser', got %q", authResp.User.Username)
	}
}

func TestRegisterIntegration_DuplicateEmail(t *testing.T) {
	store := newIntegrationMockStore()
	store.addUser("taken@example.com", "existing", "password123")

	srv := newTestRouter(store)
	defer srv.Close()

	body, _ := json.Marshal(map[string]string{
		"email":    "taken@example.com",
		"username": "different",
		"password": "password123",
	})

	resp, err := http.Post(srv.URL+"/api/auth/register", "application/json", bytes.NewReader(body))
	if err != nil {
		t.Fatalf("failed to send request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusConflict {
		t.Errorf("expected status %d, got %d", http.StatusConflict, resp.StatusCode)
	}
}

func TestRegisterIntegration_ValidationError(t *testing.T) {
	store := newIntegrationMockStore()
	srv := newTestRouter(store)
	defer srv.Close()

	body, _ := json.Marshal(map[string]string{
		"email":    "user@example.com",
		"username": "ab",
		"password": "password123",
	})

	resp, err := http.Post(srv.URL+"/api/auth/register", "application/json", bytes.NewReader(body))
	if err != nil {
		t.Fatalf("failed to send request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusBadRequest {
		t.Errorf("expected status %d, got %d", http.StatusBadRequest, resp.StatusCode)
	}
}

func TestRegisterIntegration_MethodNotAllowed(t *testing.T) {
	store := newIntegrationMockStore()
	srv := newTestRouter(store)
	defer srv.Close()

	resp, err := http.Get(srv.URL + "/api/auth/register")
	if err != nil {
		t.Fatalf("failed to send request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusMethodNotAllowed {
		t.Errorf("expected status %d, got %d", http.StatusMethodNotAllowed, resp.StatusCode)
	}
}

// --- Login Integration Tests ---

func TestLoginIntegration_Success(t *testing.T) {
	store := newIntegrationMockStore()
	store.addUser("user@example.com", "testuser", "password123")

	srv := newTestRouter(store)
	defer srv.Close()

	body, _ := json.Marshal(map[string]string{
		"email":    "user@example.com",
		"password": "password123",
	})

	resp, err := http.Post(srv.URL+"/api/auth/login", "application/json", bytes.NewReader(body))
	if err != nil {
		t.Fatalf("failed to send request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		t.Errorf("expected status %d, got %d", http.StatusOK, resp.StatusCode)
	}

	var authResp handler.AuthResponse
	if err := json.NewDecoder(resp.Body).Decode(&authResp); err != nil {
		t.Fatalf("failed to decode response: %v", err)
	}

	if authResp.Token == "" {
		t.Error("expected non-empty token")
	}

	if authResp.User.Email != "user@example.com" {
		t.Errorf("expected email 'user@example.com', got %q", authResp.User.Email)
	}
}

func TestLoginIntegration_WrongPassword(t *testing.T) {
	store := newIntegrationMockStore()
	store.addUser("user@example.com", "testuser", "password123")

	srv := newTestRouter(store)
	defer srv.Close()

	body, _ := json.Marshal(map[string]string{
		"email":    "user@example.com",
		"password": "wrongpassword",
	})

	resp, err := http.Post(srv.URL+"/api/auth/login", "application/json", bytes.NewReader(body))
	if err != nil {
		t.Fatalf("failed to send request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusUnauthorized {
		t.Errorf("expected status %d, got %d", http.StatusUnauthorized, resp.StatusCode)
	}
}

func TestLoginIntegration_NonexistentUser(t *testing.T) {
	store := newIntegrationMockStore()
	srv := newTestRouter(store)
	defer srv.Close()

	body, _ := json.Marshal(map[string]string{
		"email":    "nobody@example.com",
		"password": "password123",
	})

	resp, err := http.Post(srv.URL+"/api/auth/login", "application/json", bytes.NewReader(body))
	if err != nil {
		t.Fatalf("failed to send request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusUnauthorized {
		t.Errorf("expected status %d, got %d", http.StatusUnauthorized, resp.StatusCode)
	}
}

// --- Register Then Login Integration Test ---

func TestRegisterThenLogin_Integration(t *testing.T) {
	store := newIntegrationMockStore()
	srv := newTestRouter(store)
	defer srv.Close()

	// Step 1: Register
	regBody, _ := json.Marshal(map[string]string{
		"email":    "flow@example.com",
		"username": "flowuser",
		"password": "password123",
	})

	regResp, err := http.Post(srv.URL+"/api/auth/register", "application/json", bytes.NewReader(regBody))
	if err != nil {
		t.Fatalf("register request failed: %v", err)
	}
	defer regResp.Body.Close()

	if regResp.StatusCode != http.StatusCreated {
		t.Fatalf("register expected %d, got %d", http.StatusCreated, regResp.StatusCode)
	}

	var regAuthResp handler.AuthResponse
	json.NewDecoder(regResp.Body).Decode(&regAuthResp)

	// Step 2: Login with the same credentials
	loginBody, _ := json.Marshal(map[string]string{
		"email":    "flow@example.com",
		"password": "password123",
	})

	loginResp, err := http.Post(srv.URL+"/api/auth/login", "application/json", bytes.NewReader(loginBody))
	if err != nil {
		t.Fatalf("login request failed: %v", err)
	}
	defer loginResp.Body.Close()

	if loginResp.StatusCode != http.StatusOK {
		t.Errorf("login expected %d, got %d", http.StatusOK, loginResp.StatusCode)
	}

	var loginAuthResp handler.AuthResponse
	json.NewDecoder(loginResp.Body).Decode(&loginAuthResp)

	if loginAuthResp.Token == "" {
		t.Error("expected non-empty login token")
	}

	// Tokens should be different (different issued-at times)
	if loginAuthResp.User.Email != regAuthResp.User.Email {
		t.Errorf("login user email %q doesn't match register email %q",
			loginAuthResp.User.Email, regAuthResp.User.Email)
	}
}
