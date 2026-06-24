# Heros of Hammer and Sickle

A terminal-based roguelike RPG set in WWII occupied France. Lead a resistance cell through tactical combat, faction diplomacy, and moral choices in a grid-based world rendered with Unicode characters.

## Technology Stack

- **Runtime:** .NET 9 (C#)
- **Terminal UI:** Terminal.Gui 2.0
- **Editor UI:** Avalonia 11.2 (MVVM)
- **Scripting:** Lua (NLua/MoonSharp)
- **Logging:** Serilog
- **Testing:** MSTest

## Quick Start

```bash
# Build
dotnet build Heros.sln

# Run
dotnet run --project src/HHSGame/HHSGame.csproj

# Run with custom config
dotnet run --project src/HHSGame/HHSGame.csproj -- --config data/game.json

# Run tests
dotnet test src/HHSGameTest/HHSGameTest.csproj
```

## Solution Structure

```
Heros.sln
├── src/HHSGame.Core/       # Core game logic (no UI dependency)
├── src/HHSGame/            # Terminal.Gui game executable
├── src/HHSEditor.Core/     # Editor service layer
├── src/HHSEditor/          # Avalonia desktop editor
└── src/HHSGameTest/        # MSTest test project
```

## Key Features

- **AP-driven tactical combat** with action sequences, ranged targeting, and cover mechanics
- **8-faction reputation system** (Occupier, Resistance, Bandits, Civilians, Church, Puppet, Allies, Neutral)
- **Dialogue & quest system** with 17 quests across 3 acts, party assist for skill checks
- **Data-driven engine** loading maps, items, enemies, and classes from JSON configs
- **Lua scripting** for triggers, events, and custom game logic
- **Save/Load system** with 3 slots and full state serialization

## Documentation

- [INK.md](INK.md) — Documentation index (four-layer structure)
- [docs/conceptual/game-design-spec.md](docs/conceptual/game-design-spec.md) — Game Design Specification
- [docs/behavioral/](docs/behavioral/) — Design specs, improvement plans, system architectures
- [docs/operational/](docs/operational/) — Progress reports

## License

Private project.
