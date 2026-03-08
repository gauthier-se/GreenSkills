package handler_test

import (
	"bytes"
	"context"
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"testing"
	"time"

	"github.com/golang-jwt/jwt/v5"
	"github.com/jackc/pgx/v5/pgtype"

	"github.com/gauthier-se/GreenSkills-API/internal/db"
	"github.com/gauthier-se/GreenSkills-API/internal/handler"
	"github.com/gauthier-se/GreenSkills-API/internal/router"
)

const progressJWTSecret = "progress-integration-test-secret"

// progressTestUserUUID is a fixed UUID for the test user.
var progressTestUserUUID = pgtype.UUID{
	Bytes: [16]byte{0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10},
	Valid: true,
}

const progressTestUserID = "01020304-0506-0708-090a-0b0c0d0e0f10"

// integrationProgressStore implements handler.ProgressStore for integration tests.
type integrationProgressStore struct {
	progress []db.UserProgress
	upserted db.UserProgress
	stats    db.GetUserStatsRow
}

func newIntegrationProgressStore() *integrationProgressStore {
	return &integrationProgressStore{}
}

func (m *integrationProgressStore) GetUserProgress(_ context.Context, _ pgtype.UUID) ([]db.UserProgress, error) {
	return m.progress, nil
}

func (m *integrationProgressStore) UpsertUserProgress(_ context.Context, arg db.UpsertUserProgressParams) (db.UserProgress, error) {
	// Return the pre-configured upserted result
	return m.upserted, nil
}

func (m *integrationProgressStore) GetUserStats(_ context.Context, _ pgtype.UUID) (db.GetUserStatsRow, error) {
	return m.stats, nil
}

// generateTestToken creates a valid JWT for integration testing.
func generateTestToken(userID string) string {
	claims := jwt.RegisteredClaims{
		Subject:   userID,
		IssuedAt:  jwt.NewNumericDate(time.Now()),
		ExpiresAt: jwt.NewNumericDate(time.Now().Add(24 * time.Hour)),
		Issuer:    "greenskills-api",
	}
	token := jwt.NewWithClaims(jwt.SigningMethodHS256, claims)
	signed, _ := token.SignedString([]byte(progressJWTSecret))
	return signed
}

func newProgressTestRouter(store handler.ProgressStore) *httptest.Server {
	r := router.New(router.Config{
		CORSAllowedOrigins: []string{"*"},
		ProgressStore:      store,
		JWTSecret:          progressJWTSecret,
		JWTExpiry:          24 * time.Hour,
	})
	return httptest.NewServer(r)
}

// --- GetProgress Integration Tests ---

func TestGetProgressIntegration_Success(t *testing.T) {
	store := newIntegrationProgressStore()
	store.progress = []db.UserProgress{
		{
			LevelID:         1,
			Stars:           3,
			BestTimeSeconds: pgtype.Float8{Float64: 42.5, Valid: true},
			Unlocked:        true,
			CompletedAt:     pgtype.Timestamptz{Time: time.Date(2025, 1, 15, 10, 0, 0, 0, time.UTC), Valid: true},
		},
		{
			LevelID:  2,
			Stars:    0,
			Unlocked: true,
		},
	}

	srv := newProgressTestRouter(store)
	defer srv.Close()

	req, _ := http.NewRequest(http.MethodGet, srv.URL+"/api/users/me/progress", nil)
	req.Header.Set("Authorization", "Bearer "+generateTestToken(progressTestUserID))

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		t.Fatalf("failed to send request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		t.Errorf("expected status %d, got %d", http.StatusOK, resp.StatusCode)
	}

	contentType := resp.Header.Get("Content-Type")
	if contentType != "application/json" {
		t.Errorf("expected Content-Type 'application/json', got %q", contentType)
	}

	var result handler.ListProgressResponse
	if err := json.NewDecoder(resp.Body).Decode(&result); err != nil {
		t.Fatalf("failed to decode response: %v", err)
	}

	if len(result.Progress) != 2 {
		t.Fatalf("expected 2 progress items, got %d", len(result.Progress))
	}

	if result.Progress[0].Stars != 3 {
		t.Errorf("expected stars 3, got %d", result.Progress[0].Stars)
	}
}

func TestGetProgressIntegration_Unauthorized(t *testing.T) {
	store := newIntegrationProgressStore()
	srv := newProgressTestRouter(store)
	defer srv.Close()

	// No Authorization header
	resp, err := http.Get(srv.URL + "/api/users/me/progress")
	if err != nil {
		t.Fatalf("failed to send request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusUnauthorized {
		t.Errorf("expected status %d, got %d", http.StatusUnauthorized, resp.StatusCode)
	}
}

func TestGetProgressIntegration_InvalidToken(t *testing.T) {
	store := newIntegrationProgressStore()
	srv := newProgressTestRouter(store)
	defer srv.Close()

	req, _ := http.NewRequest(http.MethodGet, srv.URL+"/api/users/me/progress", nil)
	req.Header.Set("Authorization", "Bearer invalid-token")

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		t.Fatalf("failed to send request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusUnauthorized {
		t.Errorf("expected status %d, got %d", http.StatusUnauthorized, resp.StatusCode)
	}
}

// --- SaveProgress Integration Tests ---

func TestSaveProgressIntegration_Success(t *testing.T) {
	store := newIntegrationProgressStore()
	store.upserted = db.UserProgress{
		LevelID:         1,
		Stars:           2,
		BestTimeSeconds: pgtype.Float8{Float64: 30.0, Valid: true},
		Unlocked:        true,
		CompletedAt:     pgtype.Timestamptz{Time: time.Date(2025, 6, 1, 12, 0, 0, 0, time.UTC), Valid: true},
	}

	srv := newProgressTestRouter(store)
	defer srv.Close()

	body, _ := json.Marshal(map[string]interface{}{
		"stars":           2,
		"bestTimeSeconds": 30.0,
		"unlocked":        true,
		"completed":       true,
	})

	req, _ := http.NewRequest(http.MethodPut, srv.URL+"/api/users/me/progress/1", bytes.NewReader(body))
	req.Header.Set("Authorization", "Bearer "+generateTestToken(progressTestUserID))
	req.Header.Set("Content-Type", "application/json")

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		t.Fatalf("failed to send request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		t.Errorf("expected status %d, got %d", http.StatusOK, resp.StatusCode)
	}

	var result handler.ProgressResponse
	if err := json.NewDecoder(resp.Body).Decode(&result); err != nil {
		t.Fatalf("failed to decode response: %v", err)
	}

	if result.LevelID != 1 {
		t.Errorf("expected levelId 1, got %d", result.LevelID)
	}
	if result.Stars != 2 {
		t.Errorf("expected stars 2, got %d", result.Stars)
	}
}

func TestSaveProgressIntegration_InvalidLevelId(t *testing.T) {
	store := newIntegrationProgressStore()
	srv := newProgressTestRouter(store)
	defer srv.Close()

	body, _ := json.Marshal(map[string]interface{}{
		"stars":    1,
		"unlocked": true,
	})

	req, _ := http.NewRequest(http.MethodPut, srv.URL+"/api/users/me/progress/abc", bytes.NewReader(body))
	req.Header.Set("Authorization", "Bearer "+generateTestToken(progressTestUserID))
	req.Header.Set("Content-Type", "application/json")

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		t.Fatalf("failed to send request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusBadRequest {
		t.Errorf("expected status %d, got %d", http.StatusBadRequest, resp.StatusCode)
	}
}

func TestSaveProgressIntegration_InvalidStars(t *testing.T) {
	store := newIntegrationProgressStore()
	srv := newProgressTestRouter(store)
	defer srv.Close()

	body, _ := json.Marshal(map[string]interface{}{
		"stars":    5,
		"unlocked": true,
	})

	req, _ := http.NewRequest(http.MethodPut, srv.URL+"/api/users/me/progress/1", bytes.NewReader(body))
	req.Header.Set("Authorization", "Bearer "+generateTestToken(progressTestUserID))
	req.Header.Set("Content-Type", "application/json")

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		t.Fatalf("failed to send request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusBadRequest {
		t.Errorf("expected status %d, got %d", http.StatusBadRequest, resp.StatusCode)
	}
}

func TestSaveProgressIntegration_Unauthorized(t *testing.T) {
	store := newIntegrationProgressStore()
	srv := newProgressTestRouter(store)
	defer srv.Close()

	body, _ := json.Marshal(map[string]interface{}{
		"stars":    1,
		"unlocked": true,
	})

	req, _ := http.NewRequest(http.MethodPut, srv.URL+"/api/users/me/progress/1", bytes.NewReader(body))
	req.Header.Set("Content-Type", "application/json")

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		t.Fatalf("failed to send request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusUnauthorized {
		t.Errorf("expected status %d, got %d", http.StatusUnauthorized, resp.StatusCode)
	}
}

// --- GetStats Integration Tests ---

func TestGetStatsIntegration_Success(t *testing.T) {
	store := newIntegrationProgressStore()
	store.stats = db.GetUserStatsRow{
		TotalStars:      21,
		LevelsCompleted: 7,
		LevelsStarted:   10,
	}

	srv := newProgressTestRouter(store)
	defer srv.Close()

	req, _ := http.NewRequest(http.MethodGet, srv.URL+"/api/users/me/stats", nil)
	req.Header.Set("Authorization", "Bearer "+generateTestToken(progressTestUserID))

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		t.Fatalf("failed to send request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		t.Errorf("expected status %d, got %d", http.StatusOK, resp.StatusCode)
	}

	var result handler.StatsResponse
	if err := json.NewDecoder(resp.Body).Decode(&result); err != nil {
		t.Fatalf("failed to decode response: %v", err)
	}

	if result.TotalStars != 21 {
		t.Errorf("expected totalStars 21, got %d", result.TotalStars)
	}
	if result.LevelsCompleted != 7 {
		t.Errorf("expected levelsCompleted 7, got %d", result.LevelsCompleted)
	}
	if result.LevelsStarted != 10 {
		t.Errorf("expected levelsStarted 10, got %d", result.LevelsStarted)
	}
}

func TestGetStatsIntegration_Unauthorized(t *testing.T) {
	store := newIntegrationProgressStore()
	srv := newProgressTestRouter(store)
	defer srv.Close()

	resp, err := http.Get(srv.URL + "/api/users/me/stats")
	if err != nil {
		t.Fatalf("failed to send request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusUnauthorized {
		t.Errorf("expected status %d, got %d", http.StatusUnauthorized, resp.StatusCode)
	}
}

// --- Full Flow Integration Test ---

func TestProgressFlow_Integration(t *testing.T) {
	store := newIntegrationProgressStore()

	// Configure mock for the full flow
	store.upserted = db.UserProgress{
		LevelID:         1,
		Stars:           3,
		BestTimeSeconds: pgtype.Float8{Float64: 25.0, Valid: true},
		Unlocked:        true,
		CompletedAt:     pgtype.Timestamptz{Time: time.Now(), Valid: true},
	}
	store.progress = []db.UserProgress{store.upserted}
	store.stats = db.GetUserStatsRow{
		TotalStars:      3,
		LevelsCompleted: 1,
		LevelsStarted:   1,
	}

	srv := newProgressTestRouter(store)
	defer srv.Close()

	token := generateTestToken(progressTestUserID)

	// Step 1: Save progress
	saveBody, _ := json.Marshal(map[string]interface{}{
		"stars":           3,
		"bestTimeSeconds": 25.0,
		"unlocked":        true,
		"completed":       true,
	})

	saveReq, _ := http.NewRequest(http.MethodPut, srv.URL+"/api/users/me/progress/1", bytes.NewReader(saveBody))
	saveReq.Header.Set("Authorization", "Bearer "+token)
	saveReq.Header.Set("Content-Type", "application/json")

	saveResp, err := http.DefaultClient.Do(saveReq)
	if err != nil {
		t.Fatalf("save request failed: %v", err)
	}
	defer saveResp.Body.Close()

	if saveResp.StatusCode != http.StatusOK {
		t.Fatalf("save expected %d, got %d", http.StatusOK, saveResp.StatusCode)
	}

	// Step 2: Get all progress
	getReq, _ := http.NewRequest(http.MethodGet, srv.URL+"/api/users/me/progress", nil)
	getReq.Header.Set("Authorization", "Bearer "+token)

	getResp, err := http.DefaultClient.Do(getReq)
	if err != nil {
		t.Fatalf("get progress request failed: %v", err)
	}
	defer getResp.Body.Close()

	if getResp.StatusCode != http.StatusOK {
		t.Fatalf("get progress expected %d, got %d", http.StatusOK, getResp.StatusCode)
	}

	var progressResult handler.ListProgressResponse
	json.NewDecoder(getResp.Body).Decode(&progressResult)
	if len(progressResult.Progress) != 1 {
		t.Errorf("expected 1 progress item, got %d", len(progressResult.Progress))
	}

	// Step 3: Get stats
	statsReq, _ := http.NewRequest(http.MethodGet, srv.URL+"/api/users/me/stats", nil)
	statsReq.Header.Set("Authorization", "Bearer "+token)

	statsResp, err := http.DefaultClient.Do(statsReq)
	if err != nil {
		t.Fatalf("get stats request failed: %v", err)
	}
	defer statsResp.Body.Close()

	if statsResp.StatusCode != http.StatusOK {
		t.Fatalf("get stats expected %d, got %d", http.StatusOK, statsResp.StatusCode)
	}

	var statsResult handler.StatsResponse
	json.NewDecoder(statsResp.Body).Decode(&statsResult)
	if statsResult.TotalStars != 3 {
		t.Errorf("expected totalStars 3, got %d", statsResult.TotalStars)
	}
}
