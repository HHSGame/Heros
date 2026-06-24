---
name: project-ink-md-initialized
description: INK.md reorganized 2026-06-12 — four-layer document index with docs/ restructured
metadata:
  type: project
---

INK.md reorganized at project root on 2026-06-12. Documentation restructured into four layers under `docs/`:

- **Conceptual** (3 docs): README.md, AGENTS.md, game-design-spec.md (updated: WWII setting, 8 factions)
- **Structural** (9 refs): Heros.sln, Directory.Build.props, 4 src projects, data/, demo-game/
- **Behavioral** (9 docs): save-load-system, engine-refactoring-plan, v2/v4/v5/v6/v7 plans, demo-game, CI
- **Operational** (1 doc): implementation-progress.md (2026-05-09)
- **Archive** (1 doc): engine-refactoring-status-log.md (Jan 2026)

Changes from cleanup:
- Deleted 6 files: TODO.md, src/HHSGame/README.md, docs/track/v4-wwii-retheme.md, docs/track/v3-game-refactoring.md, tools/Requirement.md, README.md (rewritten)
- Split engine_refactoring.md into behavioral + archive
- Updated game-design-spec.md: "post-apocalyptic" → "WWII occupied France", faction names updated
- Updated v6-game-editor-plan.md: Jint → Lua (NLua/MoonSharp) to match v7 and codebase
- Removed docs/track/ directory (all contents moved to docs/behavioral/)

**Why:** Previous docs were scattered, outdated, and contradictory. Four-layer structure enables task-relevant loading on session start.

**How to apply:** On session start, read INK.md and load only documents relevant to the current task. Conceptual = why, Structural = where, Behavioral = how, Operational = what happened.