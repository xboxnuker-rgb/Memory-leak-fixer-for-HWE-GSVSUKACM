# Storage Material Leak Fix

A focused Schedule I stability patch that stops live weed-storage visuals from creating disposable Unity material instances on every refresh.

[![Schedule I](https://img.shields.io/badge/Schedule_I-0.4.6f13-8a2be2?style=for-the-badge)](https://store.steampowered.com/app/3164500/Schedule_I/)
[![Backend](https://img.shields.io/badge/backend-IL2CPP-222222?style=for-the-badge)](#requirements-and-compatibility)
[![MelonLoader](https://img.shields.io/badge/MelonLoader-0.7.0-00b894?style=for-the-badge)](https://melonwiki.xyz/)
[![License](https://img.shields.io/badge/license-MIT-blue?style=for-the-badge)](LICENSE)
[![Support on Patreon](https://img.shields.io/badge/Support_on-Patreon-FF424D?style=for-the-badge&logo=patreon&logoColor=white)](https://www.patreon.com/cw/GSVS_UK_ACM/shop)

> **Release status:** v1.1.1 is completing its heavy-save soak test. The installable release will be linked here after the full lifecycle test passes. Do not use the experimental 1.0.x or 1.1.0 builds.

## The problem

Long automated sessions can end in a hard desktop crash with no useful managed exception. Reproduced failures reached approximately **41–42 GiB of committed memory** before `UnityPlayer.dll` failed inside its dynamic heap allocator.

Symbol-resolved native dumps and recovered IL2CPP method bodies identified this path:

```text
StorageVisualizer.RefreshVisuals
→ WeedVisualsSetter.ApplyVisuals
→ Renderer.materials / Renderer.GetMaterials
→ Material::GetInstantiatedMaterial
→ DynamicHeapAllocator::CreateTLSFBlock
```

`Renderer.materials` is an instantiating Unity API. Schedule I retrieves those copies, replaces their references with the product-definition materials, and never releases the discarded instances. A busy shelf can repeat that process thousands of times.

Harder Working Employees and similar automation mods make the issue appear sooner because they move more items, but the leaking render path belongs to the base game. This mod does not patch or require HWE.

## What the fix changes

The patch replaces only `WeedVisualsSetter.ApplyVisuals(ProductDefinition)`:

1. Read each renderer through the non-instantiating `sharedMaterials` API.
2. Select the same weed materials through the game's `WeedDefinition.GetMaterial` logic.
3. Assign those shared references and enable the same visuals container.
4. Skip the original instantiating call.

Everything else remains native game behavior. The mod does **not**:

- suppress or delay storage refreshes;
- alter shelf packing, capacity, stacks, workers or transit routes;
- destroy materials or other Unity assets;
- force garbage collection or `Resources.UnloadUnusedAssets()`;
- disable the transparent/live storage display.

## Test evidence

| Check | v1.1.1 result |
| --- | --- |
| Save initialization | Loaded successfully; no `QueueRefresh` interception |
| Heavy automated storage activity | More than 138,000 patched visual applications during the initial stress run |
| Patch fallbacks | 0 |
| Memory behavior | Stabilized around 12.6–13.0 GiB instead of climbing toward the 41–42 GiB crash point |
| Product appearance | Live weed visuals retained; no material destruction |

The test counters are diagnostic aggregates, logged at most once per minute. They do not perform cleanup work and are not written once activity stops.

## Requirements and compatibility

| Component | Supported target |
| --- | --- |
| Schedule I | `0.4.6f13`, main/default branch |
| Unity | `2022.3.62f2` |
| Backend | IL2CPP |
| Mod loader | MelonLoader `0.7.0` Open-Beta, .NET 6 runtime |
| Harder Working Employees | Compatible but not required |

IL2CPP and Mono mod builds are not interchangeable. A future game update may change the affected method; use a release that explicitly names your installed game version.

## Installation

1. Close Schedule I completely.
2. Install MelonLoader `0.7.0` Open-Beta if it is not already installed.
3. Extract the release ZIP into the Schedule I installation directory. It already contains the `Mods` folder.
4. Confirm this file exists:

   ```text
   Schedule I/Mods/StorageMaterialLeakFix.dll
   ```

5. Start the game. MelonLoader should report **Storage Material Leak Fix v1.1.1**.

The usual Steam installation path is:

```text
C:\Program Files (x86)\Steam\steamapps\common\Schedule I
```

## Runtime verification

The log should contain:

```text
[Storage Material Leak Fix] Loaded storage material leak fix for Schedule I 0.4.6f13.
[Storage Material Leak Fix] Weed appearances use non-instantiating shared material assignment; storage refresh behavior is unchanged.
```

During affected storage activity, the aggregate line appears at most once per minute:

```text
Storage material fix: shared material applies=..., material fallbacks=0; private memory=... GiB.
```

`shared material applies` should rise during weed-storage refreshes. `material fallbacks` should remain zero for normal weed visuals.

## Troubleshooting and bug reports

If the game fails to start or a save no longer loads, remove `StorageMaterialLeakFix.dll` and report the following:

- exact Schedule I version and backend;
- MelonLoader version;
- mod list;
- steps immediately before the issue;
- `MelonLoader/Latest.log`;
- Windows Application Error details if the game hard-crashed.

Please use the [GitHub issue tracker](https://github.com/xboxnuker-rgb/Memory-leak-fixer-for-HWE-GSVSUKACM/issues). Do not upload saves or logs containing personal information publicly.

## Building

Use the reference assemblies generated by the exact Schedule I version being targeted. Do not commit game, Unity, MelonLoader or generated interop binaries.

```powershell
.\scripts\build.ps1 `
  -MelonLoaderRoot "C:\Program Files (x86)\Steam\steamapps\common\Schedule I\MelonLoader"

.\scripts\package.ps1 `
  -MelonLoaderRoot "C:\Program Files (x86)\Steam\steamapps\common\Schedule I\MelonLoader"
```

The installable ZIP is written under the ignored `dist/` directory.

## Source, credits and disclosure

- Maintained by **GSVS UK ACM**.
- Source and issue tracker: [xboxnuker-rgb/Memory-leak-fixer-for-HWE-GSVSUKACM](https://github.com/xboxnuker-rgb/Memory-leak-fixer-for-HWE-GSVSUKACM)
- License: [MIT](LICENSE)
- Technical crash report: [`docs/CRASH_REPORT.md`](docs/CRASH_REPORT.md)
- Contributor guidance: [`CONTRIBUTING.md`](CONTRIBUTING.md)

Codex was used as an AI-assisted investigation, development and documentation tool. The work was human-directed, source-reviewed, compiled and tested in game. The mod contains no generated visual, audio or dialogue assets.
