# GreenSkills

> **Become the Eco-Manager of tomorrow, 2 minutes at a time.**

## About This Project

This application was developed as a "Red Thread" project for the **Master 1 M2i** (Manager en Ingénierie Informatique) curriculum at **CCI Campus Strasbourg**.

It serves as the final evaluation for the **Serious Game Application Development** module.

* **Period:** January 13, 2026 – March 19, 2026

* **Objective:** Create a mobile Serious Game application demonstrating skills in Unity, API management, and Gamification.

---

## Game Concept

**GreenSkills** is a mobile game designed to train professionals in **CSR (Corporate Social Responsibility)** and **Ecology** through gamified micro-learning.

The player embodies an **"Eco-Manager"** hired to transform a grey, outdated corporate office into a sustainable, green workspace. Through short 2-minute sessions, the player unlocks new skills, upgrades the office environment, and climbs the company leaderboard.

### Key Features

* **Micro-Learning Loops:** Quick sessions consisting of quizzes, swipe-sorting games (Tinder-style), and drag-and-drop puzzles.
* **Visual Progression:** The office environment evolves in real-time from a dull workspace to a vibrant, eco-friendly hub as the player levels up.
* **Skill Tree:** Progression through key themes: Ecology, CSR, and New Technologies.
* **Gamification:** Daily streaks, XP systems, and weekly leagues to drive retention.

---

## Tech Stack

This project implements a decoupled architecture separating the Game Client from the Logic/Data layer, meeting the requirement for API and Database integration.

### Mobile Client

* **Engine:** Unity 2022 LTS

* **Render Pipeline:** URP (2D) for optimized mobile performance.
* **Target:** Android (APK) & iOS.

### Backend & Infrastructure

* **Language:** Go - High-performance REST API.

* **Database:** PostgreSQL - Relational data storage for user progress and content.

* **Deployment:** Dockerized services managed via **Dokploy** on a VPS.
* **Security:** JWT Authentication & HTTPS via Traefik.
---

## Project Structure

* `Client/` - Unity Project files (C# Scripts, Assets, Prefabs).
* `Server/` - Go API source code.
* `Docs/` - Project documentation:
  * [*Game Concept*](Docs/Game%20concept.md)
  * [*Serious Game Design Document (GDD)*](Docs/Serious%20Game%20Design%20Document.md)

---

## Authors

* **Gauthier Seyzeriat** - *Game Design, Development & Project Management*

---

## License

This project is for educational purposes as part of the CCI Campus Strasbourg curriculum. All rights reserved.
