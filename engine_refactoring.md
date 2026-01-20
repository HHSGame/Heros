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
- `map`: style, width, height, or custom map path.
- `player`: attributes, skills, starting items, start position.
- `classes`: class definitions and bonuses.
- `items`: item definitions (type, stats, effects).
- `weapons`: damage, ap cost, type, penetration.
- `enemies`: enemy definitions, spawns.
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
- 2025-01-20: Plan created.
- 2025-01-20: Added test-script flexibility considerations (conditional/guarded steps).
- 2025-01-20: Confirmed support for both linear and conditional script modes.
- 2025-01-20: Added config models + loader (`Core/Engine/Config`, `GameConfigLoader`) and default `data/game.json` with unit tests.
- 2025-01-20: Added config mapper + config-driven bootstrap in `Program`, plus starting item/start position hooks.
- 2025-01-20: Added `GameEngineFactory` wrapper for config loading and parameter mapping.
- 2025-01-20: Added `--config` CLI handling to load `data/game.json` or override path.
- 2025-01-20: Added scripted input schema + loader + runner, plus input adapter for headless/UI execution.
- 2025-01-20: Added `--test-script` (optional `--test-step-ms`) to auto-run scripts with UI and skip the wizard.
- 2025-01-20: Added sample script `data/scripts/combat_toggle_smoke.json` and runner tests.

## Next Actions
- Continue Milestone 2: finish engine bootstrap so `GameEngine` builds the full `GameContext` and owns startup/shutdown.
- Expand config-driven win/lose conditions and move `Program` boot logic into engine services.
