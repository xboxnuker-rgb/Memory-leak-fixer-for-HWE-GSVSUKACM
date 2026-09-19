# STORAGE MATERIAL LEAK FIX

> **Keep large Schedule I production networks running without live weed-storage displays slowly consuming tens of gigabytes of memory.**  
> Built for **Schedule I 0.4.6f13 IL2CPP** · Maintained by **GSVS UK ACM**

This focused stability patch fixes a confirmed Unity material-instance leak in Schedule I's live storage display. During repeated weed transfers, the base game can instantiate and discard renderer materials every time a shelf refreshes. On a busy automated save, memory can continue climbing until Windows reports resource exhaustion and Unity closes with a native allocator crash.

The fix preserves the live shelf display. Products still appear, change and move normally; the patch simply uses Unity's non-instantiating material-reference API for the same assignment.

---

## At a glance

- **Stops the confirmed allocation path:** prevents disposable material copies during weed-storage appearance updates.
- **Keeps live visuals:** no opaque replacement chest, fixed icon or disabled storage rendering.
- **Leaves gameplay alone:** no changes to capacity, stacks, packing, workers, routes or refresh timing.
- **Works with automation:** designed and stress-tested with a large HWE-driven production save.
- **Does not require HWE:** the affected renderer is base-game code.
- **No destructive cleanup:** never destroys shared materials and never forces garbage collection.
- **Observable:** a compact once-per-minute counter confirms activity, fallbacks and process private memory.

---

## This is not an HWE bug

Harder Working Employees was present when the crash was isolated because it creates far more item transfers than a small manual setup. That makes the defect appear sooner, but the resolved crash path belongs to Schedule I and Unity:

```text
StorageVisualizer.RefreshVisuals
→ WeedVisualsSetter.ApplyVisuals
→ Renderer.materials / Renderer.GetMaterials
→ Material::GetInstantiatedMaterial
→ Unity native allocator
```

The mod does not patch or depend on HWE. Vanilla or differently automated saves can hit the same path if they refresh enough visible weed storage over a long session.

---

## What was happening

In Unity, reading `Renderer.materials` can create private material instances; it is not a harmless array lookup. Schedule I's weed appearance method reads that array, replaces the returned references with the materials belonging to the product definition, and assigns it back.

The private copies created by the read are no longer useful, but they are not released. Each storage refresh can repeat the process across multiple displayed items. The final observed crashes occurred around 41–42 GiB of committed memory, inside `DynamicHeapAllocator::CreateTLSFBlock`, while Unity was copying another material property sheet.

The final native allocator error is why the game can close without a clear managed exception in the MelonLoader log.

---

## What the mod changes

The patch replaces one method: `WeedVisualsSetter.ApplyVisuals(ProductDefinition)`.

It reads each renderer through `sharedMaterials`, asks the game's own `WeedDefinition` for the correct main, secondary, leaf and stem materials, assigns those references, and enables the same visual container as the original method.

It does not modify the shared material properties themselves. It does not intercept `StorageVisualizer`, suppress an update, alter the number of displayed items or change the worker system.

This narrow approach matters. Earlier development experiments proved that destroying uncertain material references can cause magenta products, and that intercepting storage initialization can destabilize IL2CPP save loading. Neither experiment exists in v1.1.1.

---

## Requirements

You need:

1. **Schedule I 0.4.6f13 — IL2CPP/main branch**
2. **MelonLoader 0.7.0 Open-Beta** using its .NET 6 runtime

Harder Working Employees and Mod Manager & Phone App are not requirements.

IL2CPP and Mono mod binaries are not interchangeable. If your game version differs from the version above, check the maintained source page for a matching release before installing.

---

## Installation

1. Close Schedule I completely.
2. Install MelonLoader if it is not already present.
3. Extract the downloaded ZIP into the Schedule I installation directory.
4. Confirm this file exists:

   `Schedule I/Mods/StorageMaterialLeakFix.dll`

5. Launch the game. MelonLoader should list **Storage Material Leak Fix v1.1.1**.

Only one `StorageMaterialLeakFix.dll` should be installed. Replace an older copy rather than keeping multiple renamed versions in `Mods`.

---

## Verifying the fix

During affected storage activity, `MelonLoader/Latest.log` reports an aggregate line at most once per minute:

```text
Storage material fix: shared material applies=145006, material fallbacks=0; private memory=12.99 GiB.
```

The application count should rise when visible weed products are refreshed. Normal storage visuals should keep `material fallbacks=0`.

In the initial heavy-save stress test, v1.1.1 handled more than 145,000 affected appearance calls with zero fallbacks while private memory remained around 13 GiB. The previous behavior produced a continuing staircase toward the 41–42 GiB crash point.

Memory will still move as the world loads, workers activate, assets stream and other mods allocate data. The important difference is that it should fluctuate around a working range instead of rising without bound in proportion to storage refreshes.

---

## Compatibility and scope

- Designed for Schedule I `0.4.6f13`, Unity `2022.3.62f2`, IL2CPP.
- Compatible with Harder Working Employees in the tested full mod set.
- Does not depend on HWE or alter its worker logic.
- Targets weed storage appearances because that is the symbol-resolved crash path.
- Does not claim to fix unrelated leaks, pathfinding spikes or performance problems in other mods.
- A future Schedule I update may change or remove the affected code.

---

## Troubleshooting

### The game does not start or a save does not load

Close the game and remove `StorageMaterialLeakFix.dll`. Confirm the installed game version is the supported f13 IL2CPP build, then report the game version, MelonLoader version, mod list and `Latest.log` on GitHub.

### Products appear magenta

Confirm the loaded version is v1.1.1. Experimental development builds that attempted material destruction were rejected and must not remain in the `Mods` folder. Replace every old copy with the current file.

### Memory is still changing

Normal world loading and gameplay allocate and release memory. Watch the trend over time under comparable activity. A fluctuation or a higher stable baseline is not the original leak; a persistent staircase tied to storage updates is worth reporting.

### The log reports material fallbacks

Include the complete warning, the product being displayed and your mod list in the issue report. A fallback intentionally lets the original game method run when the renderer layout is not one the patch can safely reproduce.

When filing a report, avoid publicly uploading saves, dumps or logs that contain personal information. Crash dumps should be transferred privately.

---

## Source, licence and disclosure

Storage Material Leak Fix is maintained by **GSVS UK ACM** and released under the **MIT License**.

- Source, releases and bug reports: https://github.com/xboxnuker-rgb/Memory-leak-fixer-for-HWE-GSVSUKACM
- GSVS projects: https://github.com/xboxnuker-rgb/GSVS-Projects-Hub
- Optional maintainer support: https://www.patreon.com/cw/GSVS_UK_ACM/shop

Codex was used as an AI-assisted crash-investigation, development and documentation tool. All work was human-directed, source-reviewed, compiled and play-tested. The mod contains no generated image, audio or dialogue assets. The Nexus listing uses the **AI-Generated Content** tag to disclose that assistance accurately.

---

## Version 1.1.1

- Replaced the leaking weed-appearance material-array read with non-instantiating shared-material assignment.
- Preserved the game's material selection and live shelf visuals.
- Left storage refresh timing, representation packing, workers and routes unchanged.
- Removed the unsafe experimental `QueueRefresh` interception from v1.1.0.
- Removed all material-destruction behavior from rejected development builds.
- Added compact verification counters for applications, fallbacks and private memory.

If this patch keeps a large production save alive, a useful report with your game version, activity level, runtime and memory range will help validate future releases.
