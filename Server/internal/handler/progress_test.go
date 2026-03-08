package handler

import (
	"bytes"
	"context"
	"encoding/json"
	"errors"
	"net/http"
	"net/http/httptest"
	"testing"
	"time"

	"github.com/go-chi/chi/v5"
	"github.com/jackc/pgx/v5/pgtype"

	"github.com/gauthier-se/GreenSkills-API/internal/db"
	"github.com/gauthier-se/GreenSkills-API/internal/middleware"
)

// mockProgressStore implements ProgressStore for unit testing.
type mockProgressStore struct {
	progress  []db.UserProgress
	upserted  db.UserProgress
	stats     db.GetUserStatsRow
	getErr    error
	upsertErr error
	statsErr  error
}

func newMockProgressStore() *mockProgressStore {
	return &mockProgressStore{}
}

func (m *mockProgressStore) GetUserProgress(_ context.Context, _ pgtype.UUID) ([]db.UserProgress, error) {
	if m.getErr != nil {
		return nil, m.getErr
	}
	return m.progress, nil
}

func (m *mockProgressStore) UpsertUserProgress(_ context.Context, _ db.UpsertUserProgressParams) (db.UserProgress, error) {
	if m.upsertErr != nil {
		return db.UserProgress{}, m.upsertErr
	}
	return m.upserted, nil
}

func (m *mockProgressStore) GetUserStats(_ context.Context, _ pgtype.UUID) (db.GetUserStatsRow, error) {
	if m.statsErr != nil {
		return db.GetUserStatsRow{}, m.statsErr
	}
	return m.stats, nil
}

// testUserID is a valid UUID string for testing.
const testUserID = "01020304-0506-0708-090a-0b0c0d0e0f10"

// withUserID injects a user ID into the request context, simulating the auth middleware.
func withUserID(r *http.Request, userID string) *http.Request {
	ctx := context.WithValue(r.Context(), middleware.UserIDKey, userID)
	return r.WithContext(ctx)
}

// --- GetProgress Tests ---

func TestGetProgress_Success(t *testing.T) {
	store := newMockProgressStore()
	store.progress = []db.UserProgress{
		{
			LevelID:         1,
			Stars:           3,
			BestTimeSeconds: pgtype.Float8{Float64: 45.5, Valid: true},
			Unlocked:        true,
			CompletedAt:     pgtype.Timestamptz{Time: time.Date(2025, 1, 1, 0, 0, 0, 0, time.UTC), Valid: true},
		},
		{
			LevelID:  2,
			Stars:    1,
			Unlocked: true,
		},
	}
	h := NewProgressHandler(store)

	req := httptest.NewRequest(http.MethodGet, "/api/users/me/progress", nil)
	req = withUserID(req, testUserID)
	rec := httptest.NewRecorder()

	h.GetProgress().ServeHTTP(rec, req)

	if rec.Code != http.StatusOK {
		t.Errorf("expected status %d, got %d", http.StatusOK, rec.Code)
	}

	var resp ListProgressResponse
	if err := json.NewDecoder(rec.Body).Decode(&resp); err != nil {
		t.Fatalf("failed to decode response: %v", err)
	}

	if len(resp.Progress) != 2 {
		t.Fatalf("expected 2 progress items, got %d", len(resp.Progress))
	}

	p1 := resp.Progress[0]
	if p1.LevelID != 1 {
		t.Errorf("expected levelId 1, got %d", p1.LevelID)
	}
	if p1.Stars != 3 {
		t.Errorf("expected stars 3, got %d", p1.Stars)
	}
	if p1.BestTimeSeconds == nil || *p1.BestTimeSeconds != 45.5 {
		t.Errorf("expected bestTimeSeconds 45.5, got %v", p1.BestTimeSeconds)
	}
	if !p1.Unlocked {
		t.Error("expected unlocked true")
	}
	if p1.CompletedAt == nil {
		t.Error("expected completedAt to be set")
	}

	p2 := resp.Progress[1]
	if p2.BestTimeSeconds != nil {
		t.Errorf("expected nil bestTimeSeconds, got %v", p2.BestTimeSeconds)
	}
	if p2.CompletedAt != nil {
		t.Errorf("expected nil completedAt, got %v", p2.CompletedAt)
	}
}

func TestGetProgress_Empty(t *testing.T) {
	store := newMockProgressStore()
	store.progress = []db.UserProgress{}
	h := NewProgressHandler(store)

	req := httptest.NewRequest(http.MethodGet, "/api/users/me/progress", nil)
	req = withUserID(req, testUserID)
	rec := httptest.NewRecorder()

	h.GetProgress().ServeHTTP(rec, req)

	if rec.Code != http.StatusOK {
		t.Errorf("expected status %d, got %d", http.StatusOK, rec.Code)
	}

	var resp ListProgressResponse
	json.NewDecoder(rec.Body).Decode(&resp)
	if len(resp.Progress) != 0 {
		t.Errorf("expected empty progress, got %d items", len(resp.Progress))
	}
}

func TestGetProgress_StoreError(t *testing.T) {
	store := newMockProgressStore()
	store.getErr = errors.New("db error")
	h := NewProgressHandler(store)

	req := httptest.NewRequest(http.MethodGet, "/api/users/me/progress", nil)
	req = withUserID(req, testUserID)
	rec := httptest.NewRecorder()

	h.GetProgress().ServeHTTP(rec, req)

	if rec.Code != http.StatusInternalServerError {
		t.Errorf("expected status %d, got %d", http.StatusInternalServerError, rec.Code)
	}
}

func TestGetProgress_InvalidUserID(t *testing.T) {
	store := newMockProgressStore()
	h := NewProgressHandler(store)

	req := httptest.NewRequest(http.MethodGet, "/api/users/me/progress", nil)
	req = withUserID(req, "not-a-uuid")
	rec := httptest.NewRecorder()

	h.GetProgress().ServeHTTP(rec, req)

	if rec.Code != http.StatusUnauthorized {
		t.Errorf("expected status %d, got %d", http.StatusUnauthorized, rec.Code)
	}
}

// --- SaveProgress Tests ---

// withChiURLParam sets a chi URL parameter on the request context.
func withChiURLParam(r *http.Request, key, value string) *http.Request {
	rctx := chi.NewRouteContext()
	rctx.URLParams.Add(key, value)
	return r.WithContext(context.WithValue(r.Context(), chi.RouteCtxKey, rctx))
}

func TestSaveProgress_Success(t *testing.T) {
	store := newMockProgressStore()
	store.upserted = db.UserProgress{
		LevelID:         1,
		Stars:           2,
		BestTimeSeconds: pgtype.Float8{Float64: 30.0, Valid: true},
		Unlocked:        true,
		CompletedAt:     pgtype.Timestamptz{Time: time.Date(2025, 6, 1, 12, 0, 0, 0, time.UTC), Valid: true},
	}
	h := NewProgressHandler(store)

	bestTime := 30.0
	body := SaveProgressRequest{
		Stars:           2,
		BestTimeSeconds: &bestTime,
		Unlocked:        true,
		Completed:       true,
	}
	jsonBody, _ := json.Marshal(body)

	req := httptest.NewRequest(http.MethodPut, "/api/users/me/progress/1", bytes.NewReader(jsonBody))
	req = withUserID(req, testUserID)
	req = withChiURLParam(req, "levelId", "1")
	rec := httptest.NewRecorder()

	h.SaveProgress().ServeHTTP(rec, req)

	if rec.Code != http.StatusOK {
		t.Errorf("expected status %d, got %d", http.StatusOK, rec.Code)
	}

	var resp ProgressResponse
	if err := json.NewDecoder(rec.Body).Decode(&resp); err != nil {
		t.Fatalf("failed to decode response: %v", err)
	}

	if resp.LevelID != 1 {
		t.Errorf("expected levelId 1, got %d", resp.LevelID)
	}
	if resp.Stars != 2 {
		t.Errorf("expected stars 2, got %d", resp.Stars)
	}
	if resp.BestTimeSeconds == nil || *resp.BestTimeSeconds != 30.0 {
		t.Errorf("expected bestTimeSeconds 30.0, got %v", resp.BestTimeSeconds)
	}
	if resp.CompletedAt == nil {
		t.Error("expected completedAt to be set")
	}
}

func TestSaveProgress_UnlockOnly(t *testing.T) {
	store := newMockProgressStore()
	store.upserted = db.UserProgress{
		LevelID:  3,
		Stars:    0,
		Unlocked: true,
	}
	h := NewProgressHandler(store)

	body := SaveProgressRequest{
		Stars:    0,
		Unlocked: true,
	}
	jsonBody, _ := json.Marshal(body)

	req := httptest.NewRequest(http.MethodPut, "/api/users/me/progress/3", bytes.NewReader(jsonBody))
	req = withUserID(req, testUserID)
	req = withChiURLParam(req, "levelId", "3")
	rec := httptest.NewRecorder()

	h.SaveProgress().ServeHTTP(rec, req)

	if rec.Code != http.StatusOK {
		t.Errorf("expected status %d, got %d", http.StatusOK, rec.Code)
	}

	var resp ProgressResponse
	json.NewDecoder(rec.Body).Decode(&resp)
	if resp.BestTimeSeconds != nil {
		t.Errorf("expected nil bestTimeSeconds, got %v", resp.BestTimeSeconds)
	}
	if resp.CompletedAt != nil {
		t.Errorf("expected nil completedAt, got %v", resp.CompletedAt)
	}
}

func TestSaveProgress_InvalidLevelId(t *testing.T) {
	store := newMockProgressStore()
	h := NewProgressHandler(store)

	req := httptest.NewRequest(http.MethodPut, "/api/users/me/progress/abc", nil)
	req = withUserID(req, testUserID)
	req = withChiURLParam(req, "levelId", "abc")
	rec := httptest.NewRecorder()

	h.SaveProgress().ServeHTTP(rec, req)

	if rec.Code != http.StatusBadRequest {
		t.Errorf("expected status %d, got %d", http.StatusBadRequest, rec.Code)
	}

	var resp ErrorResponse
	json.NewDecoder(rec.Body).Decode(&resp)
	if resp.Error != "invalid level id" {
		t.Errorf("expected 'invalid level id', got %q", resp.Error)
	}
}

func TestSaveProgress_ZeroLevelId(t *testing.T) {
	store := newMockProgressStore()
	h := NewProgressHandler(store)

	body := SaveProgressRequest{Stars: 1, Unlocked: true}
	jsonBody, _ := json.Marshal(body)

	req := httptest.NewRequest(http.MethodPut, "/api/users/me/progress/0", bytes.NewReader(jsonBody))
	req = withUserID(req, testUserID)
	req = withChiURLParam(req, "levelId", "0")
	rec := httptest.NewRecorder()

	h.SaveProgress().ServeHTTP(rec, req)

	if rec.Code != http.StatusBadRequest {
		t.Errorf("expected status %d, got %d", http.StatusBadRequest, rec.Code)
	}
}

func TestSaveProgress_InvalidStars(t *testing.T) {
	store := newMockProgressStore()
	h := NewProgressHandler(store)

	body := SaveProgressRequest{Stars: 5, Unlocked: true}
	jsonBody, _ := json.Marshal(body)

	req := httptest.NewRequest(http.MethodPut, "/api/users/me/progress/1", bytes.NewReader(jsonBody))
	req = withUserID(req, testUserID)
	req = withChiURLParam(req, "levelId", "1")
	rec := httptest.NewRecorder()

	h.SaveProgress().ServeHTTP(rec, req)

	if rec.Code != http.StatusBadRequest {
		t.Errorf("expected status %d, got %d", http.StatusBadRequest, rec.Code)
	}

	var resp ErrorResponse
	json.NewDecoder(rec.Body).Decode(&resp)
	if resp.Error != "stars must be between 0 and 3" {
		t.Errorf("expected stars validation error, got %q", resp.Error)
	}
}

func TestSaveProgress_NegativeStars(t *testing.T) {
	store := newMockProgressStore()
	h := NewProgressHandler(store)

	body := SaveProgressRequest{Stars: -1, Unlocked: true}
	jsonBody, _ := json.Marshal(body)

	req := httptest.NewRequest(http.MethodPut, "/api/users/me/progress/1", bytes.NewReader(jsonBody))
	req = withUserID(req, testUserID)
	req = withChiURLParam(req, "levelId", "1")
	rec := httptest.NewRecorder()

	h.SaveProgress().ServeHTTP(rec, req)

	if rec.Code != http.StatusBadRequest {
		t.Errorf("expected status %d, got %d", http.StatusBadRequest, rec.Code)
	}
}

func TestSaveProgress_InvalidJSON(t *testing.T) {
	store := newMockProgressStore()
	h := NewProgressHandler(store)

	req := httptest.NewRequest(http.MethodPut, "/api/users/me/progress/1", bytes.NewReader([]byte("{bad")))
	req = withUserID(req, testUserID)
	req = withChiURLParam(req, "levelId", "1")
	rec := httptest.NewRecorder()

	h.SaveProgress().ServeHTTP(rec, req)

	if rec.Code != http.StatusBadRequest {
		t.Errorf("expected status %d, got %d", http.StatusBadRequest, rec.Code)
	}
}

func TestSaveProgress_StoreError(t *testing.T) {
	store := newMockProgressStore()
	store.upsertErr = errors.New("db error")
	h := NewProgressHandler(store)

	body := SaveProgressRequest{Stars: 1, Unlocked: true}
	jsonBody, _ := json.Marshal(body)

	req := httptest.NewRequest(http.MethodPut, "/api/users/me/progress/1", bytes.NewReader(jsonBody))
	req = withUserID(req, testUserID)
	req = withChiURLParam(req, "levelId", "1")
	rec := httptest.NewRecorder()

	h.SaveProgress().ServeHTTP(rec, req)

	if rec.Code != http.StatusInternalServerError {
		t.Errorf("expected status %d, got %d", http.StatusInternalServerError, rec.Code)
	}
}

func TestSaveProgress_InvalidUserID(t *testing.T) {
	store := newMockProgressStore()
	h := NewProgressHandler(store)

	req := httptest.NewRequest(http.MethodPut, "/api/users/me/progress/1", nil)
	req = withUserID(req, "bad-uuid")
	req = withChiURLParam(req, "levelId", "1")
	rec := httptest.NewRecorder()

	h.SaveProgress().ServeHTTP(rec, req)

	if rec.Code != http.StatusUnauthorized {
		t.Errorf("expected status %d, got %d", http.StatusUnauthorized, rec.Code)
	}
}

// --- GetStats Tests ---

func TestGetStats_Success(t *testing.T) {
	store := newMockProgressStore()
	store.stats = db.GetUserStatsRow{
		TotalStars:      15,
		LevelsCompleted: 5,
		LevelsStarted:   7,
	}
	h := NewProgressHandler(store)

	req := httptest.NewRequest(http.MethodGet, "/api/users/me/stats", nil)
	req = withUserID(req, testUserID)
	rec := httptest.NewRecorder()

	h.GetStats().ServeHTTP(rec, req)

	if rec.Code != http.StatusOK {
		t.Errorf("expected status %d, got %d", http.StatusOK, rec.Code)
	}

	var resp StatsResponse
	if err := json.NewDecoder(rec.Body).Decode(&resp); err != nil {
		t.Fatalf("failed to decode response: %v", err)
	}

	if resp.TotalStars != 15 {
		t.Errorf("expected totalStars 15, got %d", resp.TotalStars)
	}
	if resp.LevelsCompleted != 5 {
		t.Errorf("expected levelsCompleted 5, got %d", resp.LevelsCompleted)
	}
	if resp.LevelsStarted != 7 {
		t.Errorf("expected levelsStarted 7, got %d", resp.LevelsStarted)
	}
}

func TestGetStats_ZeroProgress(t *testing.T) {
	store := newMockProgressStore()
	store.stats = db.GetUserStatsRow{
		TotalStars:      0,
		LevelsCompleted: 0,
		LevelsStarted:   0,
	}
	h := NewProgressHandler(store)

	req := httptest.NewRequest(http.MethodGet, "/api/users/me/stats", nil)
	req = withUserID(req, testUserID)
	rec := httptest.NewRecorder()

	h.GetStats().ServeHTTP(rec, req)

	if rec.Code != http.StatusOK {
		t.Errorf("expected status %d, got %d", http.StatusOK, rec.Code)
	}

	var resp StatsResponse
	json.NewDecoder(rec.Body).Decode(&resp)
	if resp.TotalStars != 0 || resp.LevelsCompleted != 0 || resp.LevelsStarted != 0 {
		t.Errorf("expected all zeros, got stars=%d completed=%d started=%d",
			resp.TotalStars, resp.LevelsCompleted, resp.LevelsStarted)
	}
}

func TestGetStats_StoreError(t *testing.T) {
	store := newMockProgressStore()
	store.statsErr = errors.New("db error")
	h := NewProgressHandler(store)

	req := httptest.NewRequest(http.MethodGet, "/api/users/me/stats", nil)
	req = withUserID(req, testUserID)
	rec := httptest.NewRecorder()

	h.GetStats().ServeHTTP(rec, req)

	if rec.Code != http.StatusInternalServerError {
		t.Errorf("expected status %d, got %d", http.StatusInternalServerError, rec.Code)
	}
}

func TestGetStats_InvalidUserID(t *testing.T) {
	store := newMockProgressStore()
	h := NewProgressHandler(store)

	req := httptest.NewRequest(http.MethodGet, "/api/users/me/stats", nil)
	req = withUserID(req, "invalid")
	rec := httptest.NewRecorder()

	h.GetStats().ServeHTTP(rec, req)

	if rec.Code != http.StatusUnauthorized {
		t.Errorf("expected status %d, got %d", http.StatusUnauthorized, rec.Code)
	}
}

// --- parseUUID Tests ---

func TestParseUUID_Valid(t *testing.T) {
	uuid, err := parseUUID("01020304-0506-0708-090a-0b0c0d0e0f10")
	if err != nil {
		t.Fatalf("unexpected error: %v", err)
	}
	if !uuid.Valid {
		t.Error("expected valid UUID")
	}

	// Verify round-trip
	got := uuidToString(uuid)
	if got != "01020304-0506-0708-090a-0b0c0d0e0f10" {
		t.Errorf("round-trip failed: got %q", got)
	}
}

func TestParseUUID_Invalid(t *testing.T) {
	_, err := parseUUID("not-a-uuid")
	if err == nil {
		t.Error("expected error for invalid UUID")
	}
}

func TestParseUUID_Empty(t *testing.T) {
	_, err := parseUUID("")
	if err == nil {
		t.Error("expected error for empty string")
	}
}
