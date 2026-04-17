# Questify — Gamified Productivity Dashboard

**Questify** is a gamified productivity system that transforms everyday tasks into a structured progression experience. It combines goal tracking, habit building, and performance analytics within a clean, game-inspired interface designed to encourage consistency and measurable growth.

---

## Overview

Questify reframes productivity as a progression system. Tasks become quests, habits build streaks, and consistent effort translates into experience points, levels, and achievements. The system is designed to remain minimal and focused while still delivering a satisfying, game-like feedback loop.

---

## Core Features

### Quest System

Create and manage tasks with defined difficulty levels (Easy, Medium, Hard). Completing quests rewards experience points and in-app currency, reinforcing progress through tangible outcomes.

### Habit Tracker

Track recurring actions with support for daily, weekly, and custom frequencies. Habits maintain streaks over time, encouraging consistency through visible progression and reward incentives.

### Skill Progression

Distribute progress across key life domains such as Health, Study, and Social. Growth is reflected through structured progression rather than abstract tracking.

### Achievements

Unlock milestones and badges based on performance, consistency, and long-term engagement.

### Quest Chains

Support for multi-step tasks that follow a defined progression path, enabling structured workflows and larger goals.

---

## Progression System

### Experience and Levels

Users earn experience points through quests and habits. Accumulated XP contributes to level progression, unlocking further engagement.

### Currency System

Coins are earned through task completion and can be used within the system for future extensibility.

### Streak Mechanics

Habits build streaks over time, with incentives tied to maintaining consistency.

---

## Analytics and Insights

### Activity Feed

A real-time log of completed actions, rewards, and progression updates.

### Adaptive Difficulty

The system can evolve based on user performance, enabling smarter task balancing.

### Productivity Insights

Identify patterns in activity and determine optimal performance periods.

### Heatmap Calendar

Visual representation of consistency, inspired by contribution tracking systems.

### Analytics Dashboard

Track XP trends, distribution of effort, and overall progression metrics.

---

## User Experience

* Single Page Application architecture
* Clean and responsive interface across devices
* Theme system (Dark, Light, Cyber-inspired variants)
* Smooth transitions and feedback animations
* Minimal, structured layout with emphasis on clarity and usability

---

## Technology Stack

### Backend

* ASP.NET Core (.NET 10)
* C#
* RESTful API architecture
* In-memory data storage (extendable to EF Core or SQLite)

#### Services

* XPService
* HabitService
* AchievementService
* IntelligenceService
* QuestChainService

---

### Frontend

* Vanilla JavaScript (ES6 Modules)
* HTML5 and CSS3
* Chart.js for analytics visualization
* Canvas Confetti for visual feedback

---

## Project Structure

```
LifeAsAGame.Api/
│
├── Controllers/      # API endpoints
├── Services/         # Business logic
├── Models/           # Domain models
├── Data/             # Data layer
├── DTOs/             # API contracts
├── Infrastructure/   # Utilities
└── wwwroot/          # Frontend (SPA)
```

---

## API Overview

### Authentication

* POST /api/auth/register
* POST /api/auth/login
* GET /api/auth/me

### User

* GET /api/user/dashboard
* GET /api/user/analytics

### Tasks

* POST /api/tasks
* POST /api/tasks/{id}/complete

### Habits

* POST /api/habits
* POST /api/habits/{id}/complete

---

## Reward System

| Difficulty | XP | Coins |
| ---------- | -- | ----- |
| Easy       | 10 | 5     |
| Medium     | 25 | 12    |
| Hard       | 50 | 25    |

---

## Roadmap

* Database integration (PostgreSQL or SQLite)
* Social and multiplayer features
* Mobile application
* Advanced recommendation system

---


## Closing Note

Questify is designed as a structured system for personal growth. It focuses on clarity, consistency, and measurable progress rather than superficial gamification.

Your time and effort become trackable, visible, and actionable.
