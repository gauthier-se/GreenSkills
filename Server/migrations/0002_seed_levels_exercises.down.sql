-- 0002_seed_levels_exercises.down.sql
-- Removes all seeded data

DELETE FROM exercises WHERE level_id IN (SELECT id FROM levels WHERE level_number IN (1, 2, 3));
DELETE FROM levels WHERE level_number IN (1, 2, 3);
