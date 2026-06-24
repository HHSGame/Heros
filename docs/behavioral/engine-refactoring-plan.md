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
- UI delegates to engine's input handler to apply actions.

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
1. Define config models and loader, add `data/game.json`. ✅
2. Implement engine bootstrap using config. ✅
3. Wire CLI/config override. ✅
4. Add scripted input runner (UI + headless). ✅
5. Add first scripted test in `HHSGameTest`. ✅
6. Save/Load integration. ✅ (completed 2026-05-08, 33 tests)
7. Test scripted flow. ⚠️ (partial)

## Progress Audit (2026-05-08, updated)

### Overall Status
- Engine refactoring milestones 1-10: **Complete** ✅
- Milestone 11 (Save/Load integration): **Complete** ✅ (SaveManager + LoadManager + SaveLoadSlotDialog UI + 33 tests)
- Milestone 12 (Test scripted flow): **Partial** ⚠️

### Save/Load — Resolved
The SaveManager API issues identified in the initial audit (missing `GetTurnNumber()`, `GetTalkedNpcs()`, etc.) have been resolved. The full save/load system is implemented with:
- SaveManager + LoadManager
- SaveLoadSlotDialog UI (Ctrl+S/Ctrl+L)
- 3 unit test files covering round-trip serialization
- See [save-load-system.md](save-load-system.md) for the original design spec

### Remaining Work
- Phase-based TurnManager refactoring
- Equipment slots + item rarity (completed per v2 improvement plan)
- Interactive objects framework (completed per v2 improvement plan)
