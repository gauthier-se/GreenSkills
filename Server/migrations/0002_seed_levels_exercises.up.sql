-- 0002_seed_levels_exercises.up.sql
-- Seeds the database with the 3 existing levels and 11 exercises from levels_data.json

-- Level 1: Tri des Déchets
INSERT INTO levels (level_number, theme) VALUES (1, 'Tri des Déchets');

-- Level 2: Énergie Numérique
INSERT INTO levels (level_number, theme) VALUES (2, 'Énergie Numérique');

-- Level 3: RSE Fondamentaux
INSERT INTO levels (level_number, theme) VALUES (3, 'RSE Fondamentaux');

-- === Level 1 exercises ===

-- Exercise 1.1: Quiz - Bouteille en verre
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 1),
    'quiz', 0, 0,
    'Le verre se recycle à l''infini !',
    1,
    '{"questionText": "Dans quelle poubelle va la bouteille en verre ?", "options": ["Verte", "Jaune", "Bleue"], "correctOptionIndex": 0}'
);

-- Exercise 1.2: TrueFalse - Carton à pizza
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 1),
    'trueFalse', 1, 0,
    'Le gras souille les fibres du carton, le rendant impropre au recyclage.',
    2,
    '{"statement": "Le carton à pizza gras peut être recyclé avec les autres cartons.", "isTrue": false}'
);

-- Exercise 1.3: Sorting - Tri des déchets
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 1),
    'sorting', 1, 0,
    'Les matières souillées ou certains types de verre ne sont pas recyclables.',
    3,
    '{"instruction": "Triez ces déchets dans la bonne poubelle", "categories": [{"categoryName": "Recyclable", "categoryColor": "#4CAF50"}, {"categoryName": "Non recyclable", "categoryColor": "#F44336"}], "items": [{"itemName": "Bouteille plastique", "correctCategoryIndex": 0}, {"itemName": "Mouchoir usagé", "correctCategoryIndex": 1}, {"itemName": "Journal", "correctCategoryIndex": 0}, {"itemName": "Vaisselle cassée", "correctCategoryIndex": 1}]}'
);

-- === Level 2 exercises ===

-- Exercise 2.1: Quiz - Appareil en veille
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 2),
    'quiz', 1, 3,
    'Une box internet consomme autant qu''un frigo sur une année !',
    1,
    '{"questionText": "Quel appareil consomme le plus en veille ?", "options": ["Box Internet", "Télévision", "Micro-ondes"], "correctOptionIndex": 0}'
);

-- Exercise 2.2: TrueFalse - Écran en veille
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 2),
    'trueFalse', 0, 3,
    'Un écran en veille peut consommer jusqu''à 50% de l''énergie d''un écran actif.',
    2,
    '{"statement": "Laisser un écran en veille ne consomme aucune énergie.", "isTrue": false}'
);

-- Exercise 2.3: FillInBlank - Green IT
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 2),
    'fillInBlank', 2, 3,
    'Le Green IT (ou informatique verte) vise à réduire l''empreinte écologique du numérique.',
    3,
    '{"sentenceWithBlanks": "Le {0} est la pratique d''utiliser les technologies numériques de manière responsable pour réduire leur impact {1}.", "correctAnswers": ["Green IT", "environnemental"], "wordOptions": ["Green IT", "Cloud Computing", "environnemental", "économique", "Big Data", "social"], "caseSensitive": false}'
);

-- Exercise 2.4: Matching - Impact écologique
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 2),
    'matching', 2, 3,
    'Chaque geste numérique a un impact mesurable sur l''environnement.',
    4,
    '{"instruction": "Reliez chaque action à son impact écologique", "leftColumnHeader": "Action", "rightColumnHeader": "Impact", "pairs": [{"leftItem": "Supprimer les vieux emails", "rightItem": "-10g CO2/email"}, {"leftItem": "Éteindre la box la nuit", "rightItem": "-65 kWh/an"}, {"leftItem": "Utiliser le WiFi vs 4G", "rightItem": "20x moins d''énergie"}, {"leftItem": "Compresser les images", "rightItem": "Réduction bande passante"}], "shuffleRightColumn": true}'
);

-- === Level 3 exercises ===

-- Exercise 3.1: FillInBlank - Piliers RSE
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 3),
    'fillInBlank', 1, 0,
    'Les trois piliers de la RSE forment le concept de développement durable.',
    1,
    '{"sentenceWithBlanks": "La RSE repose sur trois piliers : {0}, {1} et {2}.", "correctAnswers": ["économique", "social", "environnemental"], "wordOptions": ["économique", "social", "environnemental", "politique", "technologique", "culturel"], "caseSensitive": false}'
);

-- Exercise 3.2: TrueFalse - RSE obligatoire
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 3),
    'trueFalse', 1, 0,
    'La RSE est une démarche volontaire pour toutes les entreprises, quelle que soit leur taille.',
    2,
    '{"statement": "La RSE est obligatoire uniquement pour les grandes entreprises.", "isTrue": false}'
);

-- Exercise 3.3: Quiz - Acronyme RSE
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 3),
    'quiz', 0, 0,
    'La RSE désigne l''intégration volontaire des préoccupations sociales et environnementales par les entreprises.',
    3,
    '{"questionText": "Que signifie l''acronyme RSE ?", "options": ["Responsabilité Sociétale des Entreprises", "Règlement Social Européen", "Rapport Stratégique Environnemental"], "correctOptionIndex": 0}'
);

-- Exercise 3.4: Matching - Piliers et domaines
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 3),
    'matching', 1, 0,
    'Chaque pilier de la RSE englobe des actions concrètes pour l''entreprise.',
    4,
    '{"instruction": "Associez chaque pilier RSE à son domaine d''action", "leftColumnHeader": "Pilier", "rightColumnHeader": "Exemple d''action", "pairs": [{"leftItem": "Environnemental", "rightItem": "Réduire les émissions CO2"}, {"leftItem": "Social", "rightItem": "Améliorer les conditions de travail"}, {"leftItem": "Économique", "rightItem": "Pratiques d''achats responsables"}], "shuffleRightColumn": true}'
);
