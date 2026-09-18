# Storage Material Leak Fix

A targeted MelonLoader mod for Schedule I that prevents runtime weed materials used by storage-item visuals from accumulating after those visuals are destroyed.

## Download

[Download StorageMaterialLeakFix v1.0.0](releases/StorageMaterialLeakFix-v1.0.0.zip)

The patch was created after repeated crashes in Schedule I `0.4.6f13` reached approximately 41–42 GiB of committed memory. Symbol-resolved dumps showed the allocation path running through:

```text
StorageVisualizer.RefreshVisuals
→ WeedVisualsSetter.ApplyVisuals
→ Renderer.materials
→ Material::GetInstantiatedMaterial
→ DynamicHeapAllocator::CreateTLSFBlock
```

## What the mod changes

The mod observes `WeedVisualsSetter.ApplyVisuals` without replacing it. It:

1. Reads each configured renderer's `sharedMaterials` before the game applies the product appearance. This read does not instantiate a new material.
2. Reads the assigned references again after the game applies the appearance.
3. Records only new materials that also carry Unity runtime-instance markers.
4. Associates those materials with the owning `StoredItem`.
5. Calls `UnityEngine.Object.Destroy()` on those exact instances when the stored-item visual is destroyed.
6. Periodically catches orphaned owners destroyed through another Unity path.

Product colours and storage visuals remain enabled. Shared material assets are not destroyed.

## Installation

Requirements:

- Schedule I IL2CPP build
- MelonLoader `0.7.0` using its .NET 6 runtime

Install either way:

- Copy `StorageMaterialLeakFix.dll` into the game's `Mods` directory, or
- Extract the release ZIP into the Schedule I game directory; it already contains the `Mods` folder.

The usual Steam path is:

```text
C:\Program Files (x86)\Steam\steamapps\common\Schedule I
```

Restart the game after installation.

## Runtime verification

The MelonLoader console/log should contain:

```text
[Storage Material Leak Fix] Loaded targeted storage-material lifetime patch...
```

While affected storage items are moving, it reports aggregate status at most once per minute:

```text
Material cleanup status: live tracked=..., captured=..., released=..., private memory=... GiB.
```

The important value is `released`. It should increase as storage representations are replaced. Process memory should eventually stabilise rather than climbing toward 40+ GiB.

## Compatibility and scope

- Designed against Schedule I `0.4.6f13` / Unity `2022.3.62f2`.
- Intended to be compatible with Harder Working Employees and other automation mods.
- Does not patch or depend on HWE itself.
- Targets weed storage visuals only, matching the resolved crash stack.
- Does not call `Resources.UnloadUnusedAssets()` or force garbage collection.
- Does not disable storage visuals or replace product appearance logic.

If a game update changes the affected classes or methods, MelonLoader will log a Harmony patch failure instead of silently modifying unrelated code.

## Building

With the matching game and a .NET 6 SDK installed:

```powershell
.\scripts\build.ps1 `
  -MelonLoaderRoot "C:\Program Files (x86)\Steam\steamapps\common\Schedule I\MelonLoader"
```

To create the distributable ZIP:

```powershell
.\scripts\package.ps1 `
  -MelonLoaderRoot "C:\Program Files (x86)\Steam\steamapps\common\Schedule I\MelonLoader"
```

The output is written under `dist`.

## Limitations

This is a targeted workaround based on two matching native crash dumps. It addresses the identified material-lifetime path, but long-session testing is still required to verify that no second independent allocation leak exists.
