# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Build the entire solution
dotnet build Heros.sln

# Build only the game project
dotnet build src/HHSGame/HHSGame.csproj

# Run the game
dotnet run --project src/HHSGame/HHSGame.csproj

# Run tests
dotnet test src/HHSGameTest/HHSGameTest.csproj

# Run specific test
dotnet test --filter "FullyQualifiedName~GameLoaderTest"

# Clean build artifacts
dotnet clean
```

## Project Structure

**HHSGame** - Terminal-based roguelike game set in WWII Northern China
- **Core/**: Game logic (Game.cs, Player.cs, GameWorld.cs, Stats.cs)
- **Enemies/**: Enemy system (Enemy.cs, EnemyManager.cs, EnemyFactory.cs)
- **Items/**: Item system (Item.cs, InventoryManager.cs, ItemFactory.cs)
- **Map/**: Map generation (CaveMapGenerator.cs, HillsMapGenerator.cs, TownMapGenerator.cs)
- **Combat/**: Turn-based combat (TurnManager.cs)
- **UI/**: Terminal.Gui-based interface (GameUI.cs, MapView.cs, InventoryFrame.cs)
- **Classes/**: Character classes and progression

**HHSGameTest** - MSTest unit tests for core functionality

## Key Architecture

- **Terminal.Gui**: UI framework for terminal interface
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Logging**: Serilog with file sinks
- **Map Generation**: Procedural with multiple generator types
- **Turn-based**: Traditional roguelike turn system
- **Unicode**: Terminal rendering with Unicode characters

## Development Status

Active development with TODO.md tracking progress across:
- Terminal display & input handling
- Game loop & state management  
- Map generation & management
- Combat system implementation
- Items & equipment systems