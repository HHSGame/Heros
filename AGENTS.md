# AGENTS.md

## Repository summary
- HHSGame is a terminal-based roguelike (NetHack-like).
- C#/.NET 9 solution: `Heros.sln` with the main game project in `src/HHSGame`.
- UI is built with Terminal.Gui; logging uses Serilog; DI uses Microsoft.Extensions.*.
- Tests live in `src/HHSGameTest` (MSTest).

## Structure overview
- `src/HHSGame/Core`: game loop, world, player, stats, map generation, combat, items, enemies.
- `src/HHSGame/UI`: Terminal.Gui views, windows, rendering.
- `src/HHSGame/Resources`: localization assets.
- `src/HHSGame/Utils`: shared utilities (e.g., i18n).
- `src/HHSGameTest`: unit tests for core functionality.
- `docs`, `data`, `tools`: supporting materials and utilities.
- `TODO.md`: roadmap (Chinese) for planned gameplay systems and features.

## Common commands
- Build solution: `dotnet build Heros.sln`
- Build game only: `dotnet build src/HHSGame/HHSGame.csproj`
- Run the game: `dotnet run --project src/HHSGame/HHSGame.csproj`
- Run tests: `dotnet test src/HHSGameTest/HHSGameTest.csproj`
- Clean: `dotnet clean`

## Notes
- Logs appear in `hss.log` at repo root and under `src/HHSGame`.
- `PROJECT.md` and `CLAUDE.md` contain detailed architecture and dev guidance.
