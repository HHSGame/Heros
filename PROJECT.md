# HHSGame Project Structure and Understanding

This document provides a high-level overview of the HHSGame project structure and key components. It is intended to serve as a quick reference for developers to understand the project and its organization.

## 1. Project Overview

HHSGame is a C# based nethack clone with random generated terrain map and enemies. The game runs on the terminal, and tiles are made by unicode characters.

## 2. Directory Structure

The project is organized into the following directories:

-   **Core:** Contains the core game logic, including:
    -   ActiveEffect.cs: Temporary effects applied to the player (e.g., poison, stun).
    -   BaseType.cs: Base classes for game objects with positions.
    -   CoreExtensions.cs
    -   Event.cs: Event system for raising and handling game events.
    -   Game.cs: Main game loop and logic.
    -   GameContext.cs: Manages game parameters, services, and dependencies.
    -   GameWorld.cs: Manages the game world, map, and entities.
    -   IGameActor.cs
    -   Player.cs: Represents the player character and handles player actions.
    -   Stats.cs: Manages player attributes and skills.
    -   Classes/: Defines character classes and their bonuses.
    -   Combat/: Manages combat-related logic (e.g., turn management).
    -   Enemies/: Manages enemies, their AI, and loot.
    -   Items/: Manages items, their properties, and usage.
    -   Map/: Generates and manages the game map.
-   **UI:** Contains the user interface code, built using Terminal.Gui:
    -   ColorPresets.cs: Defines color schemes for different game elements.
    -   DrawingContext.cs: Handles drawing the game to the terminal.
    -   GameUI.cs: Manages the overall UI and key event handling.
    -   UIExtensions.cs
    -   Views/: Contains custom views for displaying game information.
    -   Windows/: Contains window classes for the different UI elements.
-   **Infrastructure:** Likely contains infrastructure-related code (e.g., data persistence, networking).
-   **Utils:** Contains utility classes and functions (e.g., I18n for internationalization).
    -   I18n.cs
-   **Resources:** Contains resources such as localization files.
    -   Localization.resx
-   **bin:** Contains compiled binaries.
-   **obj:** Contains intermediate build objects.

## 3. Class/Namespace Hierarchy Overview

```
HHSGame
├── Core
│   ├── ActiveEffect (abstract class)
│   │   └── PoisonEffect (class)
│   │   └── StunEffect (class)
│   ├── BaseType (abstract class)
│   ├── CoreExtensions (static class)
│   ├── EventSystem (static class)
│   ├── Game (class)
│   ├── GameContext (class)
│   ├── GameWorld (class)
│   ├── IGameActor (interface)
│   │   └── Player (class)
│   │   └── Enemy (class)
│   ├── Stats (class)
│   ├── Classes
│   │   ├── AbstractClass (abstract class)
│   │   │   └── TypedClass (class)
│   │   ├── ClassConfig (record)
│   │   ├── Weapons (static class)
│   │   └── Armors (static class)
│   ├── Combat
│   │   └── TurnManager (class)
│   ├── Enemies
│   │   ├── Enemy (class)
│   │   ├── EnemyAbilitySystem (class)
│   │   ├── EnemyFactory (class)
│   │   ├── EnemyLootSystem (class)
│   │   ├── EnemyManager (class)
│   │   └── EnemyRegistry (class)
│   ├── Items
│   │   ├── Item (abstract class)
│   │   │   ├── HealthPotion (class)
│   │   │   ├── Weapon (class)
│   │   │   └── Armor (class)
│   │   ├── InventoryManager (class)
│   │   ├── ItemFactory (class)
│   │   └── ItemManager (class)
│   └── Map
│       ├── BaseMapGenerator (abstract class)
│       │   ├── CaveMapGenerator (class)
│       │   ├── HillsMapGenerator (class)
│       │   └── TownMapGenerator (class)
│       ├── CollisionSystem (class)
│       ├── MapGenerator (class)
│       ├── MapState (class)
│       ├── Pathfinder (class)
│       └── SurroundingsManager (class)
├── UI
│   ├── ColorPresets (static class)
│   ├── DrawingContext (class)
│   ├── GameUI (class)
│   ├── UIExtensions (static class)
│   ├── Views
│   │   ├── EventLoggerView (class)
│   │   ├── InventoryListView (class)
│   │   ├── MapView (class)
│   │   └── SurroundingsListView (class)
│   └── Windows
│       ├── InventoryWindow (class)
│       ├── MapWindow (class)
│       ├── MessageWindow (class)
│       ├── SurroundingsWindow (class)
│       └── UtilityWindow (class)
└── Utils
    └── I18n (class)
```

## 4. Key Components

### 4.1. Core

-   **Game Loop:** The `Game` class contains the main game loop, which updates the game state, draws the game world, and handles user input.
-   **Game Context:** The `GameContext` class manages the game's dependencies and provides access to various managers and factories.
-   **Map Generation:** The `MapGenerator` class is responsible for generating the game map. Different map generators (e.g., `CaveMapGenerator`, `HillsMapGenerator`) can be used to create different types of maps.
-   **Enemy Management:** The `EnemyManager` class manages the enemies in the game. It handles enemy spawning, updating, and death.
-   **Item Management:** The `ItemManager` class manages the items in the game. It handles item creation, usage, and inventory management.
-   **Player Actions:** The `Player` class handles player actions, such as movement, attacking, and using items.

### 4.2. UI

-   **Terminal.Gui:** The UI is built using the Terminal.Gui library, which provides a framework for creating terminal-based user interfaces.
-   **Windows:** The UI is divided into several windows, each responsible for displaying a specific part of the game:
    -   `MapWindow`: Displays the game map.
    -   `MessageWindow`: Displays game messages.
    -   `InventoryWindow`: Displays the player's inventory.
    -   `SurroundingsWindow`: Displays the entities in the player's surroundings.
    -   `UtilityWindow`: Provides access to various utility functions, such as using items.
-   **Drawing Context:** The `DrawingContext` class handles drawing the game to the terminal. It uses a `MapView` object to render the map and a `Viewport` object to manage the visible area of the map.
-   **Event Handling:** The UI uses the event system to respond to user input and game events.

## 5. Important Concepts

-   **Dependency Injection:** The project uses dependency injection to manage dependencies between classes. This makes the code more modular and testable.
-   **Event System:** The project uses an event system to communicate between different parts of the game. This allows for loose coupling and makes the code more flexible.
-   **Unicode Characters:** The game uses Unicode characters to represent game elements in the terminal.

## 6. Next Steps

-   Refer to the `TODO.md` file for a list of tasks to be completed.
-   Examine the code in more detail to gain a deeper understanding of its functionality.
-   Experiment with the game to see how different features work.
