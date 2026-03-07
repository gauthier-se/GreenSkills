-- name: GetUserByEmail :one
-- Gets a user by email address.
SELECT id, email, username, password_hash, created_at, updated_at
FROM users
WHERE email = $1;

-- name: GetUserByID :one
-- Gets a user by ID.
SELECT id, email, username, password_hash, created_at, updated_at
FROM users
WHERE id = $1;

-- name: CreateUser :one
-- Creates a new user and returns it.
INSERT INTO users (email, username, password_hash)
VALUES ($1, $2, $3)
RETURNING id, email, username, password_hash, created_at, updated_at;

-- name: UserExistsByEmail :one
-- Checks if a user with the given email exists.
SELECT EXISTS(SELECT 1 FROM users WHERE email = $1);

-- name: UserExistsByUsername :one
-- Checks if a user with the given username exists.
SELECT EXISTS(SELECT 1 FROM users WHERE username = $1);
