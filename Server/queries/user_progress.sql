-- name: GetUserProgress :many
-- Gets all level progress for a user.
SELECT id, user_id, level_id, stars, best_time_seconds, unlocked, completed_at, created_at, updated_at
FROM user_progress
WHERE user_id = $1
ORDER BY level_id;

-- name: GetUserProgressForLevel :one
-- Gets progress for a specific user and level.
SELECT id, user_id, level_id, stars, best_time_seconds, unlocked, completed_at, created_at, updated_at
FROM user_progress
WHERE user_id = $1 AND level_id = $2;

-- name: UpsertUserProgress :one
-- Creates or updates progress for a user on a specific level.
INSERT INTO user_progress (user_id, level_id, stars, best_time_seconds, unlocked, completed_at)
VALUES ($1, $2, $3, $4, $5, $6)
ON CONFLICT (user_id, level_id)
DO UPDATE SET
    stars = GREATEST(user_progress.stars, EXCLUDED.stars),
    best_time_seconds = CASE
        WHEN user_progress.best_time_seconds IS NULL THEN EXCLUDED.best_time_seconds
        WHEN EXCLUDED.best_time_seconds IS NULL THEN user_progress.best_time_seconds
        ELSE LEAST(user_progress.best_time_seconds, EXCLUDED.best_time_seconds)
    END,
    unlocked = EXCLUDED.unlocked OR user_progress.unlocked,
    completed_at = COALESCE(user_progress.completed_at, EXCLUDED.completed_at),
    updated_at = now()
RETURNING id, user_id, level_id, stars, best_time_seconds, unlocked, completed_at, created_at, updated_at;

-- name: GetUserStats :one
-- Gets aggregate stats for a user.
SELECT
    COALESCE(SUM(stars), 0)::int AS total_stars,
    COUNT(CASE WHEN completed_at IS NOT NULL THEN 1 END)::int AS levels_completed,
    COUNT(*)::int AS levels_started
FROM user_progress
WHERE user_id = $1;
