-- 0001_initial_schema.up.sql
-- Creates the core tables for GreenSkills API

CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) UNIQUE NOT NULL,
    username VARCHAR(100) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS levels (
    id SERIAL PRIMARY KEY,
    level_number INT UNIQUE NOT NULL,
    theme VARCHAR(255) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS exercises (
    id SERIAL PRIMARY KEY,
    level_id INT NOT NULL REFERENCES levels(id) ON DELETE CASCADE,
    exercise_type VARCHAR(50) NOT NULL,
    difficulty INT NOT NULL DEFAULT 0,
    category INT NOT NULL DEFAULT 0,
    explanation TEXT NOT NULL DEFAULT '',
    data_json JSONB NOT NULL DEFAULT '{}',
    sort_order INT NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_exercises_level_id ON exercises(level_id);
CREATE INDEX idx_exercises_exercise_type ON exercises(exercise_type);

CREATE TABLE IF NOT EXISTS user_progress (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    level_id INT NOT NULL REFERENCES levels(id) ON DELETE CASCADE,
    stars INT NOT NULL DEFAULT 0,
    best_time_seconds FLOAT,
    unlocked BOOLEAN NOT NULL DEFAULT false,
    completed_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    UNIQUE(user_id, level_id)
);

CREATE INDEX idx_user_progress_user_id ON user_progress(user_id);
