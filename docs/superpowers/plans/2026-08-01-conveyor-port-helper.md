# Conveyor Port Placement Helper Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Pulsar client plugin that draws cyan outward arrows on every conveyor port of the active placement ghost so AQD/vanilla/modded conveyors are easy to orient before place.

**Architecture:** Each simulation/draw frame, if helpers are enabled and `MyCubeBuilder` has an active ghost, resolve `detector_conveyor*` model dummies from the cube definition’s model, convert dummy local positions into outward face normals (game rule: direction from dummy vs cell center), transform by the ghost world matrix, and debug-draw short arrows. No entities; visuals only.

**Tech Stack:** C# / .NET Framework 4.8 (+ net10 dual build from template), Pulsar, Harmony (template), SE client assemblies (`Sandbox.Game`, `VRage.*`), Krafs Publicizer for gizmo/internal access, NUnit or xUnit for pure-logic unit tests.

## Global Constraints

- Client-only Pulsar plugin for Space Engineers 1 (not SE2).
- Show helpers only while placing a ghost that has ≥1 conveyor port.
- Apply to any block with conveyor ports (vanilla + mods including AQD).
- Visual style: short outward arrows/cones; color cyan `#00FFFF`.
- Toggle hotkey default `Ctrl+Shift+P`; `Enabled` defaults true; persist via template `Config` / `ConfigStorage`.
- Never throw on missing/malformed mount/dummy data — skip port or block.
- No network traffic; do not change placement or conveyor connectivity.
- Do not run `dotnet build` / `dotnet test` / `dotnet restore` unless the user explicitly asks; plan steps list the commands for the human/engineer to run.
- Do not create extra README/docs beyond what the template already requires for Pulsar identity.
- Do not touch commented-out code unless a task explicitly says to uncomment a marked publicizer section.

---

## File structure

```
ClientPlugin/
  Plugin.cs                          # IPlugin entry; update/draw wiring
  Config.cs                          # Enabled + ToggleHotkey (+ optional color later out of scope)
  Logic/
    ConveyorPortInfo.cs              # readonly struct: LocalPosition, LocalNormal
    ConveyorPortMath.cs              # pure: dummy local pos → face normal (unit-tested)
    ConveyorPortResolver.cs          # definition/model → IReadOnlyList<ConveyorPortInfo>
  Placement/
    PlacementGhostInfo.cs            # Definition, WorldMatrix, GridSize, IsValid
    PlacementWatcher.cs              # reads MyCubeBuilder.Static → PlacementGhostInfo?
  Rendering/
    ArrowDrawer.cs                   # world-space cyan arrows from ports + ghost matrix
  Input/
    ToggleController.cs              # edge-detect hotkey; flips Config.Current.Enabled
ClientPlugin.Tests/
  ConveyorPortMathTests.cs           # pure math tests (no SE game process)
```

Template leftovers to remove after scaffold: example Harmony patches under `ClientPlugin/Patches/`, demo-only Config controls.

---

### Task 1: Scaffold Pulsar client plugin

**Files:**
- Create: entire tree from [se-client-plugin-template](https://github.com/viktor-ferenczi/se-client-plugin-template) into this repo (preserve existing `docs/superpowers/**`)
- Modify: names via `setup.py` → project name `ConveyorHelper`
- Modify: `ClientPluginTemplate.xml` / generated hub XML → plugin display name/description
- Delete after rename: unused example patch files once verified not required by build

**Interfaces:**
- Consumes: none
- Produces: loadable Pulsar plugin named `ConveyorHelper` with `Plugin.Name == "ConveyorHelper"`

- [ ] **Step 1: Clone template contents into the workspace**

From a temp dir (or sparse checkout), copy template files into `c:\Users\user\Documents\SE_ConveyorHelperPlugin`, **without** overwriting `docs/superpowers/`. Keep `.gitignore` from template if none exists.

```powershell
# Example approach — adjust if using gh/git clone of the template
git clone --depth 1 https://github.com/viktor-ferenczi/se-client-plugin-template.git "$env:TEMP\se-client-plugin-template"
robocopy "$env:TEMP\se-client-plugin-template" "c:\Users\user\Documents\SE_ConveyorHelperPlugin" /E /XD .git docs
```

- [ ] **Step 2: Run setup**

```powershell
cd c:\Users\user\Documents\SE_ConveyorHelperPlugin
py -3.12 setup.py
# When prompted: ConveyorHelper
# Accept auto-detected SE/Pulsar paths or enter manually
```

Expected: solution/project renamed; `Directory.Build.props` created locally (not committed).

- [ ] **Step 3: Trim example patches**

Delete `ClientPlugin/Patches/ExamplePrefixPostfixPatch.cs`, `ExampleTranspilerPatch.cs`, and the `.il` example files if present. Leave `Patches/` folder empty or remove it if csproj does not require it.

- [ ] **Step 4: Set plugin identity**

In `Plugin.cs`, ensure:

```csharp
public const string Name = "ConveyorHelper";
```

Update PluginHub XML title/description to: “Draws conveyor port direction arrows on the placement ghost.”

- [ ] **Step 5: Human build & Pulsar smoke test**

Run (user explicitly):

```powershell
dotnet build ClientPlugin\ClientPlugin.csproj -c Debug
```

Enable the plugin in Pulsar; game should load with no errors. No arrows yet.

- [ ] **Step 6: Commit** (only if user asked to commit)

```bash
git add -A
git commit -m "chore: scaffold ConveyorHelper Pulsar client plugin from template"
```

---

### Task 2: Config — Enabled + toggle hotkey

**Files:**
- Modify: `ClientPlugin/Config.cs`
- Test: manual via Pulsar config dialog after build

**Interfaces:**
- Consumes: template `Binding`, `ConfigStorage`, settings attributes
- Produces:
  - `Config.Current.Enabled : bool` (default `true`)
  - `Config.Current.ToggleHotkey : Binding` (default Ctrl+Shift+P)
  - `Config.Current.Title => "Conveyor Helper"`

- [ ] **Step 1: Replace demo config with v1 options**

Replace `Config.cs` option/UI regions with:

```csharp
public class Config : INotifyPropertyChanged
{
    private bool enabled = true;
    private Binding toggleHotkey = new Binding(MyKeys.P, shift: true, ctrl: true, alt: false);

    public readonly string Title = "Conveyor Helper";

    [Checkbox(description: "Show conveyor port arrows while placing blocks")]
    public bool Enabled
    {
        get => enabled;
        set => SetField(ref enabled, value);
    }

    [Keybind(description: "Toggle port arrows — right-click button to unbind")]
    public Binding ToggleHotkey
    {
        get => toggleHotkey;
        set => SetField(ref toggleHotkey, value);
    }

    // Keep Default / Current / INotifyPropertyChanged boilerplate from template
}
```

Adjust `Binding` constructor to match the template’s actual `Binding` API in `ClientPlugin/Settings/Tools/Binding.cs` (field names may differ — use that type’s real ctor/properties for Ctrl+Shift+P).

- [ ] **Step 2: Persist check**

Confirm `ConfigStorage.Load()` still deserializes the reduced shape (missing old demo fields should just use defaults).

- [ ] **Step 3: Human verify**

Build (user), open plugin config in-game: title “Conveyor Helper”, checkbox + keybind only.

- [ ] **Step 4: Commit** (only if user asked)

```bash
git add ClientPlugin/Config.cs
git commit -m "feat: add Enabled and ToggleHotkey config"
```

---

### Task 3: Pure port-direction math (TDD)

**Files:**
- Create: `ClientPlugin/Logic/ConveyorPortMath.cs`
- Create: `ClientPlugin.Tests/ClientPlugin.Tests.csproj`
- Create: `ClientPlugin.Tests/ConveyorPortMathTests.cs`
- Modify: `ConveyorHelper.sln` (or renamed sln) to include test project

**Interfaces:**
- Consumes: none (no SE references in math)
- Produces:
  - `public static class ConveyorPortMath`
  - `public static Vector3 GetOutwardNormal(Vector3 localDummyPosition, float gridSize)`
  - `public static bool IsConveyorDummyName(string dummyName)`

**Algorithm (game-consistent):**
1. Reject null/empty names; conveyor dummies start with `detector_conveyor` (case-insensitive).
2. Given local dummy position and `gridSize` (meters per cell: 2.5 large, 0.5 small):
   - `cell = Floor(localPos / gridSize)` component-wise (handle negative coords with proper floor).
   - `cellCenter = (cell + 0.5f) * gridSize`
   - `delta = localPos - cellCenter`
   - outward normal = unit vector on the dominant axis of `delta` (sign preserved). If `|delta|` near zero, return `Vector3.Zero` (caller skips).

- [ ] **Step 1: Add test project without game refs**

`ClientPlugin.Tests.csproj` targets `net48` or `net8.0`, references `VRage.Math` **only if** already available as a NuGet/local ref; otherwise use a tiny local `Vector3` stand-in **or** reference the same `VRage.Math.dll` from SE `Bin64` as a hint-path Reference (same as game plugin). Prefer referencing `VRage.Math` from Bin64 for signature parity.

- [ ] **Step 2: Write failing tests**

```csharp
using NUnit.Framework;
using VRageMath;

[TestFixture]
public class ConveyorPortMathTests
{
    [TestCase("detector_conveyor", true)]
    [TestCase("detector_conveyorline_small_001", true)]
    [TestCase("detector_terminal", false)]
    [TestCase("mount", false)]
    public void IsConveyorDummyName_filters(string name, bool expected)
    {
        Assert.AreEqual(expected, ConveyorPortMath.IsConveyorDummyName(name));
    }

    [Test]
    public void GetOutwardNormal_points_along_dominant_axis()
    {
        // Dummy near +X face of cell (0,0,0) on large grid
        float g = 2.5f;
        var pos = new Vector3(g - 0.1f, g * 0.5f, g * 0.5f);
        var n = ConveyorPortMath.GetOutwardNormal(pos, g);
        Assert.That(n.X, Is.EqualTo(1f).Within(0.01f));
        Assert.That(n.Y, Is.EqualTo(0f).Within(0.01f));
        Assert.That(n.Z, Is.EqualTo(0f).Within(0.01f));
    }

    [Test]
    public void GetOutwardNormal_zero_when_at_cell_center()
    {
        float g = 0.5f;
        var pos = new Vector3(g * 0.5f, g * 0.5f, g * 0.5f);
        var n = ConveyorPortMath.GetOutwardNormal(pos, g);
        Assert.That(n.LengthSquared(), Is.EqualTo(0f).Within(1e-6f));
    }
}
```

- [ ] **Step 3: Run tests (user) — expect FAIL**

```powershell
dotnet test ClientPlugin.Tests\ClientPlugin.Tests.csproj -c Debug
```

Expected: FAIL (type/method missing).

- [ ] **Step 4: Implement math**

```csharp
namespace ClientPlugin.Logic;

public static class ConveyorPortMath
{
    public static bool IsConveyorDummyName(string dummyName)
    {
        if (string.IsNullOrEmpty(dummyName)) return false;
        return dummyName.StartsWith("detector_conveyor", System.StringComparison.OrdinalIgnoreCase);
    }

    public static VRageMath.Vector3 GetOutwardNormal(VRageMath.Vector3 localDummyPosition, float gridSize)
    {
        if (gridSize <= 0f) return VRageMath.Vector3.Zero;

        var cell = new VRageMath.Vector3I(
            (int)System.Math.Floor(localDummyPosition.X / gridSize),
            (int)System.Math.Floor(localDummyPosition.Y / gridSize),
            (int)System.Math.Floor(localDummyPosition.Z / gridSize));

        var cellCenter = new VRageMath.Vector3(
            (cell.X + 0.5f) * gridSize,
            (cell.Y + 0.5f) * gridSize,
            (cell.Z + 0.5f) * gridSize);

        var delta = localDummyPosition - cellCenter;
        float ax = System.Math.Abs(delta.X);
        float ay = System.Math.Abs(delta.Y);
        float az = System.Math.Abs(delta.Z);
        const float eps = 1e-5f;
        if (ax < eps && ay < eps && az < eps)
            return VRageMath.Vector3.Zero;

        if (ax >= ay && ax >= az)
            return new VRageMath.Vector3(System.Math.Sign(delta.X), 0f, 0f);
        if (ay >= ax && ay >= az)
            return new VRageMath.Vector3(0f, System.Math.Sign(delta.Y), 0f);
        return new VRageMath.Vector3(0f, 0f, System.Math.Sign(delta.Z));
    }
}
```

- [ ] **Step 5: Run tests (user) — expect PASS**

```powershell
dotnet test ClientPlugin.Tests\ClientPlugin.Tests.csproj -c Debug
```

- [ ] **Step 6: Commit** (only if user asked)

```bash
git add ClientPlugin/Logic/ConveyorPortMath.cs ClientPlugin.Tests ConveyorHelper.sln
git commit -m "feat: add conveyor dummy name and outward-normal math"
```

---

### Task 4: Port resolver (definition → local ports)

**Files:**
- Create: `ClientPlugin/Logic/ConveyorPortInfo.cs`
- Create: `ClientPlugin/Logic/ConveyorPortResolver.cs`

**Interfaces:**
- Consumes: `ConveyorPortMath.*`
- Produces:
  - `readonly struct ConveyorPortInfo { Vector3 LocalPosition; Vector3 LocalNormal; }`
  - `static class ConveyorPortResolver`
  - `static IReadOnlyList<ConveyorPortInfo> Resolve(MyCubeBlockDefinition def)` — empty list if none; never throws

- [ ] **Step 1: Define DTO**

```csharp
namespace ClientPlugin.Logic;

public readonly struct ConveyorPortInfo
{
    public ConveyorPortInfo(VRageMath.Vector3 localPosition, VRageMath.Vector3 localNormal)
    {
        LocalPosition = localPosition;
        LocalNormal = localNormal;
    }

    public VRageMath.Vector3 LocalPosition { get; }
    public VRageMath.Vector3 LocalNormal { get; }
}
```

- [ ] **Step 2: Implement resolver with cache**

Use the installed game’s model API. Canonical pattern:

```csharp
using System;
using System.Collections.Generic;
using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Models;
using VRageMath;

namespace ClientPlugin.Logic;

public static class ConveyorPortResolver
{
    private static readonly Dictionary<MyDefinitionId, IReadOnlyList<ConveyorPortInfo>> Cache =
        new Dictionary<MyDefinitionId, IReadOnlyList<ConveyorPortInfo>>();

    public static IReadOnlyList<ConveyorPortInfo> Resolve(MyCubeBlockDefinition def)
    {
        if (def == null)
            return Array.Empty<ConveyorPortInfo>();

        if (Cache.TryGetValue(def.Id, out var cached))
            return cached;

        var result = new List<ConveyorPortInfo>();
        try
        {
            float gridSize = def.CubeSize == MyCubeSize.Large ? 2.5f : 0.5f;
            // Prefer GetModelOnlyDummies if present on MyModels; else GetModel(def.Model).GetDummies
            var model = MyModels.GetModelOnlyDummies(def.Model);
            if (model == null)
            {
                Cache[def.Id] = Array.Empty<ConveyorPortInfo>();
                return Cache[def.Id];
            }

            var dummies = new Dictionary<string, IMyModelDummy>();
            model.GetDummies(dummies);
            foreach (var kv in dummies)
            {
                if (!ConveyorPortMath.IsConveyorDummyName(kv.Key))
                    continue;
                var localPos = kv.Value.Matrix.Translation;
                var n = ConveyorPortMath.GetOutwardNormal(localPos, gridSize);
                if (n.LengthSquared() < 1e-6f)
                    continue;
                result.Add(new ConveyorPortInfo(localPos, n));
            }
        }
        catch
        {
            // visuals must never break placement
        }

        Cache[def.Id] = result;
        return result;
    }
}
```

If `GetModelOnlyDummies` / `IMyModelDummy` names differ in the installed build, retarget to the equivalent symbols from `VRage.Game.Models` / `VRage.Game.ModAPI` — keep behavior identical.

- [ ] **Step 4: Human in-game sanity (after Task 6 wiring)** — skip full game test until drawer exists; unit-level: resolve vanilla conveyor definition in debugger when available.

- [ ] **Step 5: Commit** (only if user asked)

```bash
git add ClientPlugin/Logic/ConveyorPortInfo.cs ClientPlugin/Logic/ConveyorPortResolver.cs
git commit -m "feat: resolve conveyor ports from block model dummies"
```

---

### Task 5: Placement watcher (ghost pose)

**Files:**
- Create: `ClientPlugin/Placement/PlacementGhostInfo.cs`
- Create: `ClientPlugin/Placement/PlacementWatcher.cs`
- Modify: `ClientPlugin/Tools/GameAssembliesToPublicize.cs` + uncomment publicizer sections marked in template **only as needed** for `MyCubeBuilder` / gizmo matrix access

**Interfaces:**
- Consumes: `MyCubeBuilder.Static`
- Produces:
  - `readonly struct PlacementGhostInfo` with `MyCubeBlockDefinition Definition`, `MatrixD WorldMatrix`, `float GridSizeMeters`, `bool IsValid`
  - `static class PlacementWatcher`
  - `static bool TryGetGhost(out PlacementGhostInfo ghost)`

- [ ] **Step 1: Enable publicizer for required assemblies**

Follow template comments “Uncomment to enable publicizer support”. List at least `Sandbox.Game` in `GameAssembliesToPublicize.cs`.

- [ ] **Step 2: Implement TryGetGhost**

```csharp
public static bool TryGetGhost(out PlacementGhostInfo ghost)
{
    ghost = default;
    try
    {
        var builder = MyCubeBuilder.Static;
        if (builder == null || !builder.IsActivated)
            return false;

        var def = builder.CurrentBlockDefinition;
        if (def == null)
            return false;

        // Obtain ghost world matrix from builder gizmo / render data.
        // Inspect with publicizer: MyCubeBuilder.m_gizmo (MyCubeBuilderGizmo)
        // typical members: SpaceMatrix / m_spaceMatrix / GetRotationMatrix + position.
        // Fallback: GetBuildBoundingBox().Center + orientation from CubeBuilderState.
        MatrixD worldMatrix = /* resolved matrix */;
        float gridSize = def.CubeSize == MyCubeSize.Large ? 2.5f : 0.5f;

        ghost = new PlacementGhostInfo(def, worldMatrix, gridSize);
        return true;
    }
    catch
    {
        return false;
    }
}
```

**Agent must:** After publicizer build, open `MyCubeBuilder` / `MyCubeBuilderGizmo` in decompiler or IDE metadata and wire the real matrix field/property. Document the chosen member in a one-line comment above the access.

- [ ] **Step 3: Commit** (only if user asked)

```bash
git add ClientPlugin/Placement ClientPlugin/Tools/GameAssembliesToPublicize.cs ClientPlugin/ClientPlugin.csproj
git commit -m "feat: read active placement ghost pose from MyCubeBuilder"
```

---

### Task 6: Arrow drawer + plugin frame loop

**Files:**
- Create: `ClientPlugin/Rendering/ArrowDrawer.cs`
- Create: `ClientPlugin/Input/ToggleController.cs`
- Modify: `ClientPlugin/Plugin.cs`

**Interfaces:**
- Consumes: `Config.Current`, `PlacementWatcher.TryGetGhost`, `ConveyorPortResolver.Resolve`, `ConveyorPortInfo`
- Produces:
  - `ArrowDrawer.Draw(in PlacementGhostInfo ghost, IReadOnlyList<ConveyorPortInfo> ports)`
  - `ToggleController.Update()` — toggles `Config.Current.Enabled` on hotkey edge
  - `Plugin.Update` / draw hook calls the above when enabled

- [ ] **Step 1: Toggle controller**

```csharp
public static class ToggleController
{
    private static bool _wasDown;

    public static void Update()
    {
        var binding = Config.Current.ToggleHotkey;
        bool down = binding != null && binding.IsPressed(/* use Binding API: IsKeypress / matches MyInput */);
        if (down && !_wasDown)
        {
            Config.Current.Enabled = !Config.Current.Enabled;
            // ConfigStorage.Save if template requires explicit save on change
        }
        _wasDown = down;
    }
}
```

Match the template `Binding` press-check API exactly.

- [ ] **Step 2: Arrow drawer**

```csharp
public static class ArrowDrawer
{
    private static readonly Color Cyan = new Color(0, 255, 255, 255);

    public static void Draw(in PlacementGhostInfo ghost, IReadOnlyList<ConveyorPortInfo> ports)
    {
        if (ports == null || ports.Count == 0) return;

        float length = ghost.GridSizeMeters * 0.35f; // ~0.175 small / ~0.875 large — clamp to ~0.3–0.5 feel on large
        length = MathHelper.Clamp(length, 0.15f, 0.5f);

        var world = ghost.WorldMatrix;
        foreach (var port in ports)
        {
            var startLocal = port.LocalPosition;
            var endLocal = port.LocalPosition + port.LocalNormal * length;
            var start = Vector3D.Transform(startLocal, world);
            var end = Vector3D.Transform(endLocal, world);
            MyRenderProxy.DebugDrawArrow3D(start, end, Cyan, Cyan, false);
            // If DebugDrawArrow3D unavailable, use DebugDrawLine3D + small tip lines
        }
    }
}
```

Confirm `MyRenderProxy` namespace (`VRageRender`) on the installed build.

- [ ] **Step 3: Wire Plugin**

```csharp
public void Update()
{
    ToggleController.Update();
}

// Prefer draw on last frame / Draw if IPlugin supports it; else draw at end of Update.
// Many SE plugins call debug draw from Update successfully.
public void Draw() // if available on IPlugin in this SE/Pulsar version
{
    DrawHelpers();
}

private static void DrawHelpers()
{
    if (!Config.Current.Enabled) return;
    if (!PlacementWatcher.TryGetGhost(out var ghost)) return;

    var ports = ConveyorPortResolver.Resolve(ghost.Definition);
    if (ports.Count == 0) return;

    ArrowDrawer.Draw(in ghost, ports);
}
```

If `IPlugin` has no `Draw`, call `DrawHelpers()` from `Update()`.

- [ ] **Step 4: Human manual test (spec checklist)**

1. Vanilla conveyor tube / junction — arrows on known faces.
2. AQD armored T / corner / X — readable while rotating.
3. Hotkey toggles off/on.
4. Non-conveyor block — no arrows.
5. Small + large grid — readable length.

- [ ] **Step 5: Commit** (only if user asked)

```bash
git add ClientPlugin/Plugin.cs ClientPlugin/Rendering ClientPlugin/Input
git commit -m "feat: draw cyan conveyor port arrows on placement ghost"
```

---

### Task 7: Hardening + release polish

**Files:**
- Modify: `ClientPlugin/Logic/ConveyorPortResolver.cs` (cache invalidation if needed)
- Modify: `Version.Build.props` → `1.0.0`
- Modify: PluginHub XML description/tags
- Modify: remove leftover demo button/enum from Config if any remain

**Interfaces:**
- Consumes: all prior
- Produces: Release-ready plugin identity v1.0.0

- [ ] **Step 1: Guardrails pass**

- All public entrypoints wrapped so exceptions never escape `Update`/`Draw`.
- Cache keyed by definition id; no per-frame model parse after first resolve.
- Confirm no Harmony patches remain unless required.

- [ ] **Step 2: Version**

Set `Version.Build.props` to `1.0.0` (or template’s version property format).

- [ ] **Step 3: Human Release build test**

```powershell
dotnet build ClientPlugin\ClientPlugin.csproj -c Release
```

Retest AQD T-junction place + toggle.

- [ ] **Step 4: Commit** (only if user asked)

```bash
git add Version.Build.props ClientPlugin ConveyorHelper.xml
git commit -m "chore: harden ConveyorHelper and mark v1.0.0"
```

---

## Spec coverage check

| Spec requirement | Task |
|------------------|------|
| Pulsar SE1 client plugin | 1 |
| Ghost-only visualization | 5, 6 |
| Any block with conveyor ports | 3, 4 |
| Outward arrows, cyan | 6 |
| Live with rotation | 5, 6 (matrix each frame) |
| Toggle hotkey, default on | 2, 6 |
| Skip bad data, no throw | 4, 5, 6 |
| MP client-only / no net | 6 (draw only) |
| Manual AQD/vanilla tests | 6 Step 4 |
| Out of scope excluded | — |

## Placeholder / API resolution notes (not TODOs — agent actions)

During Tasks 4–6 the implementer must bind these to the **installed** SE binaries (names drift by game version):

1. Dummy enumeration API (`GetDummies` / `GetModelOnlyDummies`).
2. Ghost world matrix member on `MyCubeBuilderGizmo`.
3. `Binding` pressed state API.
4. `DebugDrawArrow3D` vs line fallback.
5. Whether `IPlugin.Draw` exists; else `Update`.
