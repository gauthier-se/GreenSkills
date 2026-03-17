# GreenSkills

> **Devenez l'Éco-Manager de demain, 2 minutes à la fois.**

## À propos du projet

Cette application a été développée dans le cadre du projet "Fil Rouge" du cursus **Master 1 M2i** (Manager en Ingénierie Informatique) au **CCI Campus Strasbourg**.

Elle constitue l'évaluation finale du module **Développement d'Applications Serious Game**.

* **Période :** 13 janvier 2026 – 19 mars 2026

* **Objectif :** Créer une application mobile de type Serious Game démontrant des compétences en Unity, gestion d'API et Gamification.

---

## Concept du jeu

**GreenSkills** est un jeu mobile conçu pour former les professionnels à la **RSE (Responsabilité Sociétale des Entreprises)** et à l'**Écologie** via du micro-apprentissage gamifié.

Le joueur incarne un **"Éco-Manager"** recruté pour transformer un bureau d'entreprise gris et vétuste en un espace de travail durable et verdoyant. À travers de courtes sessions de 2 minutes, le joueur débloque de nouvelles compétences, améliore l'environnement du bureau et gravit le classement de l'entreprise.

### Fonctionnalités clés

* **Boucles de micro-apprentissage :** Sessions rapides composées de quiz, de jeux de tri par glissement (style Tinder) et de puzzles par glisser-déposer.
* **Progression visuelle :** L'environnement du bureau évolue en temps réel, passant d'un espace de travail terne à un lieu éco-responsable et vibrant au fil de la progression.
* **Arbre de compétences :** Progression à travers des thématiques clés : Écologie, RSE et Nouvelles Technologies.
* **Gamification :** Séries quotidiennes, systèmes d'XP et ligues hebdomadaires pour stimuler la rétention.

---

## Architecture

```
┌──────────────────┐       HTTPS/JSON        ┌──────────────────┐       SQL        ┌──────────────────┐
│   Client Mobile  │  ◄──────────────────►    │    API REST      │  ◄────────────►  │   PostgreSQL 17  │
│   Unity 6 (C#)   │                          │    Go 1.25       │                  │                  │
└──────────────────┘                          └──────────────────┘                  └──────────────────┘
```

- **Client** : Application Unity 6 (C# 9.0) — architecture Manager/Controller/Data avec 5 types d'exercices interactifs (Quiz, Vrai/Faux, Texte à trous, Tri, Mise en paire).
- **Serveur** : API REST en Go — Clean Architecture avec injection de dépendances, JWT, sqlc pour les requêtes type-safe.
- **Base de données** : PostgreSQL 17 — 4 tables (users, levels, exercises, user_progress), migrations automatiques.
- **Déploiement** : Docker + Dokploy sur VPS, HTTPS via Traefik.

Pour les détails complets, voir la [Documentation Technique](Docs/Documentation%20Technique.md).

---

## Stack technique

Ce projet implémente une architecture découplée séparant le Client de jeu de la couche Logique/Données, répondant aux exigences d'intégration API et Base de données.

### Client mobile

* **Moteur :** Unity 6 (6000.3.3f1)

* **Pipeline de rendu :** URP (2D) pour des performances mobiles optimisées.
* **Cibles :** Android (APK) et iOS.

### Backend et infrastructure

* **Langage :** Go — API REST haute performance.

* **Base de données :** PostgreSQL — Stockage relationnel pour la progression des utilisateurs et le contenu.

* **Déploiement :** Services conteneurisés gérés via **Dokploy** sur un VPS.
* **Sécurité :** Authentification JWT et HTTPS via Traefik.

---

## Démarrage rapide

### Serveur

```bash
cd Server && docker compose up -d db    # Démarrer PostgreSQL
cd Server && go run ./cmd/server         # Lancer l'API (migrations automatiques)
curl http://localhost:8080/api/health    # Vérifier
```

### Client

1. Ouvrir **Unity Hub** et ajouter le projet `Client/`
2. S'assurer que Unity **6000.3.3f1** est installé
3. Ouvrir et lancer la scène **Boot**

Voir la [Documentation Technique](Docs/Documentation%20Technique.md) pour le guide de développement complet et les commandes Makefile.

---

## Structure du projet

* `Client/` — Fichiers du projet Unity (scripts C#, assets, prefabs).
* `Server/` — Code source de l'API Go.
* `Docs/` — Documentation du projet :
  * [*Concept du Jeu*](Docs/Game%20concept.md)
  * [*Serious Game Design Document (GDD)*](Docs/Serious%20Game%20Design%20Document.md)
  * [*Business Model Canvas*](Docs/Business%20Model%20Canvas.md)
  * [*Documentation Technique*](Docs/Documentation%20Technique.md)

---

## Auteur

* **Gauthier Seyzeriat** — *Game Design, Développement et Gestion de projet*

---

## Licence

Ce projet est réalisé à des fins pédagogiques dans le cadre du cursus CCI Campus Strasbourg. Tous droits réservés.
