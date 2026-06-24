# INK.md

## Conceptual (Why)

- [README.md](README.md) — Project overview: WWII occupied-France terminal roguelike, tech stack, quick start
- [AGENTS.md](AGENTS.md) — Repository summary, solution structure, build commands, agent guidelines
- [docs/conceptual/game-design-spec.md](docs/conceptual/game-design-spec.md) — Game Design Specification: 5 core attributes, Karma, 15 skills, AP combat economy, 8 factions, perks & traits

## Structural (Where)

- [Heros.sln](Heros.sln) — Solution: HHSGame.Core, HHSGame, HHSEditor.Core, HHSEditor
- [Directory.Build.props](Directory.Build.props) — Shared build properties: LangVersion=latest, Nullable=enable, AnalysisMode=Recommended
- [src/HHSGame.Core/](src/HHSGame.Core/) — Core game logic (no UI): Combat, Map, Items, Enemies, Skills, Quests, Dialogue, Triggers, Scripting, Factions, Classes, Save/Load, Engine
- [src/HHSGame/](src/HHSGame/) — Terminal.Gui executable: UI views, engine integration, resources
- [src/HHSEditor/](src/HHSEditor/) — Avalonia desktop editor: 10 ViewModels, 14 Views, Controls (MapCanvas, GameCanvas, DialogueGraph, QuestGraph)
- [src/HHSEditor.Core/](src/HHSEditor.Core/) — Editor service layer bridging editor and game core
- [src/HHSGameTest/](src/HHSGameTest/) — MSTest project: 27 test files covering all subsystems
- [data/](data/) — Game data: game.json, catalogs/ (armors, classes, enemies, items, weapons), maps/ (10 ASCII tilemaps), scripts/
- [demo-game/](demo-game/) — Self-contained demo showcasing editor capabilities

## Behavioral (How)

- [docs/behavioral/save-load-system.md](docs/behavioral/save-load-system.md) — Save/Load design: data models, UX flow, 20 acceptance criteria
- [docs/behavioral/engine-refactoring-plan.md](docs/behavioral/engine-refactoring-plan.md) — Engine architecture: config schema, test script format, milestones
- [docs/behavioral/v2-improvement-plan.md](docs/behavioral/v2-improvement-plan.md) — Sprint record: P0-P7 improvements with metrics (2026-05-08)
- [docs/behavioral/v4-wwii-retheme-status.md](docs/behavioral/v4-wwii-retheme-status.md) — WWII retheme: factions, NPCs, quests, weapons, implementation status
- [docs/behavioral/v5-codebase-improvement-plan.md](docs/behavioral/v5-codebase-improvement-plan.md) — Code quality: God Class decomposition, CI/CD, layer fixes (2026-06-05)
- [docs/behavioral/v6-game-editor-plan.md](docs/behavioral/v6-game-editor-plan.md) — Editor vision: Avalonia, MVVM, LLM integration, Lua scripting (2026-06-05)
- [docs/behavioral/v7-trigger-system.md](docs/behavioral/v7-trigger-system.md) — Trigger system: MapTrigger, Lua scripting, TriggerManager (2026-06-08)
- [docs/behavioral/demo-game.md](docs/behavioral/demo-game.md) — Demo game showcasing editor capabilities
- [.github/workflows/dotnet.yml](.github/workflows/dotnet.yml) — CI pipeline: build + test on push to main/master/wwii

## Operational (What Happened)

- [docs/operational/implementation-progress.md](docs/operational/implementation-progress.md) — Progress report (2026-05-09): WWII retheme ~70%, core systems ~85%, 234 tests passing

## Archive

- [docs/archive/engine-refactoring-status-log.md](docs/archive/engine-refactoring-status-log.md) — Engine refactoring daily log (2026-01-20 to 2026-01-23)
