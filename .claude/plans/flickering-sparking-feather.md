# Documentation Reorganization Plan

## Context

HHS project has 17 documentation files scattered across root, docs/, docs/track/, src/, tools/, and demo-game/. Many are outdated, contradict each other, or contain stale operational data. Need to clean up, archive old operational content, and reorganize into a four-layer structure (Conceptual/Structural/Behavioral/Operational).

## Step 1: Create directory structure

```
mkdir -p docs/conceptual docs/structural docs/behavioral docs/operational docs/archive
```

## Step 2: Delete 6 outdated/invalid files

| File | Reason |
|------|--------|
| `README.md` | 2 lines, says "modern settings" (wrong), rewrite from scratch |
| `src/HHSGame/README.md` | Says "RPG Game", typo "terrian", no WWII mention |
| `docs/track/v4-wwii-retheme.md` | Fully superseded by detailed `docs/track/v4` |
| `docs/track/v3-game-refactoring.md` | Completed plan, outcome captured in v2 |
| `tools/Requirement.md` | HTML/JS tilemap editor, unrelated to C# project |
| `TODO.md` | Massively outdated, contradicts v2/Implementation_Progress on save/load, equipment, item rarity; contains AI artifacts |

## Step 3: Move surviving docs to new structure

| From | To |
|------|-----|
| `docs/Stats_Specification.md` | `docs/conceptual/game-design-spec.md` |
| `docs/track/v1-save-load.md` | `docs/behavioral/save-load-system.md` |
| `docs/track/v2-improvement-plan.md` | `docs/behavioral/v2-improvement-plan.md` |
| `docs/track/v4` | `docs/behavioral/v4-wwii-retheme-status.md` |
| `docs/track/v5-codebase-improvement-plan.md` | `docs/behavioral/v5-codebase-improvement-plan.md` |
| `docs/track/v6-game-editor-plan.md` | `docs/behavioral/v6-game-editor-plan.md` |
| `docs/track/v7-trigger-system.md` | `docs/behavioral/v7-trigger-system.md` |
| `docs/Implementation_Progress_and_Plan.md` | `docs/operational/implementation-progress.md` |

## Step 4: Split engine_refactoring.md

- Lines 1-58 (design plan) + Lines 109-147 (progress audit, updated) → `docs/behavioral/engine-refactoring-plan.md`
- Lines 60-108 (Jan 2026 status log, >3mo old) → `docs/archive/engine-refactoring-status-log.md`
- Delete original `engine_refactoring.md`

## Step 5: Content updates

- `game-design-spec.md`: "post-apocalyptic" → "WWII occupied France"; update faction names
- `v6-game-editor-plan.md`: Replace Jint recommendation with Lua to match v7 and codebase
- `engine-refactoring-plan.md`: Mark Save/Load milestone as COMPLETE

## Step 6: Rewrite root files

- `README.md`: Full rewrite with WWII theme, tech stack, quick start, solution structure
- `AGENTS.md`: Add WWII context, HHSEditor projects, updated directory structure
- `INK.md`: Complete rewrite reflecting new four-layer structure

## Step 7: Clean up empty docs/track/ directory

## Verification

- All paths in INK.md resolve to existing files
- No references to deleted files remain
- `dotnet build Heros.sln` still works
