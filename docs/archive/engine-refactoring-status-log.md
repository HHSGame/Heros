# Engine Refactoring Status Log

> Archived from `engine_refactoring.md`. Entries from 2026-01-20 to 2026-01-23.

## Status Log

- 2026-01-20: Plan created.
- 2026-01-20: Added test-script flexibility considerations (conditional/guarded steps).
- 2026-01-20: Confirmed support for both linear and conditional script modes.
- 2026-01-20: Added config models + loader (`Core/Engine/Config`, `GameConfigLoader`) and default `data/game.json` with unit tests.
- 2026-01-20: Added config mapper + config-driven bootstrap in `Program`, plus starting item/start position hooks.
- 2026-01-20: Added `GameEngineFactory` wrapper for config loading and parameter mapping.
- 2026-01-20: Added `--config` CLI handling to load `data/game.json` or override path.
- 2026-01-20: Added scripted input schema + loader + runner, plus input adapter for headless/UI execution.
- 2026-01-20: Added `--test-script` (optional `--test-step-ms`) to auto-run scripts with UI and skip the wizard.
- 2026-01-20: Added sample script `data/scripts/combat_toggle_smoke.json` and runner tests.
- 2026-01-20: Removed runtime map/enemy/item generators in favor of config-driven spawns and map loading (custom map or empty map).
- 2026-01-20: Externalized weapons/armors/items/classes/enemies into `data/catalogs/*.json` with catalog loader + runtime catalogs.
- 2026-01-20: Moved core service wiring into `GameEngine` and added a `BuildServiceProvider` helper for bootstrap.
- 2026-01-20: Removed enemy enum dependency; enemy IDs and abilities are now fully config-driven in `data/catalogs/enemies.json`.
- 2026-01-20: Added catalog loader coverage for enemy ability parsing.
- 2026-01-20: Introduced `GameEngineHost` to own Terminal UI startup and scripted runs.
- 2026-01-20: Added config-driven win/lose evaluation hooks for `PlayerDeath` and `AllEnemiesDefeated`.
- 2026-01-20: Added config-driven `TurnLimit` condition support plus validation for unknown or malformed condition entries.
- 2026-01-20: Consolidated CLI argument parsing and engine startup into `GameEngineLauncher`.
- 2026-01-20: Added parameterized conditions for `EnemyCountAtMost`, `EnemyCountAtLeast`, `HasItem`, and `ReachMarker` plus inventory item ids.
- 2026-01-22: Updated default `data/game.json` to use entry-based conditions with explicit map, item, and enemy spawns.
- 2026-01-22: Added a multi-character tactics scenario (`data/maps/tactics-01.txt`) and switched `data/game.json` to use `players` spawns.
- 2026-01-22: Added multi-player spawns, collision checks, and player-specific glyph/name support.
- 2026-01-22: Added active-player highlighting plus status-bar display for the selected character.
- 2026-01-22: Added planned-destination markers and planned-position-aware combat planning (move/attack/pickup).
- 2026-01-22: Added per-action rendering delays for player/enemy action execution to make turns visibly step through.
- 2026-01-22: Added victory messaging and combat cleanup on win/lose conditions.
- 2026-01-22: Added ranged targeting helpers with line/arc trajectories and projectile LOS rules (water no longer blocks).
- 2026-01-22: Added ranged target selection UI with attack range overlays in combat planning.
- 2026-01-22: Expanded weapon catalog with ranged/magic examples and trajectory metadata.
- 2026-01-22: Forced immediate per-action redraws via `Application.LayoutAndDraw` for clearer step-by-step animations.
- 2026-01-23: Added NPC/quest/dialogue config models, runtime managers, and party assist logic for dialogue checks.
- 2026-01-23: Added quest log and dialogue UI windows plus talk selection (T) and quest log toggle (Q, Ctrl+Q to quit).
- 2026-01-23: Added NPC/quest scenario in `data/game.json` with dialogues, quests, achievements, and a dedicated map.
- 2026-01-23: Assigned full 15-skill profiles to all classes for role-based starts.
- 2026-01-23: Added quest turn-in dialogue options, praise follow-ups, and completion gating updates in `data/game.json`.
- 2026-01-23: Allowed combined start/complete dialogue effects to display when a quest is inactive, enabling knowledge-based instant resolution.
- 2026-01-23: Added skill action definitions, targeting flows, and core execution hooks with initial tests for stealth and hidden-item interactions.
- 2026-01-23: Added selection-mode status bar overrides plus blinking target highlights for move/attack/skill/talk selections.
- 2026-01-23: Ensured map refresh after layout changes and cleared the map backbuffer each render to avoid blank/black screens when toggling UI windows.
- 2026-01-23: Added diagonal move support in movement planning with 1.5 AP rounding rules and updated pathfinding to include diagonal costs.
- 2026-01-23: Executed player action sequences in input order across all controlled characters (instead of per-character batching).
- 2026-01-23: Added currency and quest-status end conditions plus configurable win/lose messages.
- 2026-01-23: Moved Serilog setup into the engine launcher for a self-contained entrypoint.
- 2026-01-23: Added quest-objective and quest-ready end conditions with integration tests.
- 2026-01-23: Added achievement-unlocked end condition plus game.json example and tests.
- 2026-01-23: Expanded the default scenario with a larger map, a 3-member party, additional NPCs/quests, and updated spawns for manual verification.
