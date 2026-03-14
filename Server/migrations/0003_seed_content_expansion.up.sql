-- 0003_seed_content_expansion.up.sql
-- Expands content from 3 levels / 11 exercises to 10 levels / 53 exercises
-- Adds levels 4-10 and their exercises, plus 3 new exercises for level 4 (already in client JSON)

-- === New Levels ===

INSERT INTO levels (level_number, theme) VALUES (4, 'Mobilité Durable');
INSERT INTO levels (level_number, theme) VALUES (5, 'Sobriété Numérique');
INSERT INTO levels (level_number, theme) VALUES (6, 'RSE en Entreprise');
INSERT INTO levels (level_number, theme) VALUES (7, 'Tri Avancé');
INSERT INTO levels (level_number, theme) VALUES (8, 'Mobilité Avancée');
INSERT INTO levels (level_number, theme) VALUES (9, 'Numérique Responsable');
INSERT INTO levels (level_number, theme) VALUES (10, 'RSE en Action');

-- =============================================
-- Level 4: Mobilité Durable (6 exercises)
-- =============================================

-- Exercise 4.1: Quiz - Mode de transport le moins polluant
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 4),
    'quiz', 0, 1,
    'Le vélo n''émet aucun CO2 direct et très peu à la fabrication comparé aux autres modes.',
    1,
    '{"questionText": "Quel mode de transport émet le moins de CO2 par km ?", "options": ["Le vélo", "La voiture électrique", "Le bus"], "correctOptionIndex": 0}'
);

-- Exercise 4.2: TrueFalse - Covoiturage
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 4),
    'trueFalse', 0, 1,
    'En partageant un trajet à deux, chaque passager divise effectivement l''empreinte par deux.',
    2,
    '{"statement": "Le covoiturage divise par deux l''empreinte carbone d''un trajet en voiture.", "isTrue": true}'
);

-- Exercise 4.3: Sorting - Impact écologique des transports
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 4),
    'sorting', 1, 1,
    'Les transports en commun et la mobilité douce ont un impact bien moindre que les véhicules individuels thermiques.',
    3,
    '{"instruction": "Classez ces modes de transport par impact écologique", "categories": [{"categoryName": "Faible impact", "categoryColor": "#4CAF50"}, {"categoryName": "Fort impact", "categoryColor": "#F44336"}], "items": [{"itemName": "Marche à pied", "correctCategoryIndex": 0}, {"itemName": "Avion court-courrier", "correctCategoryIndex": 1}, {"itemName": "Tramway", "correctCategoryIndex": 0}, {"itemName": "SUV diesel", "correctCategoryIndex": 1}]}'
);

-- Exercise 4.4: FillInBlank - Covoiturage définition
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 4),
    'fillInBlank', 1, 1,
    'Le covoiturage permet de réduire le nombre de voitures sur la route et de diviser les coûts et les émissions.',
    4,
    '{"sentenceWithBlanks": "Le {0} consiste à partager un véhicule entre plusieurs {1} pour un même trajet.", "correctAnswers": ["covoiturage", "passagers"], "wordOptions": ["covoiturage", "autopartage", "passagers", "conducteurs", "véhicules", "trajets"], "caseSensitive": false}'
);

-- Exercise 4.5: Matching - Émissions CO2 par transport
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 4),
    'matching', 2, 1,
    'L''avion est le mode de transport le plus polluant par km, tandis que le train est le moins émetteur parmi les transports motorisés.',
    5,
    '{"instruction": "Associez chaque mode de transport à ses émissions moyennes de CO2 par km", "leftColumnHeader": "Transport", "rightColumnHeader": "Émissions CO2/km", "pairs": [{"leftItem": "Avion", "rightItem": "285 g CO2/km"}, {"leftItem": "Voiture thermique", "rightItem": "128 g CO2/km"}, {"leftItem": "TGV", "rightItem": "2,4 g CO2/km"}, {"leftItem": "Bus urbain", "rightItem": "68 g CO2/km"}], "shuffleRightColumn": true}'
);

-- Exercise 4.6: Quiz - Vélo vs voiture en ville
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 4),
    'quiz', 1, 1,
    'En milieu urbain, pour les trajets de moins de 5 km, le vélo est souvent plus rapide que la voiture en comptant le stationnement.',
    6,
    '{"questionText": "En dessous de quelle distance le vélo est-il généralement plus rapide que la voiture en ville ?", "options": ["5 km", "15 km", "1 km"], "correctOptionIndex": 0}'
);

-- =============================================
-- Level 5: Sobriété Numérique (6 exercises)
-- =============================================

-- Exercise 5.1: Quiz - Premier réflexe empreinte numérique
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 5),
    'quiz', 0, 3,
    'La fabrication représente 80% de l''empreinte carbone d''un appareil numérique. Garder son équipement plus longtemps est le geste le plus impactant.',
    1,
    '{"questionText": "Quel est le premier réflexe pour réduire son empreinte numérique ?", "options": ["Allonger la durée de vie de ses appareils", "Supprimer ses emails", "Utiliser un moteur de recherche vert"], "correctOptionIndex": 0}'
);

-- Exercise 5.2: TrueFalse - Fabrication smartphone vs usage
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 5),
    'trueFalse', 0, 3,
    'La fabrication d''un smartphone émet environ 40 kg de CO2, contre 2 à 4 kg par an d''utilisation.',
    2,
    '{"statement": "La fabrication d''un smartphone génère plus de CO2 que 10 ans de son utilisation.", "isTrue": true}'
);

-- Exercise 5.3: Sorting - Pratiques sobres vs non sobres
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 5),
    'sorting', 1, 3,
    'La sobriété numérique consiste à réduire notre consommation de ressources numériques au strict nécessaire.',
    3,
    '{"instruction": "Classez ces pratiques numériques", "categories": [{"categoryName": "Sobre", "categoryColor": "#4CAF50"}, {"categoryName": "Non sobre", "categoryColor": "#F44336"}], "items": [{"itemName": "Réparer son téléphone", "correctCategoryIndex": 0}, {"itemName": "Changer de smartphone chaque année", "correctCategoryIndex": 1}, {"itemName": "Utiliser le WiFi plutôt que la 4G", "correctCategoryIndex": 0}, {"itemName": "Laisser tourner les apps en arrière-plan", "correctCategoryIndex": 1}, {"itemName": "Désactiver la lecture automatique des vidéos", "correctCategoryIndex": 0}, {"itemName": "Stocker tous ses fichiers en double dans le cloud", "correctCategoryIndex": 1}]}'
);

-- Exercise 5.4: FillInBlank - Numérique et émissions mondiales
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 5),
    'fillInBlank', 1, 3,
    'Le numérique représente environ 4% des émissions mondiales de GES, soit plus que le transport aérien civil.',
    4,
    '{"sentenceWithBlanks": "Le secteur {0} est responsable d''environ {1}% des émissions mondiales de gaz à effet de serre.", "correctAnswers": ["numérique", "4"], "wordOptions": ["numérique", "automobile", "4", "15", "1", "textile"], "caseSensitive": false}'
);

-- Exercise 5.5: Matching - Habitudes numériques et CO2
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 5),
    'matching', 2, 3,
    'Le streaming vidéo est l''activité numérique la plus énergivore, représentant plus de 60% du trafic Internet mondial.',
    5,
    '{"instruction": "Associez chaque habitude numérique à son équivalent CO2 annuel", "leftColumnHeader": "Habitude", "rightColumnHeader": "Équivalent CO2/an", "pairs": [{"leftItem": "1h de streaming vidéo/jour", "rightItem": "36 kg CO2"}, {"leftItem": "50 emails/jour", "rightItem": "18 kg CO2"}, {"leftItem": "Stocker 1 Go dans le cloud", "rightItem": "7 kg CO2"}, {"leftItem": "Recherches web (30/jour)", "rightItem": "5 kg CO2"}], "shuffleRightColumn": true}'
);

-- Exercise 5.6: Quiz - Eau et smartphone
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 5),
    'quiz', 1, 0,
    'La fabrication d''un smartphone nécessite environ 12 000 litres d''eau, principalement pour l''extraction des minerais et la production des composants.',
    6,
    '{"questionText": "Combien de litres d''eau faut-il pour fabriquer un seul smartphone ?", "options": ["Environ 12 000 litres", "Environ 500 litres", "Environ 100 litres"], "correctOptionIndex": 0}'
);

-- =============================================
-- Level 6: RSE en Entreprise (6 exercises)
-- =============================================

-- Exercise 6.1: Quiz - Référentiel RSE
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 6),
    'quiz', 0, 2,
    'L''ISO 26000 est la norme internationale qui fournit des lignes directrices sur la responsabilité sociétale des organisations.',
    1,
    '{"questionText": "Quel référentiel international est la référence en matière de RSE ?", "options": ["ISO 26000", "ISO 9001", "ISO 14001"], "correctOptionIndex": 0}'
);

-- Exercise 6.2: TrueFalse - CSRD obligatoire
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 6),
    'trueFalse', 1, 2,
    'La directive CSRD (Corporate Sustainability Reporting Directive) impose un reporting de durabilité aux entreprises de plus de 250 salariés depuis 2025.',
    2,
    '{"statement": "Le reporting extra-financier (CSRD) est obligatoire pour les entreprises européennes de plus de 250 salariés.", "isTrue": true}'
);

-- Exercise 6.3: FillInBlank - ISO 26000
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 6),
    'fillInBlank', 1, 2,
    'L''ISO 26000, publiée en 2010, est le référentiel international de la RSE. Contrairement à d''autres normes ISO, elle n''est pas certifiable.',
    3,
    '{"sentenceWithBlanks": "La norme {0} définit les lignes directrices relatives à la {1} sociétale des organisations.", "correctAnswers": ["ISO 26000", "responsabilité"], "wordOptions": ["ISO 26000", "ISO 14001", "responsabilité", "performance", "ISO 9001", "conformité"], "caseSensitive": false}'
);

-- Exercise 6.4: Sorting - Parties prenantes
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 6),
    'sorting', 1, 2,
    'Les parties prenantes internes font partie de l''organisation, les externes sont impactées par ou impactent l''activité de l''entreprise.',
    4,
    '{"instruction": "Classez ces acteurs selon leur type de partie prenante", "categories": [{"categoryName": "Partie prenante interne", "categoryColor": "#2196F3"}, {"categoryName": "Partie prenante externe", "categoryColor": "#FF9800"}], "items": [{"itemName": "Salariés", "correctCategoryIndex": 0}, {"itemName": "Fournisseurs", "correctCategoryIndex": 1}, {"itemName": "Direction", "correctCategoryIndex": 0}, {"itemName": "Collectivités locales", "correctCategoryIndex": 1}, {"itemName": "Actionnaires", "correctCategoryIndex": 0}, {"itemName": "ONG", "correctCategoryIndex": 1}]}'
);

-- Exercise 6.5: Matching - Référentiels RSE
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 6),
    'matching', 2, 2,
    'Chaque référentiel a un rôle spécifique : l''ISO 26000 guide la stratégie, le GRI structure le reporting, la CSRD le rend obligatoire, et les ODD fixent les objectifs mondiaux.',
    5,
    '{"instruction": "Associez chaque référentiel RSE à sa description", "leftColumnHeader": "Référentiel", "rightColumnHeader": "Description", "pairs": [{"leftItem": "ISO 26000", "rightItem": "Lignes directrices RSE internationales"}, {"leftItem": "GRI", "rightItem": "Standard de reporting développement durable"}, {"leftItem": "CSRD", "rightItem": "Directive européenne de reporting durabilité"}, {"leftItem": "ODD", "rightItem": "17 objectifs de développement durable de l''ONU"}], "shuffleRightColumn": true}'
);

-- Exercise 6.6: Quiz - Bilan carbone
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 6),
    'quiz', 1, 0,
    'Le bilan carbone quantifie les émissions directes et indirectes de GES d''une organisation pour identifier les leviers de réduction.',
    6,
    '{"questionText": "Quel est l''objectif principal d''un bilan carbone en entreprise ?", "options": ["Mesurer les émissions de gaz à effet de serre", "Calculer le chiffre d''affaires vert", "Évaluer la satisfaction des employés"], "correctOptionIndex": 0}'
);

-- =============================================
-- Level 7: Tri Avancé (6 exercises)
-- =============================================

-- Exercise 7.1: Quiz - Taux de recyclage du verre
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 7),
    'quiz', 1, 0,
    'La France recycle environ 85% de son verre d''emballage, ce qui en fait l''un des matériaux les mieux recyclés.',
    1,
    '{"questionText": "Quel est le taux de recyclage du verre en France ?", "options": ["Environ 85%", "Environ 50%", "Environ 30%"], "correctOptionIndex": 0}'
);

-- Exercise 7.2: TrueFalse - Bioplastiques
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 7),
    'trueFalse', 1, 0,
    'La plupart des bioplastiques nécessitent des conditions industrielles (60°C+) pour se décomposer. Ils ne se compostent pas dans un composteur de jardin.',
    2,
    '{"statement": "Les bioplastiques sont toujours compostables dans un composteur domestique.", "isTrue": false}'
);

-- Exercise 7.3: Sorting - 3 catégories de déchets
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 7),
    'sorting', 2, 0,
    'Le compostage valorise les déchets organiques, le recyclage transforme les matériaux, et les déchets ultimes vont en enfouissement ou incinération.',
    3,
    '{"instruction": "Classez ces déchets selon leur filière de traitement", "categories": [{"categoryName": "Compostable", "categoryColor": "#795548"}, {"categoryName": "Recyclable", "categoryColor": "#4CAF50"}, {"categoryName": "Déchet ultime", "categoryColor": "#9E9E9E"}], "items": [{"itemName": "Épluchures de légumes", "correctCategoryIndex": 0}, {"itemName": "Canette aluminium", "correctCategoryIndex": 1}, {"itemName": "Couche bébé usagée", "correctCategoryIndex": 2}, {"itemName": "Marc de café", "correctCategoryIndex": 0}, {"itemName": "Bouteille en plastique PET", "correctCategoryIndex": 1}, {"itemName": "Mégot de cigarette", "correctCategoryIndex": 2}]}'
);

-- Exercise 7.4: FillInBlank - Écoconception
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 7),
    'fillInBlank', 2, 0,
    'L''écoconception intègre les critères environnementaux dès la phase de design pour minimiser l''impact tout au long du cycle de vie.',
    4,
    '{"sentenceWithBlanks": "L''{0} est une approche qui vise à éliminer les {1} dès le stade de la conception d''un produit.", "correctAnswers": ["écoconception", "déchets"], "wordOptions": ["écoconception", "recyclage", "déchets", "emballages", "suremballage", "économie circulaire"], "caseSensitive": false}'
);

-- Exercise 7.5: Matching - Logos de recyclage
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 7),
    'matching', 1, 0,
    'Attention : le Point Vert ne signifie PAS que le produit est recyclable, mais que le fabricant contribue financièrement au recyclage.',
    5,
    '{"instruction": "Associez chaque logo de recyclage à son matériau", "leftColumnHeader": "Symbole", "rightColumnHeader": "Matériau", "pairs": [{"leftItem": "Triangle avec chiffre 1 (PET)", "rightItem": "Bouteilles plastiques transparentes"}, {"leftItem": "Anneau de Möbius", "rightItem": "Matériau recyclable (générique)"}, {"leftItem": "Point vert", "rightItem": "Contribution au recyclage (pas recyclable)"}, {"leftItem": "Triman", "rightItem": "Produit soumis à une consigne de tri"}], "shuffleRightColumn": true}'
);

-- Exercise 7.6: Quiz - Recyclage aluminium
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 7),
    'quiz', 2, 0,
    'L''aluminium est recyclable à l''infini sans perdre ses propriétés. Recycler une canette économise 95% de l''énergie nécessaire à sa fabrication.',
    6,
    '{"questionText": "Combien de fois peut-on recycler l''aluminium sans perte de qualité ?", "options": ["À l''infini", "5 fois", "10 fois"], "correctOptionIndex": 0}'
);

-- =============================================
-- Level 8: Mobilité Avancée (6 exercises)
-- =============================================

-- Exercise 8.1: Quiz - Part du transport dans les émissions
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 8),
    'quiz', 1, 1,
    'Le transport représente environ 30% des émissions de CO2 en France, dont plus de la moitié provient des voitures particulières.',
    1,
    '{"questionText": "Quelle est la part du transport dans les émissions de CO2 en France ?", "options": ["Environ 30%", "Environ 10%", "Environ 50%"], "correctOptionIndex": 0}'
);

-- Exercise 8.2: TrueFalse - Voiture électrique zéro émission
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 8),
    'trueFalse', 1, 1,
    'La fabrication de la batterie et la production d''électricité génèrent des émissions. Une voiture électrique émet 2 à 3 fois moins qu''une thermique sur son cycle de vie, mais pas zéro.',
    2,
    '{"statement": "Une voiture électrique a zéro émission sur l''ensemble de son cycle de vie.", "isTrue": false}'
);

-- Exercise 8.3: FillInBlank - ZFE
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 8),
    'fillInBlank', 2, 1,
    'Les ZFE-m (Zones à Faibles Émissions mobilité) visent à améliorer la qualité de l''air en restreignant l''accès aux véhicules les plus polluants.',
    3,
    '{"sentenceWithBlanks": "Les {0} à faibles émissions (ZFE) interdisent progressivement les véhicules les plus {1} dans les centres-villes.", "correctAnswers": ["zones", "polluants"], "wordOptions": ["zones", "routes", "polluants", "anciens", "voies", "lourds"], "caseSensitive": false}'
);

-- Exercise 8.4: Sorting - Mobilité individuelle vs partagée
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 8),
    'sorting', 1, 1,
    'La mobilité partagée et collective optimise le taux de remplissage des véhicules et réduit le nombre total de véhicules en circulation.',
    4,
    '{"instruction": "Classez ces solutions de mobilité", "categories": [{"categoryName": "Mobilité individuelle", "categoryColor": "#FF9800"}, {"categoryName": "Mobilité partagée/collective", "categoryColor": "#4CAF50"}], "items": [{"itemName": "Voiture personnelle", "correctCategoryIndex": 0}, {"itemName": "Autopartage", "correctCategoryIndex": 1}, {"itemName": "Trottinette personnelle", "correctCategoryIndex": 0}, {"itemName": "Covoiturage", "correctCategoryIndex": 1}, {"itemName": "Métro", "correctCategoryIndex": 1}, {"itemName": "Moto", "correctCategoryIndex": 0}]}'
);

-- Exercise 8.5: Matching - Politiques de mobilité
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 8),
    'matching', 2, 1,
    'Ces politiques publiques encouragent la transition vers une mobilité moins carbonée et plus sûre.',
    5,
    '{"instruction": "Associez chaque politique de mobilité à son objectif", "leftColumnHeader": "Politique", "rightColumnHeader": "Objectif", "pairs": [{"leftItem": "Forfait Mobilités Durables", "rightItem": "Indemniser les trajets domicile-travail verts"}, {"leftItem": "Prime à la conversion", "rightItem": "Remplacer un véhicule polluant"}, {"leftItem": "Plan de Mobilité Entreprise", "rightItem": "Optimiser les déplacements des salariés"}, {"leftItem": "Zones 30 km/h", "rightItem": "Sécuriser et apaiser la circulation"}], "shuffleRightColumn": true}'
);

-- Exercise 8.6: Quiz - Coût annuel voiture
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 8),
    'quiz', 2, 3,
    'Le coût complet d''une voiture est souvent sous-estimé. L''Automobile Club Association estime le budget moyen à environ 6 000 euros par an.',
    6,
    '{"questionText": "Quel est le coût moyen annuel d''une voiture individuelle en France (achat amorti, carburant, assurance, entretien) ?", "options": ["Environ 6 000 euros", "Environ 2 000 euros", "Environ 12 000 euros"], "correctOptionIndex": 0}'
);

-- =============================================
-- Level 9: Numérique Responsable (6 exercises)
-- =============================================

-- Exercise 9.1: Quiz - Fabrication vs usage
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 9),
    'quiz', 2, 3,
    'La fabrication des terminaux (smartphones, ordinateurs, tablettes) représente environ 80% de l''empreinte carbone du numérique, contre 20% pour l''usage.',
    1,
    '{"questionText": "Quelle part de l''empreinte carbone du numérique est liée à la fabrication des terminaux ?", "options": ["Environ 80%", "Environ 40%", "Environ 20%"], "correctOptionIndex": 0}'
);

-- Exercise 9.2: TrueFalse - Cloud et empreinte carbone
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 9),
    'trueFalse', 1, 3,
    'Le cloud peut réduire l''empreinte si les serveurs mutualisés sont mieux optimisés, mais l''effet rebond (plus de stockage, plus de services) peut annuler les gains.',
    2,
    '{"statement": "Migrer vers le cloud réduit systématiquement l''empreinte carbone d''une entreprise.", "isTrue": false}'
);

-- Exercise 9.3: FillInBlank - ACV
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 9),
    'fillInBlank', 2, 3,
    'L''Analyse du Cycle de Vie (ACV) évalue l''impact environnemental à chaque étape : extraction, fabrication, transport, usage et fin de vie.',
    3,
    '{"sentenceWithBlanks": "L''{0} mesure l''impact environnemental d''un produit de sa fabrication à sa {1} de vie.", "correctAnswers": ["ACV", "fin"], "wordOptions": ["ACV", "RSE", "fin", "durée", "bilan carbone", "qualité"], "caseSensitive": false}'
);

-- Exercise 9.4: Sorting - Intensité énergétique des activités numériques
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 9),
    'sorting', 2, 3,
    'Le streaming vidéo et la visioconférence sont parmi les activités les plus énergivores car elles nécessitent un transfert massif de données.',
    4,
    '{"instruction": "Classez ces activités numériques par intensité énergétique", "categories": [{"categoryName": "Faible consommation", "categoryColor": "#4CAF50"}, {"categoryName": "Forte consommation", "categoryColor": "#F44336"}], "items": [{"itemName": "Envoyer un SMS", "correctCategoryIndex": 0}, {"itemName": "Streaming vidéo 4K", "correctCategoryIndex": 1}, {"itemName": "Lire un article de blog", "correctCategoryIndex": 0}, {"itemName": "Visioconférence avec caméra HD", "correctCategoryIndex": 1}, {"itemName": "Envoyer un email avec pièce jointe de 10 Mo", "correctCategoryIndex": 1}, {"itemName": "Consulter l''heure sur son téléphone", "correctCategoryIndex": 0}]}'
);

-- Exercise 9.5: Matching - Indicateurs data center
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 9),
    'matching', 2, 0,
    'Ces indicateurs permettent de mesurer et comparer la performance environnementale des centres de données. Un PUE idéal est proche de 1,0.',
    5,
    '{"instruction": "Associez chaque indicateur de data center à sa signification", "leftColumnHeader": "Indicateur", "rightColumnHeader": "Signification", "pairs": [{"leftItem": "PUE", "rightItem": "Efficacité énergétique globale"}, {"leftItem": "WUE", "rightItem": "Consommation d''eau pour le refroidissement"}, {"leftItem": "CUE", "rightItem": "Émissions carbone par kWh consommé"}, {"leftItem": "ERF", "rightItem": "Part d''énergie renouvelable utilisée"}], "shuffleRightColumn": true}'
);

-- Exercise 9.6: Quiz - Résolution streaming
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 9),
    'quiz', 1, 0,
    'Passer de la 4K au 480p divise par 10 la quantité de données transférées, réduisant significativement l''impact énergétique.',
    6,
    '{"questionText": "Quel réglage simple permet de réduire la consommation de données lors du streaming vidéo ?", "options": ["Baisser la résolution (ex : 480p)", "Fermer les autres applications", "Utiliser un écran plus petit"], "correctOptionIndex": 0}'
);

-- =============================================
-- Level 10: RSE en Action (6 exercises)
-- =============================================

-- Exercise 10.1: Quiz - CSRD
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 10),
    'quiz', 1, 2,
    'La CSRD est la directive européenne qui impose aux entreprises un reporting détaillé sur leur impact environnemental, social et de gouvernance.',
    1,
    '{"questionText": "Que signifie l''acronyme CSRD ?", "options": ["Corporate Sustainability Reporting Directive", "Corporate Social Responsibility Document", "Climate Strategy and Reporting Directive"], "correctOptionIndex": 0}'
);

-- Exercise 10.2: TrueFalse - 17 ODD
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 10),
    'trueFalse', 1, 2,
    'L''Agenda 2030 de l''ONU définit 17 ODD et 169 cibles pour un développement durable à l''échelle mondiale.',
    2,
    '{"statement": "Les Objectifs de Développement Durable (ODD) de l''ONU comportent 17 objectifs.", "isTrue": true}'
);

-- Exercise 10.3: FillInBlank - Taxonomie européenne
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 10),
    'fillInBlank', 2, 2,
    'La taxonomie verte européenne définit 6 objectifs : atténuation du changement climatique, adaptation, eau, économie circulaire, pollution, biodiversité.',
    3,
    '{"sentenceWithBlanks": "La taxonomie {0} classifie les activités économiques selon leur contribution à {1} objectifs environnementaux.", "correctAnswers": ["européenne", "6"], "wordOptions": ["européenne", "française", "6", "3", "17", "mondiale"], "caseSensitive": false}'
);

-- Exercise 10.4: Sorting - ODD par pilier (3 catégories)
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 10),
    'sorting', 1, 2,
    'Les 17 ODD couvrent les trois dimensions du développement durable. Chaque entreprise peut contribuer aux ODD pertinents pour son activité.',
    4,
    '{"instruction": "Classez ces ODD selon leur pilier principal", "categories": [{"categoryName": "Environnement", "categoryColor": "#4CAF50"}, {"categoryName": "Social", "categoryColor": "#2196F3"}, {"categoryName": "Économie", "categoryColor": "#FF9800"}], "items": [{"itemName": "ODD 13 - Action climatique", "correctCategoryIndex": 0}, {"itemName": "ODD 4 - Éducation de qualité", "correctCategoryIndex": 1}, {"itemName": "ODD 8 - Travail décent", "correctCategoryIndex": 2}, {"itemName": "ODD 14 - Vie aquatique", "correctCategoryIndex": 0}, {"itemName": "ODD 3 - Bonne santé", "correctCategoryIndex": 1}, {"itemName": "ODD 12 - Consommation responsable", "correctCategoryIndex": 2}]}'
);

-- Exercise 10.5: Matching - ODD numéros et intitulés
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 10),
    'matching', 2, 2,
    'Les ODD forment un cadre universel adopté par 193 pays en 2015 pour transformer le monde d''ici 2030.',
    5,
    '{"instruction": "Associez chaque ODD à son intitulé", "leftColumnHeader": "Numéro ODD", "rightColumnHeader": "Intitulé", "pairs": [{"leftItem": "ODD 7", "rightItem": "Énergie propre et d''un coût abordable"}, {"leftItem": "ODD 11", "rightItem": "Villes et communautés durables"}, {"leftItem": "ODD 13", "rightItem": "Mesures relatives à la lutte contre le changement climatique"}, {"leftItem": "ODD 15", "rightItem": "Vie terrestre"}], "shuffleRightColumn": true}'
);

-- Exercise 10.6: Quiz - Principe de précaution
INSERT INTO exercises (level_id, exercise_type, difficulty, category, explanation, sort_order, data_json)
VALUES (
    (SELECT id FROM levels WHERE level_number = 10),
    'quiz', 2, 1,
    'Le principe de précaution, inscrit dans la Constitution française, stipule que l''absence de certitude scientifique ne doit pas retarder les mesures de prévention.',
    6,
    '{"questionText": "Quel principe RSE impose de prendre des mesures préventives même en l''absence de certitude scientifique complète ?", "options": ["Le principe de précaution", "Le principe pollueur-payeur", "Le principe de subsidiarité"], "correctOptionIndex": 0}'
);
