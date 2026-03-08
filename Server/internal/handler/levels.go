package handler

import (
	"context"
	"encoding/json"
	"log/slog"
	"net/http"
	"strconv"

	"github.com/go-chi/chi/v5"
	"github.com/jackc/pgx/v5"

	"github.com/gauthier-se/GreenSkills-API/internal/db"
)

// LevelsStore defines the database operations required by level handlers.
// This interface enables unit testing with mock implementations.
type LevelsStore interface {
	ListLevels(ctx context.Context) ([]db.Level, error)
	GetLevel(ctx context.Context, levelNumber int32) (db.Level, error)
	ListExercisesByLevelID(ctx context.Context, levelID int32) ([]db.Exercise, error)
	CountExercisesByLevelID(ctx context.Context, levelID int32) (int64, error)
}

// LevelsHandler holds the dependencies for level endpoints.
type LevelsHandler struct {
	store LevelsStore
}

// NewLevelsHandler creates a new LevelsHandler with the provided dependencies.
func NewLevelsHandler(store LevelsStore) *LevelsHandler {
	return &LevelsHandler{store: store}
}

// --- Response types ---

// LevelSummary represents a level in the list response with metadata.
type LevelSummary struct {
	LevelID       int32  `json:"levelId"`
	Theme         string `json:"theme"`
	ExerciseCount int64  `json:"exerciseCount"`
}

// ListLevelsResponse represents the response for GET /api/levels.
type ListLevelsResponse struct {
	Levels []LevelSummary `json:"levels"`
}

// LevelWithExercisesResponse represents the response for GET /api/levels/{id}/exercises.
// Matches the Unity client's LevelWithExercisesDto structure.
type LevelWithExercisesResponse struct {
	LevelID   int32                    `json:"levelId"`
	Theme     string                   `json:"theme"`
	Exercises []map[string]interface{} `json:"exercises"`
}

// --- Handlers ---

// ListLevels handles GET /api/levels.
// Returns all levels with metadata (levelId, theme, exerciseCount).
func (h *LevelsHandler) ListLevels() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		ctx := r.Context()

		levels, err := h.store.ListLevels(ctx)
		if err != nil {
			slog.Error("[LevelsHandler] Failed to list levels", "error", err)
			writeError(w, http.StatusInternalServerError, "internal server error")
			return
		}

		summaries := make([]LevelSummary, 0, len(levels))
		for _, level := range levels {
			count, err := h.store.CountExercisesByLevelID(ctx, level.ID)
			if err != nil {
				slog.Error("[LevelsHandler] Failed to count exercises",
					"levelId", level.LevelNumber,
					"error", err,
				)
				writeError(w, http.StatusInternalServerError, "internal server error")
				return
			}

			summaries = append(summaries, LevelSummary{
				LevelID:       level.LevelNumber,
				Theme:         level.Theme,
				ExerciseCount: count,
			})
		}

		writeJSON(w, http.StatusOK, ListLevelsResponse{Levels: summaries})
	}
}

// GetLevelExercises handles GET /api/levels/{id}/exercises.
// Returns a level with all its exercises in the format expected by the Unity client.
func (h *LevelsHandler) GetLevelExercises() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		idParam := chi.URLParam(r, "id")
		levelNumber, err := strconv.Atoi(idParam)
		if err != nil {
			writeError(w, http.StatusBadRequest, "invalid level id")
			return
		}

		ctx := r.Context()

		// Get the level by level_number
		level, err := h.store.GetLevel(ctx, int32(levelNumber))
		if err != nil {
			if err == pgx.ErrNoRows {
				writeError(w, http.StatusNotFound, "level not found")
				return
			}
			slog.Error("[LevelsHandler] Failed to get level",
				"levelNumber", levelNumber,
				"error", err,
			)
			writeError(w, http.StatusInternalServerError, "internal server error")
			return
		}

		// Get all exercises for this level
		exercises, err := h.store.ListExercisesByLevelID(ctx, level.ID)
		if err != nil {
			slog.Error("[LevelsHandler] Failed to list exercises",
				"levelId", level.ID,
				"error", err,
			)
			writeError(w, http.StatusInternalServerError, "internal server error")
			return
		}

		// Convert exercises to the flat DTO format expected by the Unity client
		exerciseDtos := make([]map[string]interface{}, 0, len(exercises))
		for _, exercise := range exercises {
			dto, err := buildExerciseDto(exercise)
			if err != nil {
				slog.Error("[LevelsHandler] Failed to build exercise DTO",
					"exerciseId", exercise.ID,
					"error", err,
				)
				writeError(w, http.StatusInternalServerError, "internal server error")
				return
			}
			exerciseDtos = append(exerciseDtos, dto)
		}

		writeJSON(w, http.StatusOK, LevelWithExercisesResponse{
			LevelID:   level.LevelNumber,
			Theme:     level.Theme,
			Exercises: exerciseDtos,
		})
	}
}

// buildExerciseDto merges the top-level exercise columns with the data_json fields
// into a single flat map matching the Unity client's GenericExerciseDto.
func buildExerciseDto(exercise db.Exercise) (map[string]interface{}, error) {
	// Unmarshal data_json into a map
	dto := make(map[string]interface{})
	if err := json.Unmarshal(exercise.DataJson, &dto); err != nil {
		return nil, err
	}

	// Add the common top-level fields
	dto["exerciseType"] = exercise.ExerciseType
	dto["difficulty"] = exercise.Difficulty
	dto["category"] = exercise.Category
	dto["explanation"] = exercise.Explanation

	return dto, nil
}
