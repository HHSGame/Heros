# AGENTS.md

## Repository summary
- HHSGame is a terminal-based roguelike (NetHack-like).
- C#/.NET 9 solution: `Heros.sln` with the main game project in `src/HHSGame`.
- UI is built with Terminal.Gui; logging uses Serilog; DI uses Microsoft.Extensions.*.
- Combat uses AP-driven action planning with queued sequences (`ActionSequence`) that execute on turn commit; AP only applies in combat.
- Tests live in `src/HHSGameTest` (MSTest).

## Structure overview
- `src/HHSGame/Core`: game loop, world, player, stats, map generation, combat (including action sequences), items, enemies.
- `src/HHSGame/UI`: Terminal.Gui views, windows, rendering, combat planning input handling.
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

## Guidelines

- Git Commit History contains detailed recent changes.
- Add Unit test as soon as possible and as much as possible, Make sure most of chages are test covered.
- Design and implement the features from a game player's viewpoint, make sure it's fun and challenging.
- Make a friendly user experience by improve clear and simple guidance.
- Consider build a engine-based game to make sure it supports different genres (RPG, Turn-Based Strategy, etc.).
