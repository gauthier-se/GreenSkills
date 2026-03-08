package handler

import (
	"context"
	"encoding/json"
	"errors"
	"net/http"
	"net/http/httptest"
	"testing"

	"github.com/go-chi/chi/v5"
	"github.com/jackc/pgx/v5"
	"github.com/jackc/pgx/v5/pgtype"

	"github.com/gauthier-se/GreenSkills-API/internal/db"
)

// mockLevelsStore implements LevelsStore for unit testing.
type mockLevelsStore struct {
	levels    []db.Level
	exercises map[int32][]db.Exercise // keyed by level DB ID
	counts    map[int32]int64         // keyed by level DB ID
	listErr   error
	getErr    error
	exErr     error
	countErr  error
}

func newMockLevelsStore() *mockLevelsStore {
	return &mockLevelsStore{
		levels:    []db.Level{},
		exercises: make(map[int32][]db.Exercise),
		counts:    make(map[int32]int64),
	}
}

func (m *mockLevelsStore) ListLevels(_ context.Context) ([]db.Level, error) {
	if m.listErr != nil {
		return nil, m.listErr
	}
	return m.levels, nil
}

func (m *mockLevelsStore) GetLevel(_ context.Context, levelNumber int32) (db.Level, error) {
	if m.getErr != nil {
		return db.Level{}, m.getErr
	}
	for _, l := range m.levels {
		if l.LevelNumber == levelNumber {
			return l, nil
		}
	}
	return db.Level{}, pgx.ErrNoRows
}

func (m *mockLevelsStore) ListExercisesByLevelID(_ context.Context, levelID int32) ([]db.Exercise, error) {
	if m.exErr != nil {
		return nil, m.exErr
	}
	return m.exercises[levelID], nil
}

func (m *mockLevelsStore) CountExercisesByLevelID(_ context.Context, levelID int32) (int64, error) {
	if m.countErr != nil {
		return 0, m.countErr
	}
	return m.counts[levelID], nil
}

func (m *mockLevelsStore) addLevel(id, levelNumber int32, theme string) {
	m.levels = append(m.levels, db.Level{
		ID:          id,
		LevelNumber: levelNumber,
		Theme:       theme,
		CreatedAt:   pgtype.Timestamptz{Valid: true},
	})
}

func (m *mockLevelsStore) addExercise(levelID int32, exerciseType string, difficulty, category int32, explanation string, dataJSON string) {
	exercise := db.Exercise{
		ID:           int32(len(m.exercises[levelID]) + 1),
		LevelID:      levelID,
		ExerciseType: exerciseType,
		Difficulty:   difficulty,
		Category:     category,
		Explanation:  explanation,
		DataJson:     []byte(dataJSON),
		SortOrder:    int32(len(m.exercises[levelID]) + 1),
		CreatedAt:    pgtype.Timestamptz{Valid: true},
	}
	m.exercises[levelID] = append(m.exercises[levelID], exercise)
	m.counts[levelID] = int64(len(m.exercises[levelID]))
}

func newTestLevelsHandler(store *mockLevelsStore) *LevelsHandler {
	return NewLevelsHandler(store)
}

// --- ListLevels Tests ---

func TestListLevels_Success(t *testing.T) {
	store := newMockLevelsStore()
	store.addLevel(1, 1, "Tri des Déchets")
	store.addLevel(2, 2, "Énergie Numérique")
	store.counts[1] = 3
	store.counts[2] = 4
	h := newTestLevelsHandler(store)

	req := httptest.NewRequest(http.MethodGet, "/api/levels", nil)
	rec := httptest.NewRecorder()

	h.ListLevels().ServeHTTP(rec, req)

	if rec.Code != http.StatusOK {
		t.Errorf("expected status %d, got %d", http.StatusOK, rec.Code)
	}

	var resp ListLevelsResponse
	if err := json.NewDecoder(rec.Body).Decode(&resp); err != nil {
		t.Fatalf("failed to decode response: %v", err)
	}

	if len(resp.Levels) != 2 {
		t.Fatalf("expected 2 levels, got %d", len(resp.Levels))
	}

	if resp.Levels[0].LevelID != 1 {
		t.Errorf("expected levelId 1, got %d", resp.Levels[0].LevelID)
	}

	if resp.Levels[0].Theme != "Tri des Déchets" {
		t.Errorf("expected theme 'Tri des Déchets', got %q", resp.Levels[0].Theme)
	}

	if resp.Levels[0].ExerciseCount != 3 {
		t.Errorf("expected exerciseCount 3, got %d", resp.Levels[0].ExerciseCount)
	}

	if resp.Levels[1].LevelID != 2 {
		t.Errorf("expected levelId 2, got %d", resp.Levels[1].LevelID)
	}

	if resp.Levels[1].ExerciseCount != 4 {
		t.Errorf("expected exerciseCount 4, got %d", resp.Levels[1].ExerciseCount)
	}
}

func TestListLevels_Empty(t *testing.T) {
	store := newMockLevelsStore()
	h := newTestLevelsHandler(store)

	req := httptest.NewRequest(http.MethodGet, "/api/levels", nil)
	rec := httptest.NewRecorder()

	h.ListLevels().ServeHTTP(rec, req)

	if rec.Code != http.StatusOK {
		t.Errorf("expected status %d, got %d", http.StatusOK, rec.Code)
	}

	var resp ListLevelsResponse
	if err := json.NewDecoder(rec.Body).Decode(&resp); err != nil {
		t.Fatalf("failed to decode response: %v", err)
	}

	if len(resp.Levels) != 0 {
		t.Errorf("expected 0 levels, got %d", len(resp.Levels))
	}
}

func TestListLevels_DBError(t *testing.T) {
	store := newMockLevelsStore()
	store.listErr = errors.New("database connection lost")
	h := newTestLevelsHandler(store)

	req := httptest.NewRequest(http.MethodGet, "/api/levels", nil)
	rec := httptest.NewRecorder()

	h.ListLevels().ServeHTTP(rec, req)

	if rec.Code != http.StatusInternalServerError {
		t.Errorf("expected status %d, got %d", http.StatusInternalServerError, rec.Code)
	}

	var resp ErrorResponse
	json.NewDecoder(rec.Body).Decode(&resp)
	if resp.Error != "internal server error" {
		t.Errorf("expected 'internal server error', got %q", resp.Error)
	}
}

func TestListLevels_CountError(t *testing.T) {
	store := newMockLevelsStore()
	store.addLevel(1, 1, "Test Level")
	store.countErr = errors.New("count failed")
	h := newTestLevelsHandler(store)

	req := httptest.NewRequest(http.MethodGet, "/api/levels", nil)
	rec := httptest.NewRecorder()

	h.ListLevels().ServeHTTP(rec, req)

	if rec.Code != http.StatusInternalServerError {
		t.Errorf("expected status %d, got %d", http.StatusInternalServerError, rec.Code)
	}
}

// --- GetLevelExercises Tests ---

// newRequestWithChiParam creates a request with a chi URL parameter injected.
func newRequestWithChiParam(method, path, paramName, paramValue string) *http.Request {
	req := httptest.NewRequest(method, path, nil)
	rctx := chi.NewRouteContext()
	rctx.URLParams.Add(paramName, paramValue)
	return req.WithContext(context.WithValue(req.Context(), chi.RouteCtxKey, rctx))
}

func TestGetLevelExercises_Success_Quiz(t *testing.T) {
	store := newMockLevelsStore()
	store.addLevel(1, 1, "Tri des Déchets")
	store.addExercise(1, "quiz", 0, 0,
		"Le verre se recycle à l'infini !",
		`{"questionText": "Dans quelle poubelle va la bouteille en verre ?", "options": ["Verte", "Jaune", "Bleue"], "correctOptionIndex": 0}`,
	)
	h := newTestLevelsHandler(store)

	req := newRequestWithChiParam(http.MethodGet, "/api/levels/1/exercises", "id", "1")
	rec := httptest.NewRecorder()

	h.GetLevelExercises().ServeHTTP(rec, req)

	if rec.Code != http.StatusOK {
		t.Errorf("expected status %d, got %d", http.StatusOK, rec.Code)
	}

	var resp LevelWithExercisesResponse
	if err := json.NewDecoder(rec.Body).Decode(&resp); err != nil {
		t.Fatalf("failed to decode response: %v", err)
	}

	if resp.LevelID != 1 {
		t.Errorf("expected levelId 1, got %d", resp.LevelID)
	}

	if resp.Theme != "Tri des Déchets" {
		t.Errorf("expected theme 'Tri des Déchets', got %q", resp.Theme)
	}

	if len(resp.Exercises) != 1 {
		t.Fatalf("expected 1 exercise, got %d", len(resp.Exercises))
	}

	ex := resp.Exercises[0]
	if ex["exerciseType"] != "quiz" {
		t.Errorf("expected exerciseType 'quiz', got %v", ex["exerciseType"])
	}

	if ex["questionText"] != "Dans quelle poubelle va la bouteille en verre ?" {
		t.Errorf("unexpected questionText: %v", ex["questionText"])
	}

	if ex["explanation"] != "Le verre se recycle à l'infini !" {
		t.Errorf("unexpected explanation: %v", ex["explanation"])
	}

	// correctOptionIndex should be present as a number
	if idx, ok := ex["correctOptionIndex"].(float64); !ok || idx != 0 {
		t.Errorf("expected correctOptionIndex 0, got %v", ex["correctOptionIndex"])
	}
}

func TestGetLevelExercises_Success_TrueFalse(t *testing.T) {
	store := newMockLevelsStore()
	store.addLevel(1, 1, "Test Level")
	store.addExercise(1, "trueFalse", 1, 0,
		"Explanation text",
		`{"statement": "Some statement", "isTrue": false}`,
	)
	h := newTestLevelsHandler(store)

	req := newRequestWithChiParam(http.MethodGet, "/api/levels/1/exercises", "id", "1")
	rec := httptest.NewRecorder()

	h.GetLevelExercises().ServeHTTP(rec, req)

	if rec.Code != http.StatusOK {
		t.Errorf("expected status %d, got %d", http.StatusOK, rec.Code)
	}

	var resp LevelWithExercisesResponse
	json.NewDecoder(rec.Body).Decode(&resp)

	ex := resp.Exercises[0]
	if ex["exerciseType"] != "trueFalse" {
		t.Errorf("expected exerciseType 'trueFalse', got %v", ex["exerciseType"])
	}

	if ex["statement"] != "Some statement" {
		t.Errorf("unexpected statement: %v", ex["statement"])
	}

	if ex["isTrue"] != false {
		t.Errorf("expected isTrue false, got %v", ex["isTrue"])
	}
}

func TestGetLevelExercises_Success_MultipleExercises(t *testing.T) {
	store := newMockLevelsStore()
	store.addLevel(1, 1, "Test Level")
	store.addExercise(1, "quiz", 0, 0, "Exp1",
		`{"questionText": "Q1", "options": ["A", "B"], "correctOptionIndex": 0}`)
	store.addExercise(1, "trueFalse", 1, 0, "Exp2",
		`{"statement": "S1", "isTrue": true}`)
	store.addExercise(1, "fillInBlank", 2, 3, "Exp3",
		`{"sentenceWithBlanks": "The {0} is green.", "correctAnswers": ["grass"], "wordOptions": ["grass", "sky"], "caseSensitive": false}`)
	h := newTestLevelsHandler(store)

	req := newRequestWithChiParam(http.MethodGet, "/api/levels/1/exercises", "id", "1")
	rec := httptest.NewRecorder()

	h.GetLevelExercises().ServeHTTP(rec, req)

	if rec.Code != http.StatusOK {
		t.Errorf("expected status %d, got %d", http.StatusOK, rec.Code)
	}

	var resp LevelWithExercisesResponse
	json.NewDecoder(rec.Body).Decode(&resp)

	if len(resp.Exercises) != 3 {
		t.Fatalf("expected 3 exercises, got %d", len(resp.Exercises))
	}

	// Verify each exercise has the common fields
	for i, ex := range resp.Exercises {
		if _, ok := ex["exerciseType"]; !ok {
			t.Errorf("exercise %d: missing exerciseType", i)
		}
		if _, ok := ex["difficulty"]; !ok {
			t.Errorf("exercise %d: missing difficulty", i)
		}
		if _, ok := ex["category"]; !ok {
			t.Errorf("exercise %d: missing category", i)
		}
		if _, ok := ex["explanation"]; !ok {
			t.Errorf("exercise %d: missing explanation", i)
		}
	}
}

func TestGetLevelExercises_NotFound(t *testing.T) {
	store := newMockLevelsStore()
	h := newTestLevelsHandler(store)

	req := newRequestWithChiParam(http.MethodGet, "/api/levels/999/exercises", "id", "999")
	rec := httptest.NewRecorder()

	h.GetLevelExercises().ServeHTTP(rec, req)

	if rec.Code != http.StatusNotFound {
		t.Errorf("expected status %d, got %d", http.StatusNotFound, rec.Code)
	}

	var resp ErrorResponse
	json.NewDecoder(rec.Body).Decode(&resp)
	if resp.Error != "level not found" {
		t.Errorf("expected 'level not found', got %q", resp.Error)
	}
}

func TestGetLevelExercises_InvalidID(t *testing.T) {
	store := newMockLevelsStore()
	h := newTestLevelsHandler(store)

	req := newRequestWithChiParam(http.MethodGet, "/api/levels/abc/exercises", "id", "abc")
	rec := httptest.NewRecorder()

	h.GetLevelExercises().ServeHTTP(rec, req)

	if rec.Code != http.StatusBadRequest {
		t.Errorf("expected status %d, got %d", http.StatusBadRequest, rec.Code)
	}

	var resp ErrorResponse
	json.NewDecoder(rec.Body).Decode(&resp)
	if resp.Error != "invalid level id" {
		t.Errorf("expected 'invalid level id', got %q", resp.Error)
	}
}

func TestGetLevelExercises_DBError(t *testing.T) {
	store := newMockLevelsStore()
	store.getErr = errors.New("database error")
	h := newTestLevelsHandler(store)

	req := newRequestWithChiParam(http.MethodGet, "/api/levels/1/exercises", "id", "1")
	rec := httptest.NewRecorder()

	h.GetLevelExercises().ServeHTTP(rec, req)

	if rec.Code != http.StatusInternalServerError {
		t.Errorf("expected status %d, got %d", http.StatusInternalServerError, rec.Code)
	}
}

func TestGetLevelExercises_ExercisesDBError(t *testing.T) {
	store := newMockLevelsStore()
	store.addLevel(1, 1, "Test Level")
	store.exErr = errors.New("exercises query failed")
	h := newTestLevelsHandler(store)

	req := newRequestWithChiParam(http.MethodGet, "/api/levels/1/exercises", "id", "1")
	rec := httptest.NewRecorder()

	h.GetLevelExercises().ServeHTTP(rec, req)

	if rec.Code != http.StatusInternalServerError {
		t.Errorf("expected status %d, got %d", http.StatusInternalServerError, rec.Code)
	}
}

func TestGetLevelExercises_EmptyExercises(t *testing.T) {
	store := newMockLevelsStore()
	store.addLevel(1, 1, "Empty Level")
	h := newTestLevelsHandler(store)

	req := newRequestWithChiParam(http.MethodGet, "/api/levels/1/exercises", "id", "1")
	rec := httptest.NewRecorder()

	h.GetLevelExercises().ServeHTTP(rec, req)

	if rec.Code != http.StatusOK {
		t.Errorf("expected status %d, got %d", http.StatusOK, rec.Code)
	}

	var resp LevelWithExercisesResponse
	json.NewDecoder(rec.Body).Decode(&resp)

	if resp.LevelID != 1 {
		t.Errorf("expected levelId 1, got %d", resp.LevelID)
	}

	if len(resp.Exercises) != 0 {
		t.Errorf("expected 0 exercises, got %d", len(resp.Exercises))
	}
}

// --- buildExerciseDto Tests ---

func TestBuildExerciseDto_MergesFields(t *testing.T) {
	exercise := db.Exercise{
		ID:           1,
		ExerciseType: "quiz",
		Difficulty:   0,
		Category:     0,
		Explanation:  "Test explanation",
		DataJson:     []byte(`{"questionText": "Q?", "options": ["A", "B"], "correctOptionIndex": 1}`),
	}

	dto, err := buildExerciseDto(exercise)
	if err != nil {
		t.Fatalf("unexpected error: %v", err)
	}

	// Check common fields
	if dto["exerciseType"] != "quiz" {
		t.Errorf("expected exerciseType 'quiz', got %v", dto["exerciseType"])
	}
	if dto["explanation"] != "Test explanation" {
		t.Errorf("expected explanation 'Test explanation', got %v", dto["explanation"])
	}

	// Check data_json fields
	if dto["questionText"] != "Q?" {
		t.Errorf("expected questionText 'Q?', got %v", dto["questionText"])
	}

	// difficulty and category are int32, JSON will serialize as float64
	if diff, ok := dto["difficulty"].(int32); !ok || diff != 0 {
		t.Errorf("expected difficulty 0, got %v", dto["difficulty"])
	}
	if cat, ok := dto["category"].(int32); !ok || cat != 0 {
		t.Errorf("expected category 0, got %v", dto["category"])
	}
}

func TestBuildExerciseDto_InvalidJSON(t *testing.T) {
	exercise := db.Exercise{
		DataJson: []byte(`{invalid json`),
	}

	_, err := buildExerciseDto(exercise)
	if err == nil {
		t.Error("expected error for invalid JSON, got nil")
	}
}
