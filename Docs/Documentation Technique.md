# Documentation Technique — GreenSkills

**Projet :** GreenSkills | **Version :** 1.0 | **Date :** 17/03/2026

---

## Table des matières

1. [Vue d'ensemble de l'architecture](#1-vue-densemble-de-larchitecture)
2. [Architecture Client (Unity)](#2-architecture-client-unity)
3. [Architecture Serveur (Go)](#3-architecture-serveur-go)
4. [Référence API](#4-référence-api)
5. [Schéma de base de données](#5-schéma-de-base-de-données)
6. [Système de gamification](#6-système-de-gamification)
7. [Déploiement et infrastructure](#7-déploiement-et-infrastructure)
8. [Guide de développement](#8-guide-de-développement)
9. [CI/CD](#9-cicd)

---

## 1. Vue d'ensemble de l'architecture

GreenSkills repose sur une **architecture découplée** séparant le client mobile de la logique serveur :

```
┌──────────────────┐       HTTPS/JSON        ┌──────────────────┐       SQL        ┌──────────────────┐
│                  │  ◄──────────────────►    │                  │  ◄────────────►  │                  │
│   Client Mobile  │                          │    API REST      │                  │   PostgreSQL 17  │
│   Unity 6 (C#)   │                          │    Go 1.25       │                  │                  │
│                  │                          │                  │                  │                  │
└──────────────────┘                          └──────────────────┘                  └──────────────────┘
     Android / iOS                              Docker + Traefik                       Volume persistant
```

- **Client mobile** : Application Unity 6 ciblant Android et iOS, communiquant avec l'API via des requêtes HTTPS.
- **API REST** : Serveur Go exposant des endpoints JSON, gérant l'authentification JWT, les niveaux, les exercices et la progression.
- **Base de données** : PostgreSQL 17 stockant les utilisateurs, niveaux, exercices et progression.
- **Déploiement** : Conteneurs Docker orchestrés via Dokploy sur un VPS, avec Traefik pour le routage HTTPS et les certificats SSL.

---

## 2. Architecture Client (Unity)

### 2.1 Stack technique

| Composant | Version / Détail |
|-----------|-----------------|
| Moteur | Unity 6 (6000.3.3f1) |
| Langage | C# 9.0 |
| Framework | .NET Framework 4.7.1 |
| Render Pipeline | URP 2D (Universal Render Pipeline) |
| Cibles | Android (APK) et iOS |

### 2.2 Architecture 3 couches

Le client suit une architecture en trois couches distinctes :

```
┌─────────────────────────────────────────────────┐
│                  MANAGERS                         │
│         (Singletons, DontDestroyOnLoad)          │
│  GameManager · AuthManager · ExerciseManager     │
│  UIManager · GamificationManager · AudioManager  │
├─────────────────────────────────────────────────┤
│                CONTROLLERS                        │
│            (Logique UI / Exercices)               │
│  IExerciseController → BaseExerciseController    │
│  → Quiz · TrueFalse · FillInBlank · Sorting ·   │
│    Matching                                       │
├─────────────────────────────────────────────────┤
│                    DATA                           │
│       (ScriptableObjects, DTOs, Enums)           │
│  BaseExerciseData → 5 types concrets             │
│  ExerciseFactory · ExerciseDtos · AuthDtos       │
└─────────────────────────────────────────────────┘
```

### 2.3 Managers

Tous les Managers implémentent le pattern **Singleton** avec `DontDestroyOnLoad` pour persister entre les scènes.

| Manager | Responsabilité |
|---------|---------------|
| **GameManager** | Coordinateur central : chargement des niveaux, gestion des vies, validation des réponses, flux de jeu (exercice → feedback → suivant), navigation entre scènes |
| **AuthManager** | Authentification (login, register, logout), stockage du JWT dans PlayerPrefs, auto-login au démarrage. URL API : `https://greenskills-api.seyzeriat.com` |
| **ExerciseManager** | Chargement des niveaux depuis l'API REST ou des fichiers JSON locaux, conversion DTO → Data via ExerciseFactory, cache du nombre de niveaux |
| **GamificationManager** | Gestion XP, EcoCoins, streaks quotidiens, niveaux joueur. Événements : `OnXPGained`, `OnLevelUp`, `OnCoinsEarned`, `OnStreakUpdated` |
| **UIManager** | Mise à jour des éléments UI (barre de progression, vies, boutons), affichage des écrans de victoire et game over |
| **AudioManager** | Lecture de musique et effets sonores via AudioLibraryData, contrôle du volume, mute, pause automatique |
| **MainMenuManager** | Initialisation de la grille de sélection des niveaux, gestion des états de boutons (verrouillé/déverrouillé) |
| **BootManager** | Point d'entrée de l'application : vérification des managers, splash screen, routage vers Login ou MainMenu |
| **LevelScoreManager** | Persistance des étoiles par niveau (meilleur score), calcul du total d'étoiles |
| **SceneManager** | Wrapper pour l'API SceneManagement de Unity |

### 2.4 Contrôleurs d'exercices

Hiérarchie basée sur l'interface `IExerciseController` et la classe abstraite `BaseExerciseController` :

```
IExerciseController (interface)
  └── BaseExerciseController (abstract, MonoBehaviour)
        ├── QuizController
        ├── TrueFalseController
        ├── FillInBlankController
        ├── SortingController
        └── MatchingController
```

| Contrôleur | Interaction | Type de réponse |
|-----------|-------------|----------------|
| **QuizController** | Choix multiples via boutons | `int` (index de l'option) |
| **TrueFalseController** | Deux boutons "VRAI" / "FAUX" | `bool` |
| **FillInBlankController** | Boutons de mots à placer dans les trous `{0}`, `{1}`... | `List<string>` |
| **SortingController** | Glisser-déposer (DraggableItem → DropZone) | `Dictionary<int, int>` (item → catégorie) |
| **MatchingController** | Clic pour sélectionner et relier (colonnes gauche/droite) | `Dictionary<int, int>` (gauche → droite) |

L'interface `IExerciseController` expose :
- **Événements** : `OnAnswerSubmitted`, `OnExerciseCompleted`
- **Méthodes** : `Initialize()`, `Show()`, `Hide()`, `ShowFeedback()`, `Reset()`, `SetInteractable()`

### 2.5 Couche Data

#### BaseExerciseData (ScriptableObject abstrait)

Champs communs : `id`, `exerciseType`, `explanation`, `difficulty`, `category`, `image`.
Méthodes abstraites : `ValidateAnswer(object answer)`, `GetMainText()`.

| Type de données | Champs spécifiques |
|----------------|-------------------|
| **QuizExerciseData** | `questionText`, `options` (List), `correctOptionIndex` |
| **TrueFalseExerciseData** | `statement`, `isTrue` |
| **FillInBlankExerciseData** | `sentenceWithBlanks`, `correctAnswers`, `wordOptions`, `caseSensitive` |
| **SortingExerciseData** | `instruction`, `categories` (SortingCategory[]), `items` (SortableItem[]) |
| **MatchingExerciseData** | `instruction`, `leftColumnHeader`, `rightColumnHeader`, `pairs` (MatchPair[]), `shuffleRightColumn` |

#### ExerciseFactory (classe statique)

Convertit les DTOs reçus de l'API en objets `BaseExerciseData` via un dispatch par type d'exercice (switch expression). Crée dynamiquement des instances de ScriptableObject.

#### DTOs

- **AuthDtos** : `LoginRequestDto`, `RegisterRequestDto`, `AuthResponseDto`, `UserResponseDto`, `ErrorResponseDto`
- **ExerciseDtos** : `BaseExerciseDto` → `QuizExerciseDto`, `TrueFalseExerciseDto`, `FillInBlankExerciseDto`, `SortingExerciseDto`, `MatchingExerciseDto`, `GenericExerciseDto`, `LevelWithExercisesDto`

#### Enums

- **ExerciseType** : `Quiz`, `TrueFalse`, `FillInBlank`, `Sorting`, `Matching`
- **Difficulty** : `Easy` (0), `Medium` (1), `Hard` (2)
- **Category** : `Environment` (0), `Social` (1), `Governance` (2), `Economy` (3)

### 2.6 Design Patterns

| Pattern | Utilisation |
|---------|------------|
| **Singleton** | Tous les Managers (`Instance` statique + `DontDestroyOnLoad`) |
| **Factory** | `ExerciseFactory` — création d'exercices depuis les DTOs |
| **Strategy** | `IExerciseController` — chaque type d'exercice implémente sa propre logique d'affichage et de validation |
| **Template Method** | `BaseExerciseController` — définit le flux (Initialize → Reset → Show → Feedback), les sous-classes surchargent les étapes |
| **Observer** | Événements C# (`Action<T>`) pour la communication inter-composants : `OnAnswerSubmitted`, `OnXPGained`, `OnAuthStateChanged`, etc. |

### 2.7 Namespaces

Les namespaces reflètent l'arborescence des dossiers :

```
Client/Assets/Scripts/
├── Managers/           → namespace Managers
├── Data/               → namespace Data
│   └── Exercises/      → namespace Data.Exercises
├── UI/                 → namespace UI
│   └── Exercises/      → namespace UI.Exercises
├── Testing/            → namespace Testing
└── Utilities/          → namespace Utilities
```

### 2.8 Flux de jeu

```
Boot Scene (BootManager)
  │
  ├─ Authentifié ? → Oui → Menu Principal (MainMenuManager)
  └─ Non → Scène Login

Menu Principal
  │ (sélection d'un niveau)
  ↓
GameManager.LoadLevel(apiUrl)
  → ExerciseManager.LoadLevelAsync(apiUrl)
  → ExerciseFactory.CreateFromDtos()
  → GameManager.StartGame() → charge la scène Game

Scène Game
  │
  ExerciseSceneInitializer → enregistre ExerciseUIController
  │
  GameManager.DisplayCurrentExercise()
  → ExerciseUIController sélectionne le bon contrôleur
  → Le joueur répond → événement OnAnswerSubmitted
  │
  GameManager.HandleAnswerSubmitted()
  ├─ Valide via BaseExerciseData.ValidateAnswer()
  ├─ Correct → GamificationManager.AwardCorrectAnswerXP()
  ├─ Incorrect → perte d'une vie
  ├─ Affiche le feedback visuel + panneau d'explication
  └─ Exercice suivant ou fin de niveau

Fin de niveau (Victoire)
  ├─ Calcul des étoiles (selon vies restantes)
  ├─ LevelScoreManager.SaveLevelStars()
  ├─ GamificationManager.AwardLevelCompletion()
  └─ UIManager.ShowVictoryScreen()

Fin de niveau (Game Over — 0 vies)
  └─ UIManager.ShowGameOverScreen()
```

---

## 3. Architecture Serveur (Go)

### 3.1 Stack technique

| Composant | Version / Package |
|-----------|------------------|
| Langage | Go 1.25.6 |
| Routeur HTTP | chi/v5 (v5.2.5) |
| Base de données | pgx/v5 (v5.8.0) — driver PostgreSQL |
| Migrations | golang-migrate/v4 (v4.19.1) |
| Authentification | golang-jwt/v5 (v5.3.1) — JWT HS256 |
| Hash de mots de passe | golang.org/x/crypto — bcrypt |
| Génération de requêtes | sqlc |
| CORS | go-chi/cors (v1.2.2) |
| Variables d'environnement | godotenv (v1.5.1) |

### 3.2 Structure du projet

```
Server/
├── cmd/server/
│   └── main.go                             # Point d'entrée : config, DB, migrations, routeur, shutdown
├── internal/
│   ├── config/
│   │   └── config.go                       # Chargement de la configuration (env vars)
│   ├── database/
│   │   └── db.go                           # Connexion PostgreSQL (pgxpool) + migrations
│   ├── db/                                 # ⚠️ Généré par sqlc — NE PAS MODIFIER
│   │   ├── db.go                           # Interface DBTX
│   │   ├── models.go                       # Structs des tables
│   │   ├── users.sql.go
│   │   ├── levels.sql.go
│   │   ├── exercises.sql.go
│   │   └── user_progress.sql.go
│   ├── handler/                            # Handlers HTTP (un fichier par ressource + tests)
│   │   ├── auth.go
│   │   ├── health.go
│   │   ├── levels.go
│   │   ├── progress.go
│   │   ├── helpers.go
│   │   ├── *_test.go                       # Tests unitaires
│   │   └── *_integration_test.go           # Tests d'intégration
│   ├── middleware/
│   │   ├── auth.go                         # Validation JWT
│   │   ├── logging.go                      # Log des requêtes
│   │   ├── recovery.go                     # Récupération des panics
│   │   └── json.go                         # Content-Type application/json
│   └── router/
│       └── router.go                       # Configuration Chi et montage des routes
├── migrations/
│   ├── migrations.go                       # Embedded FS
│   ├── 0001_initial_schema.up.sql          # Tables de base
│   ├── 0001_initial_schema.down.sql
│   ├── 0002_seed_levels_exercises.up.sql   # 3 niveaux, 11 exercices
│   ├── 0002_seed_levels_exercises.down.sql
│   ├── 0003_seed_content_expansion.up.sql  # 7 niveaux supplémentaires, 42 exercices
│   └── 0003_seed_content_expansion.down.sql
├── queries/                                # Requêtes SQL pour sqlc
│   ├── users.sql
│   ├── levels.sql
│   ├── exercises.sql
│   └── user_progress.sql
├── Dockerfile                              # Build multi-stage (golang:1.25 → alpine:3.21)
├── docker-compose.yml
├── Makefile
├── sqlc.yaml
├── go.mod
└── go.sum
```

### 3.3 Clean Architecture

- **Injection de dépendances** via champs de struct (pas de variables globales).
- **Interfaces Store** pour la testabilité : les handlers dépendent d'interfaces, mockées dans les tests unitaires.
- **Séparation des responsabilités** : `handler/` gère le HTTP, `db/` gère les requêtes SQL, `middleware/` gère les aspects transversaux.

### 3.4 Middleware

**Middleware global** (appliqué à toutes les routes) :

| Middleware | Rôle |
|-----------|------|
| **Recovery** | Récupère les panics → réponse JSON 500 avec stack trace |
| **Logger** | Log : méthode, chemin, code de statut, durée (ms) |
| **CORS** | Origines configurables, méthodes GET/POST/PUT/DELETE/OPTIONS |

**Middleware spécifique aux routes** :

| Middleware | Rôle |
|-----------|------|
| **JSON** | Définit `Content-Type: application/json` |
| **Auth** | Valide le JWT Bearer, extrait le user ID dans le contexte |

L'authentification JWT utilise l'algorithme **HS256**. Le token contient le user ID en claim `Subject`, avec des claims `IssuedAt`, `ExpiresAt` et `Issuer` ("greenskills-api").

---

## 4. Référence API

### 4.1 Informations générales

- **Base URL** : `/api`
- **Format** : JSON (`Content-Type: application/json`)
- **Authentification** : Bearer Token JWT (`Authorization: Bearer <token>`)

### 4.2 Routes publiques

#### `GET /api/health`

Vérification de l'état du serveur.

**Réponse 200 :**
```json
{
  "status": "ok",
  "timestamp": "2026-03-17T10:00:00Z"
}
```

#### `POST /api/auth/register`

Inscription d'un nouvel utilisateur.

**Corps de la requête :**
```json
{
  "email": "user@example.com",
  "username": "eco_manager",
  "password": "securePassword123"
}
```

**Réponse 201 :**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "user": {
    "id": "uuid",
    "email": "user@example.com",
    "username": "eco_manager",
    "createdAt": "2026-03-17T10:00:00Z"
  }
}
```

**Erreurs** : 400 (champs manquants), 409 (email ou username déjà utilisé)

#### `POST /api/auth/login`

Connexion d'un utilisateur existant.

**Corps de la requête :**
```json
{
  "email": "user@example.com",
  "password": "securePassword123"
}
```

**Réponse 200 :** Même format que la réponse de register.

**Erreurs** : 400 (champs manquants), 401 (identifiants invalides)

#### `GET /api/levels`

Liste tous les niveaux avec le nombre d'exercices.

**Réponse 200 :**
```json
{
  "levels": [
    { "levelId": 1, "theme": "Tri des Déchets", "exerciseCount": 5 },
    { "levelId": 2, "theme": "Énergie Numérique", "exerciseCount": 3 }
  ]
}
```

#### `GET /api/levels/{id}/exercises`

Récupère les exercices d'un niveau (id = numéro du niveau).

**Réponse 200 :**
```json
{
  "levelId": 1,
  "theme": "Tri des Déchets",
  "exercises": [
    {
      "exerciseType": "quiz",
      "explanation": "Le verre se recycle...",
      "difficulty": 0,
      "category": 0,
      "questionText": "...",
      "options": ["A", "B", "C", "D"],
      "correctOptionIndex": 2
    }
  ]
}
```

**Erreurs** : 404 (niveau non trouvé)

### 4.3 Routes protégées (JWT requis)

#### `GET /api/users/me/progress`

Récupère la progression de l'utilisateur connecté.

**Réponse 200 :**
```json
{
  "progress": [
    {
      "levelId": 1,
      "stars": 3,
      "bestTimeSeconds": 45.2,
      "unlocked": true,
      "completedAt": "2026-03-15T14:30:00Z"
    }
  ]
}
```

#### `PUT /api/users/me/progress/{levelId}`

Sauvegarde ou met à jour la progression d'un niveau.

**Corps de la requête :**
```json
{
  "stars": 3,
  "bestTimeSeconds": 42.5,
  "unlocked": true,
  "completed": true
}
```

**Réponse 200 :** Objet progression mis à jour.

**Logique UPSERT** : conserve le meilleur score (GREATEST pour les étoiles, LEAST pour le temps).

#### `GET /api/users/me/stats`

Statistiques agrégées de l'utilisateur.

**Réponse 200 :**
```json
{
  "totalStars": 24,
  "levelsCompleted": 8,
  "levelsStarted": 10
}
```

**Erreur commune** : 401 (token manquant ou invalide)

---

## 5. Schéma de base de données

### Diagramme des relations

```
┌──────────┐       ┌─────────────────┐       ┌──────────────┐
│  users   │       │ user_progress   │       │   levels     │
├──────────┤       ├─────────────────┤       ├──────────────┤
│ id (PK)  │──1:N──│ user_id (FK)    │       │ id (PK)      │
│ email    │       │ level_id (FK)   │──N:1──│ level_number │
│ username │       │ stars           │       │ theme        │
│ password │       │ best_time       │       │ created_at   │
│ hash     │       │ unlocked        │       └──────┬───────┘
│ created  │       │ completed_at    │              │
│ updated  │       │ UNIQUE(user,    │              │
└──────────┘       │   level)        │       ┌──────┴───────┐
                   └─────────────────┘       │  exercises   │
                                             ├──────────────┤
                                             │ id (PK)      │
                                             │ level_id(FK) │
                                             │ exercise_type│
                                             │ difficulty   │
                                             │ category     │
                                             │ explanation  │
                                             │ data_json    │
                                             │ sort_order   │
                                             │ created_at   │
                                             └──────────────┘
```

### Détail des tables

#### `users`

| Colonne | Type | Contraintes |
|---------|------|------------|
| `id` | UUID | PK, DEFAULT `gen_random_uuid()` |
| `email` | VARCHAR(255) | UNIQUE, NOT NULL |
| `username` | VARCHAR(100) | UNIQUE, NOT NULL |
| `password_hash` | VARCHAR(255) | NOT NULL |
| `created_at` | TIMESTAMPTZ | NOT NULL, DEFAULT `now()` |
| `updated_at` | TIMESTAMPTZ | NOT NULL, DEFAULT `now()` |

#### `levels`

| Colonne | Type | Contraintes |
|---------|------|------------|
| `id` | SERIAL | PK |
| `level_number` | INT | UNIQUE, NOT NULL |
| `theme` | VARCHAR(255) | NOT NULL |
| `created_at` | TIMESTAMPTZ | NOT NULL, DEFAULT `now()` |

**Données** : 10 niveaux (Tri des Déchets, Énergie Numérique, RSE Fondamentaux, Mobilité Durable, Sobriété Numérique, RSE en Entreprise, Tri Avancé, Mobilité Avancée, Numérique Responsable, RSE en Action).

#### `exercises`

| Colonne | Type | Contraintes |
|---------|------|------------|
| `id` | SERIAL | PK |
| `level_id` | INT | FK → levels(id) ON DELETE CASCADE |
| `exercise_type` | VARCHAR(50) | NOT NULL (quiz, trueFalse, sorting, fillInBlank, matching) |
| `difficulty` | INT | NOT NULL, DEFAULT 0 |
| `category` | INT | NOT NULL, DEFAULT 0 |
| `explanation` | TEXT | NOT NULL, DEFAULT '' |
| `data_json` | JSONB | NOT NULL, DEFAULT '{}' |
| `sort_order` | INT | NOT NULL, DEFAULT 0 |
| `created_at` | TIMESTAMPTZ | NOT NULL, DEFAULT `now()` |

**Index** : `idx_exercises_level_id`, `idx_exercises_exercise_type`

**Données** : 53 exercices répartis sur 10 niveaux.

#### `user_progress`

| Colonne | Type | Contraintes |
|---------|------|------------|
| `id` | UUID | PK, DEFAULT `gen_random_uuid()` |
| `user_id` | UUID | FK → users(id) ON DELETE CASCADE |
| `level_id` | INT | FK → levels(id) ON DELETE CASCADE |
| `stars` | INT | NOT NULL, DEFAULT 0 (0–3) |
| `best_time_seconds` | FLOAT | Nullable |
| `unlocked` | BOOLEAN | NOT NULL, DEFAULT false |
| `completed_at` | TIMESTAMPTZ | Nullable |
| `created_at` | TIMESTAMPTZ | NOT NULL, DEFAULT `now()` |
| `updated_at` | TIMESTAMPTZ | NOT NULL, DEFAULT `now()` |

**Contraintes** : UNIQUE(`user_id`, `level_id`)
**Index** : `idx_user_progress_user_id`

---

## 6. Système de gamification

### 6.1 XP (Points d'expérience)

| Source | XP gagnés |
|--------|----------|
| Bonne réponse | 10 × streak multiplier |
| Complétion de niveau | 50 × streak multiplier |
| Bonus parfait (0 vie perdue) | +30 × streak multiplier |

- **Niveau joueur** = 1 + (XP total ÷ 100)
- **XP dans le niveau actuel** = XP total mod 100
- **Level-up** déclenché tous les 100 XP

### 6.2 Streak multiplier

Le multiplicateur de série augmente avec les jours consécutifs de jeu :

```
Multiplicateur = min(1.0 + streak × 0.1, 2.0)
```

| Jours consécutifs | Multiplicateur |
|-------------------|---------------|
| 0 | 1.0× |
| 1 | 1.1× |
| 5 | 1.5× |
| 10+ | 2.0× (maximum) |

**Logique du streak** :
- Si la dernière session est aujourd'hui → streak inchangé
- Si la dernière session est hier → streak + 1
- Sinon → streak remis à 0

### 6.3 EcoCoins

| Source | Coins gagnés |
|--------|-------------|
| Complétion de niveau | 5 |
| Bonus parfait (0 vie perdue) | +10 |
| Bonus streak | 2 × nombre de jours de streak |

### 6.4 Étoiles

Les étoiles sont attribuées à la fin de chaque niveau en fonction des vies restantes :

| Étoiles | Condition |
|---------|----------|
| ⭐⭐⭐ (3) | 100% des vies restantes |
| ⭐⭐ (2) | ≥ 66% des vies restantes |
| ⭐ (1) | < 66% des vies restantes |

Seul le **meilleur score** est conservé (pas de régression).

### 6.5 Vies

- **3 vies** par niveau
- **−1 vie** par mauvaise réponse
- **0 vie** = Game Over (possibilité de recommencer le niveau)

### 6.6 Persistance

**Côté client (PlayerPrefs)** :

| Clé | Donnée |
|-----|--------|
| `Gamification_TotalXP` | XP total accumulé |
| `Gamification_EcoCoins` | Solde d'EcoCoins |
| `Gamification_Streak` | Nombre de jours consécutifs |
| `Gamification_LastPlayDate` | Date de la dernière session |
| `HighestLevelUnlocked` | Dernier niveau déverrouillé |
| `Level_{id}_Stars` | Étoiles du niveau (meilleur score) |

**Côté serveur** : Table `user_progress` (étoiles, meilleur temps, état de complétion, état de déverrouillage).

---

## 7. Déploiement et infrastructure

### 7.1 Architecture de déploiement

```
Internet
  │
  ▼
┌─────────┐     ┌───────────────────────────────────────┐
│ Traefik │────►│           VPS (Dokploy)                │
│ (HTTPS) │     │  ┌─────────────┐  ┌────────────────┐  │
└─────────┘     │  │  API (Go)   │  │ PostgreSQL 17  │  │
                │  │  :8080      │──│  :5432         │  │
                │  └─────────────┘  └────────────────┘  │
                └───────────────────────────────────────┘
```

### 7.2 Docker Compose

Deux services définis dans `docker-compose.yml` :

**Service `postgres`** :
- Image : `postgres:17-alpine`
- Port : `5432:5432`
- Volume : `pgdata` (persistant)
- Health check : `pg_isready -U postgres` (intervalle 5s, 5 tentatives)

**Service `api`** :
- Build depuis le `Dockerfile` (multi-stage : `golang:1.25-alpine` → `alpine:3.21`)
- Port : `8080:8080`
- Dépend de : `postgres` (condition: healthy)
- Health check : `wget -qO- http://localhost:8080/api/health` (intervalle 10s)

### 7.3 Variables d'environnement

| Variable | Défaut | Description |
|----------|--------|------------|
| `PORT` | `8080` | Port d'écoute du serveur |
| `DATABASE_URL` | `postgres://postgres:postgres@localhost:5432/greenskills?sslmode=disable` | Chaîne de connexion PostgreSQL |
| `JWT_SECRET` | _(vide)_ | **Requis** — Clé secrète pour la signature JWT |
| `JWT_EXPIRY_HOURS` | `24` | Durée de validité du token JWT (heures) |
| `CORS_ALLOWED_ORIGINS` | `http://localhost:*` | Origines autorisées (séparées par des virgules) |
| `ENV` | `development` | Mode (`development` ou `production`) |

### 7.4 Migrations

Les migrations s'exécutent **automatiquement** au démarrage du serveur via `golang-migrate` avec un système de fichiers embarqué (`embed.FS`).

| Migration | Contenu |
|-----------|---------|
| `0001_initial_schema` | Création des tables `users`, `levels`, `exercises`, `user_progress` |
| `0002_seed_levels_exercises` | 3 niveaux et 11 exercices initiaux |
| `0003_seed_content_expansion` | 7 niveaux supplémentaires et 42 exercices |

Chaque migration dispose d'un fichier `.up.sql` et `.down.sql` pour les rollbacks.

---

## 8. Guide de développement

### 8.1 Prérequis

| Outil | Version |
|-------|---------|
| Go | 1.25.6+ |
| Docker & Docker Compose | Dernière version stable |
| Unity Hub + Unity | 6000.3.3f1 |
| sqlc | Dernière version |
| PostgreSQL client (optionnel) | 17 |

### 8.2 Démarrage rapide — Serveur

```bash
# Démarrer la base de données
cd Server && docker compose up -d db

# Lancer le serveur (les migrations s'exécutent automatiquement)
cd Server && go run ./cmd/server
```

Le serveur écoute sur `http://localhost:8080`. Testez avec :
```bash
curl http://localhost:8080/api/health
```

### 8.3 Démarrage rapide — Client

1. Ouvrir **Unity Hub**
2. Ajouter le projet `Client/`
3. S'assurer que la version Unity **6000.3.3f1** est installée
4. Ouvrir le projet et lancer la scène **Boot**

### 8.4 Commandes Makefile

| Commande | Description |
|----------|------------|
| `make up` | Démarrer l'API + PostgreSQL (avec rebuild) |
| `make down` | Arrêter tous les services |
| `make db-up` | Démarrer PostgreSQL uniquement |
| `make db-down` | Arrêter PostgreSQL |
| `make db-reset` | Réinitialiser la base de données (supprime le volume) |
| `make run` | Lancer le serveur localement (nécessite `make db-up`) |
| `make build` | Compiler le binaire dans `bin/server` |
| `make test` | Exécuter les tests unitaires |
| `make test-v` | Tests en mode verbeux |
| `make test-cover` | Tests avec couverture de code |
| `make vet` | Analyse statique (`go vet`) |
| `make clean` | Supprimer le dossier `bin/` |

### 8.5 Tests

**Tests unitaires** (mocks des interfaces Store) :
```bash
cd Server && go test ./...
```

**Tests d'intégration** (nécessitent PostgreSQL) :
```bash
cd Server && go test -tags=integration ./...
```

**Test spécifique** :
```bash
cd Server && go test -run TestName ./internal/handler/...
```

**Avec couverture** :
```bash
cd Server && go test -v -cover ./...
```

### 8.6 Génération sqlc

Le dossier `Server/internal/db/` est **généré automatiquement** par sqlc à partir des fichiers dans `queries/` et du schéma dans `migrations/`. **Ne jamais modifier ces fichiers manuellement.**

Pour régénérer après modification des requêtes SQL :
```bash
cd Server && sqlc generate
```

Configuration dans `sqlc.yaml` :
- Engine : PostgreSQL
- Package généré : `db`
- Driver SQL : `pgx/v5`
- Tags JSON et slices vides activés

---

## 9. CI/CD

### GitHub Actions

Le pipeline CI est défini dans `.github/workflows/ci.yml` et se déclenche sur :
- **Pull request** vers `main` (chemins : `Server/**`)
- **Push** vers `main` (chemins : `Server/**`)

**Étapes du job "Server checks"** (Ubuntu latest) :

| Étape | Commande |
|-------|---------|
| Checkout | `actions/checkout@v4` |
| Setup Go | `actions/setup-go@v5` (version depuis `go.mod`) |
| Build | `go build ./...` |
| Vet | `go vet ./...` |
| Tests unitaires | `go test -v -race -count=1 ./...` |
| Tests d'intégration | `go test -v -race -count=1 -tags=integration ./...` |

**Services CI** : PostgreSQL 17-alpine avec health check `pg_isready`.

**Variables d'environnement CI** :
- `DATABASE_URL` : connexion locale au PostgreSQL du service
- `JWT_SECRET` : `ci-test-secret`

---

## Documents associés

- [Concept du Jeu](Game%20concept.md)
- [Serious Game Design Document (GDD)](Serious%20Game%20Design%20Document.md)
- [Business Model Canvas](Business%20Model%20Canvas.md)
- [README (accueil du projet)](../README.md)
