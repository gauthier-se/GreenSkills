package handler_test

import (
	"context"
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"testing"
	"time"

	"github.com/jackc/pgx/v5"
	"github.com/jackc/pgx/v5/pgtype"

	"github.com/gauthier-se/GreenSkills-API/internal/db"
	"github.com/gauthier-se/GreenSkills-API/internal/handler"
	"github.com/gauthier-se/GreenSkills-API/internal/router"
)

// integrationMockLevelsStore implements handler.LevelsStore for integration tests.
type integrationMockLevelsStore struct {
	levels    []db.Level
	exercises map[int32][]db.Exercise
	counts    map[int32]int64
}

func newIntegrationMockLevelsStore() *integrationMockLevelsStore {
	return &integrationMockLevelsStore{
		levels:    []db.Level{},
		exercises: make(map[int32][]db.Exercise),
		counts:    make(map[int32]int64),
	}
}

func (m *integrationMockLevelsStore) ListLevels(_ context.Context) ([]db.Level, error) {
	return m.levels, nil
}

func (m *integrationMockLevelsStore) GetLevel(_ context.Context, levelNumber int32) (db.Level, error) {
	for _, l := range m.levels {
		if l.LevelNumber == levelNumber {
			return l, nil
		}
	}
	return db.Level{}, pgx.ErrNoRows
}

func (m *integrationMockLevelsStore) ListExercisesByLevelID(_ context.Context, levelID int32) ([]db.Exercise, error) {
	return m.exercises[levelID], nil
}

func (m *integrationMockLevelsStore) CountExercisesByLevelID(_ context.Context, levelID int32) (int64, error) {
	return m.counts[levelID], nil
}

func (m *integrationMockLevelsStore) addLevel(id, levelNumber int32, theme string) {
	m.levels = append(m.levels, db.Level{
		ID:          id,
		LevelNumber: levelNumber,
		Theme:       theme,
		CreatedAt:   pgtype.Timestamptz{Time: time.Now(), Valid: true},
	})
}

func (m *integrationMockLevelsStore) addExercise(levelID int32, exerciseType string, difficulty, category int32, explanation string, dataJSON string) {
	exercise := db.Exercise{
		ID:           int32(len(m.exercises[levelID]) + 1),
		LevelID:      levelID,
		ExerciseType: exerciseType,
		Difficulty:   difficulty,
		Category:     category,
		Explanation:  explanation,
		DataJson:     []byte(dataJSON),
		SortOrder:    int32(len(m.exercises[levelID]) + 1),
		CreatedAt:    pgtype.Timestamptz{Time: time.Now(), Valid: true},
	}
	m.exercises[levelID] = append(m.exercises[levelID], exercise)
	m.counts[levelID] = int64(len(m.exercises[levelID]))
}

func newTestLevelsRouter(levelsStore handler.LevelsStore) *httptest.Server {
	r := router.New(router.Config{
		CORSAllowedOrigins: []string{"*"},
		LevelsStore:        levelsStore,
		JWTSecret:          "integration-test-secret-key",
		JWTExpiry:          24 * time.Hour,
	})
	return httptest.NewServer(r)
}

// --- ListLevels Integration Tests ---

func TestListLevelsIntegration_Success(t *testing.T) {
	store := newIntegrationMockLevelsStore()
	store.addLevel(1, 1, "Tri des Déchets")
	store.addLevel(2, 2, "Énergie Numérique")
	store.addLevel(3, 3, "RSE Fondamentaux")
	store.counts[1] = 3
	store.counts[2] = 4
	store.counts[3] = 4

	srv := newTestLevelsRouter(store)
	defer srv.Close()

	resp, err := http.Get(srv.URL + "/api/levels")
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

	var levelsResp handler.ListLevelsResponse
	if err := json.NewDecoder(resp.Body).Decode(&levelsResp); err != nil {
		t.Fatalf("failed to decode response: %v", err)
	}

	if len(levelsResp.Levels) != 3 {
		t.Fatalf("expected 3 levels, got %d", len(levelsResp.Levels))
	}

	if levelsResp.Levels[0].LevelID != 1 {
		t.Errorf("expected first level id 1, got %d", levelsResp.Levels[0].LevelID)
	}

	if levelsResp.Levels[0].Theme != "Tri des Déchets" {
		t.Errorf("expected theme 'Tri des Déchets', got %q", levelsResp.Levels[0].Theme)
	}

	if levelsResp.Levels[0].ExerciseCount != 3 {
		t.Errorf("expected exerciseCount 3, got %d", levelsResp.Levels[0].ExerciseCount)
	}
}

func TestListLevelsIntegration_Empty(t *testing.T) {
	store := newIntegrationMockLevelsStore()
	srv := newTestLevelsRouter(store)
	defer srv.Close()

	resp, err := http.Get(srv.URL + "/api/levels")
	if err != nil {
		t.Fatalf("failed to send request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		t.Errorf("expected status %d, got %d", http.StatusOK, resp.StatusCode)
	}

	var levelsResp handler.ListLevelsResponse
	json.NewDecoder(resp.Body).Decode(&levelsResp)

	if len(levelsResp.Levels) != 0 {
		t.Errorf("expected 0 levels, got %d", len(levelsResp.Levels))
	}
}

func TestListLevelsIntegration_MethodNotAllowed(t *testing.T) {
	store := newIntegrationMockLevelsStore()
	srv := newTestLevelsRouter(store)
	defer srv.Close()

	resp, err := http.Post(srv.URL+"/api/levels", "application/json", nil)
	if err != nil {
		t.Fatalf("failed to send request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusMethodNotAllowed {
		t.Errorf("expected status %d, got %d", http.StatusMethodNotAllowed, resp.StatusCode)
	}
}

// --- GetLevelExercises Integration Tests ---

func TestGetLevelExercisesIntegration_Success(t *testing.T) {
	store := newIntegrationMockLevelsStore()
	store.addLevel(1, 1, "Tri des Déchets")
	store.addExercise(1, "quiz", 0, 0,
		"Le verre se recycle à l'infini !",
		`{"questionText": "Dans quelle poubelle va la bouteille en verre ?", "options": ["Verte", "Jaune", "Bleue"], "correctOptionIndex": 0}`,
	)
	store.addExercise(1, "trueFalse", 1, 0,
		"Le gras souille les fibres du carton.",
		`{"statement": "Le carton à pizza gras peut être recyclé.", "isTrue": false}`,
	)

	srv := newTestLevelsRouter(store)
	defer srv.Close()

	resp, err := http.Get(srv.URL + "/api/levels/1/exercises")
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

	var levelResp handler.LevelWithExercisesResponse
	if err := json.NewDecoder(resp.Body).Decode(&levelResp); err != nil {
		t.Fatalf("failed to decode response: %v", err)
	}

	if levelResp.LevelID != 1 {
		t.Errorf("expected levelId 1, got %d", levelResp.LevelID)
	}

	if levelResp.Theme != "Tri des Déchets" {
		t.Errorf("expected theme 'Tri des Déchets', got %q", levelResp.Theme)
	}

	if len(levelResp.Exercises) != 2 {
		t.Fatalf("expected 2 exercises, got %d", len(levelResp.Exercises))
	}

	// Verify first exercise (quiz)
	ex1 := levelResp.Exercises[0]
	if ex1["exerciseType"] != "quiz" {
		t.Errorf("expected exerciseType 'quiz', got %v", ex1["exerciseType"])
	}
	if ex1["questionText"] != "Dans quelle poubelle va la bouteille en verre ?" {
		t.Errorf("unexpected questionText: %v", ex1["questionText"])
	}

	// Verify second exercise (trueFalse)
	ex2 := levelResp.Exercises[1]
	if ex2["exerciseType"] != "trueFalse" {
		t.Errorf("expected exerciseType 'trueFalse', got %v", ex2["exerciseType"])
	}
	if ex2["isTrue"] != false {
		t.Errorf("expected isTrue false, got %v", ex2["isTrue"])
	}
}

func TestGetLevelExercisesIntegration_NotFound(t *testing.T) {
	store := newIntegrationMockLevelsStore()
	srv := newTestLevelsRouter(store)
	defer srv.Close()

	resp, err := http.Get(srv.URL + "/api/levels/999/exercises")
	if err != nil {
		t.Fatalf("failed to send request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusNotFound {
		t.Errorf("expected status %d, got %d", http.StatusNotFound, resp.StatusCode)
	}

	var errResp handler.ErrorResponse
	json.NewDecoder(resp.Body).Decode(&errResp)
	if errResp.Error != "level not found" {
		t.Errorf("expected 'level not found', got %q", errResp.Error)
	}
}

func TestGetLevelExercisesIntegration_InvalidID(t *testing.T) {
	store := newIntegrationMockLevelsStore()
	srv := newTestLevelsRouter(store)
	defer srv.Close()

	resp, err := http.Get(srv.URL + "/api/levels/abc/exercises")
	if err != nil {
		t.Fatalf("failed to send request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusBadRequest {
		t.Errorf("expected status %d, got %d", http.StatusBadRequest, resp.StatusCode)
	}
}

func TestGetLevelExercisesIntegration_AllExerciseTypes(t *testing.T) {
	store := newIntegrationMockLevelsStore()
	store.addLevel(1, 1, "Full Test Level")
	store.addExercise(1, "quiz", 0, 0, "Quiz exp",
		`{"questionText": "Q?", "options": ["A", "B"], "correctOptionIndex": 0}`)
	store.addExercise(1, "trueFalse", 1, 0, "TF exp",
		`{"statement": "Statement", "isTrue": true}`)
	store.addExercise(1, "fillInBlank", 2, 3, "FIB exp",
		`{"sentenceWithBlanks": "The {0} is {1}.", "correctAnswers": ["sky", "blue"], "wordOptions": ["sky", "blue", "red"], "caseSensitive": false}`)
	store.addExercise(1, "sorting", 1, 0, "Sort exp",
		`{"instruction": "Sort items", "categories": [{"categoryName": "A", "categoryColor": "#000"}], "items": [{"itemName": "Item1", "correctCategoryIndex": 0}]}`)
	store.addExercise(1, "matching", 2, 0, "Match exp",
		`{"instruction": "Match items", "leftColumnHeader": "Left", "rightColumnHeader": "Right", "pairs": [{"leftItem": "L1", "rightItem": "R1"}], "shuffleRightColumn": true}`)

	srv := newTestLevelsRouter(store)
	defer srv.Close()

	resp, err := http.Get(srv.URL + "/api/levels/1/exercises")
	if err != nil {
		t.Fatalf("failed to send request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		t.Errorf("expected status %d, got %d", http.StatusOK, resp.StatusCode)
	}

	var levelResp handler.LevelWithExercisesResponse
	json.NewDecoder(resp.Body).Decode(&levelResp)

	if len(levelResp.Exercises) != 5 {
		t.Fatalf("expected 5 exercises, got %d", len(levelResp.Exercises))
	}

	expectedTypes := []string{"quiz", "trueFalse", "fillInBlank", "sorting", "matching"}
	for i, ex := range levelResp.Exercises {
		if ex["exerciseType"] != expectedTypes[i] {
			t.Errorf("exercise %d: expected type %q, got %v", i, expectedTypes[i], ex["exerciseType"])
		}
		// All exercises should have common fields
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
