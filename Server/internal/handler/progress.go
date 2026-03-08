package handler

import (
	"context"
	"encoding/json"
	"fmt"
	"log/slog"
	"net/http"
	"strconv"
	"time"

	"github.com/go-chi/chi/v5"
	"github.com/jackc/pgx/v5/pgtype"

	"github.com/gauthier-se/GreenSkills-API/internal/db"
	"github.com/gauthier-se/GreenSkills-API/internal/middleware"
)

// ProgressStore defines the database operations required by progress handlers.
// This interface enables unit testing with mock implementations.
type ProgressStore interface {
	GetUserProgress(ctx context.Context, userID pgtype.UUID) ([]db.UserProgress, error)
	UpsertUserProgress(ctx context.Context, arg db.UpsertUserProgressParams) (db.UserProgress, error)
	GetUserStats(ctx context.Context, userID pgtype.UUID) (db.GetUserStatsRow, error)
}

// ProgressHandler holds the dependencies for user progress endpoints.
type ProgressHandler struct {
	store ProgressStore
}

// NewProgressHandler creates a new ProgressHandler with the provided dependencies.
func NewProgressHandler(store ProgressStore) *ProgressHandler {
	return &ProgressHandler{store: store}
}

// --- Request / Response types ---

// SaveProgressRequest represents the request body for PUT /api/users/me/progress/{levelId}.
type SaveProgressRequest struct {
	Stars           int32    `json:"stars"`
	BestTimeSeconds *float64 `json:"bestTimeSeconds"`
	Unlocked        bool     `json:"unlocked"`
	Completed       bool     `json:"completed"`
}

// ProgressResponse represents a single level's progress in API responses.
type ProgressResponse struct {
	LevelID         int32    `json:"levelId"`
	Stars           int32    `json:"stars"`
	BestTimeSeconds *float64 `json:"bestTimeSeconds,omitempty"`
	Unlocked        bool     `json:"unlocked"`
	CompletedAt     *string  `json:"completedAt,omitempty"`
}

// ListProgressResponse represents the response for GET /api/users/me/progress.
type ListProgressResponse struct {
	Progress []ProgressResponse `json:"progress"`
}

// StatsResponse represents the response for GET /api/users/me/stats.
type StatsResponse struct {
	TotalStars      int32 `json:"totalStars"`
	LevelsCompleted int32 `json:"levelsCompleted"`
	LevelsStarted   int32 `json:"levelsStarted"`
}

// --- Handlers ---

// GetProgress handles GET /api/users/me/progress.
// Returns all level progress for the authenticated user.
func (h *ProgressHandler) GetProgress() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		userID, err := parseUUID(middleware.GetUserID(r.Context()))
		if err != nil {
			slog.Error("[ProgressHandler] Invalid user ID from token", "error", err)
			writeError(w, http.StatusUnauthorized, "invalid user identity")
			return
		}

		progress, err := h.store.GetUserProgress(r.Context(), userID)
		if err != nil {
			slog.Error("[ProgressHandler] Failed to get user progress", "error", err)
			writeError(w, http.StatusInternalServerError, "internal server error")
			return
		}

		items := make([]ProgressResponse, 0, len(progress))
		for _, p := range progress {
			items = append(items, toProgressResponse(p))
		}

		writeJSON(w, http.StatusOK, ListProgressResponse{Progress: items})
	}
}

// SaveProgress handles PUT /api/users/me/progress/{levelId}.
// Creates or updates progress for a specific level (upsert).
func (h *ProgressHandler) SaveProgress() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		userID, err := parseUUID(middleware.GetUserID(r.Context()))
		if err != nil {
			slog.Error("[ProgressHandler] Invalid user ID from token", "error", err)
			writeError(w, http.StatusUnauthorized, "invalid user identity")
			return
		}

		// Parse levelId from URL
		levelIDParam := chi.URLParam(r, "levelId")
		levelID, err := strconv.Atoi(levelIDParam)
		if err != nil || levelID < 1 {
			writeError(w, http.StatusBadRequest, "invalid level id")
			return
		}

		// Decode request body
		var req SaveProgressRequest
		if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
			writeError(w, http.StatusBadRequest, "invalid request body")
			return
		}

		// Validate stars range (0-3)
		if req.Stars < 0 || req.Stars > 3 {
			writeError(w, http.StatusBadRequest, "stars must be between 0 and 3")
			return
		}

		// Build upsert params
		params := db.UpsertUserProgressParams{
			UserID:   userID,
			LevelID:  int32(levelID),
			Stars:    req.Stars,
			Unlocked: req.Unlocked,
		}

		// Handle optional bestTimeSeconds
		if req.BestTimeSeconds != nil {
			params.BestTimeSeconds = pgtype.Float8{Float64: *req.BestTimeSeconds, Valid: true}
		}

		// Handle completed flag → set completedAt to now
		if req.Completed {
			params.CompletedAt = pgtype.Timestamptz{Time: time.Now(), Valid: true}
		}

		result, err := h.store.UpsertUserProgress(r.Context(), params)
		if err != nil {
			slog.Error("[ProgressHandler] Failed to upsert progress",
				"levelId", levelID,
				"error", err,
			)
			writeError(w, http.StatusInternalServerError, "internal server error")
			return
		}

		writeJSON(w, http.StatusOK, toProgressResponse(result))
	}
}

// GetStats handles GET /api/users/me/stats.
// Returns aggregate stats for the authenticated user.
func (h *ProgressHandler) GetStats() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		userID, err := parseUUID(middleware.GetUserID(r.Context()))
		if err != nil {
			slog.Error("[ProgressHandler] Invalid user ID from token", "error", err)
			writeError(w, http.StatusUnauthorized, "invalid user identity")
			return
		}

		stats, err := h.store.GetUserStats(r.Context(), userID)
		if err != nil {
			slog.Error("[ProgressHandler] Failed to get user stats", "error", err)
			writeError(w, http.StatusInternalServerError, "internal server error")
			return
		}

		writeJSON(w, http.StatusOK, StatsResponse{
			TotalStars:      stats.TotalStars,
			LevelsCompleted: stats.LevelsCompleted,
			LevelsStarted:   stats.LevelsStarted,
		})
	}
}

// --- Helpers ---

// toProgressResponse converts a db.UserProgress to a ProgressResponse.
func toProgressResponse(p db.UserProgress) ProgressResponse {
	resp := ProgressResponse{
		LevelID:  p.LevelID,
		Stars:    p.Stars,
		Unlocked: p.Unlocked,
	}

	if p.BestTimeSeconds.Valid {
		t := p.BestTimeSeconds.Float64
		resp.BestTimeSeconds = &t
	}

	if p.CompletedAt.Valid {
		s := p.CompletedAt.Time.Format(time.RFC3339)
		resp.CompletedAt = &s
	}

	return resp
}

// parseUUID converts a UUID string to a pgtype.UUID.
func parseUUID(s string) (pgtype.UUID, error) {
	var uuid pgtype.UUID
	if err := uuid.Scan(s); err != nil {
		return pgtype.UUID{}, fmt.Errorf("invalid UUID %q: %w", s, err)
	}
	return uuid, nil
}
