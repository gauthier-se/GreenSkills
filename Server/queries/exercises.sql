-- name: ListExercisesByLevelID :many
-- Lists all exercises for a given level, ordered by sort_order.
SELECT id, level_id, exercise_type, difficulty, category, explanation, data_json, sort_order, created_at
FROM exercises
WHERE level_id = $1
ORDER BY sort_order;

-- name: GetExercise :one
-- Gets a single exercise by its ID.
SELECT id, level_id, exercise_type, difficulty, category, explanation, data_json, sort_order, created_at
FROM exercises
WHERE id = $1;

-- name: CreateExercise :one
-- Creates a new exercise and returns it.
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, data_json, sort_order)
VALUES ($1, $2, $3, $4, $5, $6, $7)
RETURNING id, level_id, exercise_type, difficulty, category, explanation, data_json, sort_order, created_at;

-- name: CountExercisesByLevelID :one
-- Counts the number of exercises in a given level.
SELECT count(*) FROM exercises WHERE level_id = $1;
