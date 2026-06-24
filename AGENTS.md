# AGENTS.md

## Repository Summary
- HHSGame is a terminal-based roguelike (NetHack-like) set in WWII occupied France.
- C#/.NET 9 solution: `Heros.sln` with 4 projects + 1 test project.
- UI: Terminal.Gui 2.0 (game), Avalonia 11.2 (editor).
- Logging: Serilog; DI: Microsoft.Extensions.*; Scripting: Lua (NLua/MoonSharp).
- Combat uses AP-driven action planning with queued sequences (`ActionSequence`) that execute on turn commit; AP only applies in combat.
- Tests live in `src/HHSGameTest` (MSTest).

## Solution Structure
```
Heros.sln
├── src/HHSGame.Core/       # Core game logic (no UI dependency)
│   ├── Combat/             # Combat resolver, targeting, action sequences
│   ├── Map/                # Map state, pathfinding, generation
│   ├── Items/              # Item definitions, catalogs
│   ├── Enemies/            # Enemy AI, abilities, loot
│   ├── Skills/             # Skill actions, execution
│   ├── Quests/             # Quest models, manager
│   ├── Dialogue/           # Dialogue manager, definitions
│   ├── Interactions/       # IInteractable, InteractableManager
│   ├── Triggers/           # Map trigger system
│   ├── Scripting/          # Lua script engine integration
│   ├── Factions/           # FactionManager (8 factions)
│   ├── Classes/            # Class definitions
│   ├── Save/               # SaveManager, LoadManager
│   ├── Engine/             # GameEngine, Config, Launcher
│   └── ...
├── src/HHSGame/            # Terminal.Gui game executable
│   ├── UI/                 # Views, windows, rendering
│   ├── Engine/             # Game engine integration
│   └── Resources/          # Localization assets
├── src/HHSEditor.Core/     # Editor service layer
├── src/HHSEditor/          # Avalonia desktop editor (MVVM)
│   ├── ViewModels/         # 10 ViewModels
│   ├── Views/              # 14 Avalonia views
│   └── Controls/           # MapCanvas, GameCanvas, DialogueGraph, QuestGraph
└── src/HHSGameTest/        # MSTest test project (27 test files)
```

## Data Structure
```
data/
├── game.json               # Main game configuration
├── catalogs/               # armors, classes, enemies, items, weapons
├── maps/                   # 10 ASCII tilemap files
└── scripts/                # Combat scripts
```

## Common Commands
- Build solution: `dotnet build Heros.sln`
- Build game only: `dotnet build src/HHSGame/HHSGame.csproj`
- Run the game: `dotnet run --project src/HHSGame/HHSGame.csproj`
- Run with config: `dotnet run --project src/HHSGame/HHSGame.csproj -- --config data/game.json`
- Run tests: `dotnet test src/HHSGameTest/HHSGameTest.csproj`
- Clean: `dotnet clean`

## Notes
- Logs appear in `hss.log` at repo root.
- CI runs on push to main/master/wwii branches (`.github/workflows/dotnet.yml`).
- Documentation is organized in four layers under `docs/` — see `INK.md` for the index.

## Guidelines
- Add unit tests as soon as possible; aim for test coverage on all changes.
- Design features from a player's viewpoint — make it fun and challenging.
- Provide clear, simple guidance for a friendly user experience.
- The engine is data-driven: maps, items, enemies, and classes load from JSON configs in `data/`.
- Use Lua scripts for triggers, events, and custom game logic (see `docs/behavioral/v7-trigger-system.md`).
