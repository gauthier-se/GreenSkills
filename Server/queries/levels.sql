-- name: ListLevels :many
-- Lists all levels ordered by level number.
SELECT id, level_number, theme, created_at
FROM levels
ORDER BY level_number;

-- name: GetLevel :one
-- Gets a single level by its level number.
SELECT id, level_number, theme, created_at
FROM levels
WHERE level_number = $1;

-- name: GetLevelByID :one
-- Gets a single level by its database ID.
SELECT id, level_number, theme, created_at
FROM levels
WHERE id = $1;

-- name: CreateLevel :one
-- Creates a new level and returns it.
INSERT INTO levels (level_number, theme)
VALUES ($1, $2)
RETURNING id, level_number, theme, created_at;
