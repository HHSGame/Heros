# Engine Refactoring Plan

## Goal
Split HHSGame into a data-driven engine that loads a game configuration file to determine maps, player state, combatants, items, effects, and end conditions. Add a scripted test mode that can drive Terminal UI and run in `HHSGameTest` for assertions.

## Scope Summary
- Engine layer builds a game from config (default `data/game.json`).
- Config file controls: map data, player state, weapons, enemies, classes, items, effects, victory/defeat conditions, start/end rules.
- CLI or engine parameter selects config file.
- Test mode accepts an input sequence/script and validates resulting game state.

## Proposed Architecture
### Engine Layer (new)
- `Core/Engine/GameEngine` (or similar): bootstrap from config, own runtime state, provide hooks for UI and test runner.
- `Core/Engine/Config/*`: config models and validation.
- `Core/Engine/ScenarioRunner`: apply scripted inputs and expose state snapshots.

### UI Integration
- `Program` loads config, builds engine, starts Terminal UI.
- UI delegates to engine’s input handler to apply actions.

### Test Mode
- Script format (JSON or list of key tokens) to drive a sequence.
- Runner executes script in UI or headless.
- Assertions in `HHSGameTest` verify state transitions (combat on/off, positions, game end state).

## Config Schema (Draft)
- `game`: name, version.
- `map`: custom map path or empty-map dimensions.
- `player`: attributes, skills, starting items, start position.
- `catalogs`: external definitions for classes, enemies, weapons, armors, items.
- `classes`: class definitions and bonuses.
- `items`: item definitions with positions.
- `weapons`: damage, ap cost, type, penetration.
- `enemies`: enemy definitions with spawn positions.
- `effects`: active effects and durations.
- `conditions`: win/lose rules (player death, goal reached, turn limit, etc).

## Test Script Format (Draft)
- Simple: `{ "inputs": ["C", "M", "Right", "Enter", "Enter", "C", "Q"] }` or top-level string array.
- Conditional steps: `steps` list with `waitUntil`, `when` (`then`/`else`), `repeat` (`while`, `steps`), and `assert`.

## Test Script Considerations
Linear scripts are simple but brittle for interactive games. If we need to decide the next input based on game state, add conditional/guarded steps:
- `wait_until`: block until a state condition is true (with timeout).
- `when`: conditional branch based on state (e.g., combat active, enemy visible).
- `repeat`: loop with max iterations while a condition holds.
- `assert`: in-script assertions to verify state at checkpoints.
- `target`: resolve selectors (nearest enemy, adjacent tile, visible tile) instead of fixed coordinates.

Decision: keep both a minimal linear script format (for quick smoke flows) and an extended conditional format (for robust interaction tests).

## Milestones
1. Define config models and loader, add `data/game.json`.
2. Implement engine bootstrap using config.
3. Wire CLI/config override.
4. Add scripted input runner (UI + headless).
5. Add first scripted test in `HHSGameTest`.

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

## Next Actions
- Add more condition types (currency, quest flags) and expose end-state messaging.
- Consider moving Serilog setup into the launcher for a fully self-contained engine entrypoint.
