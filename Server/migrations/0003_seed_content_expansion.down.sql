-- 0003_seed_content_expansion.down.sql
-- Removes levels 4-10 and all their exercises (CASCADE handles exercise deletion)

DELETE FROM exercises WHERE level_id IN (SELECT id FROM levels WHERE level_number IN (4, 5, 6, 7, 8, 9, 10));
DELETE FROM levels WHERE level_number IN (4, 5, 6, 7, 8, 9, 10);
