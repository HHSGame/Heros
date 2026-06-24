---
name: palest-ink
description: "A memoryless document architecture. Use whenever working in a project with an INK.md file, or when the user wants to organize project docs into explicit inspectable layers, set up project documentation structure, or adopt a memoryless approach to project knowledge. Also triggers on mentions of INK.md, four-layer docs, or disabling implicit memory."
license: MIT
metadata:
  author: kenpusney
  version: "0.1.0"
---

# Palest Ink

The palest ink is better than the strongest memory. 好记性不如烂笔头。

Disable implicit memory systems — Mem0, MemPalace, etc.

Use INK.md (project root or .agents/) to map project documents into four
layers. See assets/INK.md for the template. Opt-in: if INK.md doesn't
exist, do nothing unless the user asks to adopt it.

On session start, read INK.md and load only task-relevant documents.

Classification — what question does the document answer?
- Conceptual: why (vision, decisions, constraints)
- Structural: where (layout, modules, schemas)
- Behavioral: how (conventions, workflows, rules)
- Operational: what happened (logs, changelogs, debug notes)

Write discoveries to the appropriate layer's paths. Write operational
entries whenever operational work is done. Never modify INK.md without
user approval.
