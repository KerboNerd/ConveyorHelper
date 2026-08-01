# Conveyor Port Placement Helper — Design Spec

**Date:** 2026-08-01  
**Status:** Approved for planning  
**Platform:** Space Engineers 1 (client) via Pulsar

## Problem

When placing conveyor blocks — especially AQD armored conveyors (T junctions, corners, X junctions, etc.) — port directions are hard to read from the ghost mesh alone. Wrong rotation is common and costly to fix after welding.

## Goal

Client-only Pulsar plugin that draws clear outward arrows on every conveyor port of the active placement ghost so the player can orient the block correctly before placing.

## Non-goals (v1)

- Overlays on already-built blocks
- Neighbor connection / network preview
- Custom meshes or per-mod visual packs
- Changing placement rules or connectivity
- Space Engineers 2

## Approach

**Ghost-hook + debug draw:** Each frame while placing, resolve the ghost cube definition’s conveyor mount points, transform them by the ghost world matrix, and draw short arrows along port normals. No entities spawned.

## Architecture

| Component | Responsibility |
|-----------|----------------|
| Plugin entry | Init/dispose; register update/draw; load/save config |
| Placement watcher | Detect local player block-place mode + valid ghost (definition, pose) |
| Port resolver | Read conveyor mounts from cube definition; local → world transforms |
| Arrow drawer | Draw short cone/arrow outward from each port |
| Input / config | Toggle hotkey; persist enabled state |

```
Place mode → ghost block → cube def mounts → world transforms → draw (if enabled)
```

## Behavior

- Show **only** while a placeable ghost exists and the definition has ≥1 conveyor port.
- Apply to **any** block with conveyor ports (vanilla + mods, including AQD).
- Arrows update live with ghost position and rotation.
- One visual style: short outward arrow/cone per port.
- Single color in v1: high-contrast cyan (`#00FFFF`).
- No input/output distinction (conveyor ports are bidirectional).
- Arrow length ~0.3–0.5 m, scaled for small vs large grid.
- Hide when: toggle off, not placing, invalid ghost, or no ports.

## Configuration (v1)

| Key | Type | Default | Notes |
|-----|------|---------|-------|
| `Enabled` | bool | `true` | Master toggle |
| `ToggleHotkey` | key combo | `Ctrl+Shift+P` | Change if conflict found in game |

Persist with the client plugin template’s standard config pattern.

## Edge cases

- Missing or malformed mount data on a modded block: skip that port (or the block); never throw.
- Multiplayer: client-only drawing; no network messages.
- Creative and survival place modes behave the same.
- Visual-only: does not alter game placement or conveyor logic.

## Testing (manual)

1. Vanilla straight / junction conveyors — arrows match known port faces.
2. AQD T / corner / X armored — arrows readable while rotating the ghost.
3. Toggle off — nothing drawn; toggle on — restored.
4. Block with no conveyor ports — nothing drawn.
5. Small and large grid — length/scale still readable.

## Project shape

- Greenfield repo based on [SE client plugin template](https://github.com/viktor-ferenczi/se-client-plugin-template) for Pulsar.
- C# / .NET Framework targeting SE1 client assemblies.
- Deliverable: plugin DLL loadable via Pulsar (dev folder / PluginHub registration as needed).

## Success criteria

While placing an AQD armored conveyor (or any ported block), the player can tell every open port direction from the arrows alone, without guessing from the mesh, and can disable the helper with one hotkey.
