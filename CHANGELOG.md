# Changelog

## 1.1.1

- Remove the `StorageVisualizer.QueueRefresh` interception introduced by 1.1.0. Patching that IL2CPP method caused an immediate `GameAssembly.dll` breakpoint crash while a save initialized its storage entities.
- Retain only the bounded `WeedVisualsSetter.ApplyVisuals` replacement that avoids Unity's instantiating `Renderer.materials` API.
- Leave the game's storage-refresh lifecycle and visual packing behavior completely unchanged.

## 1.1.0 — rejected test build

- Replace the leaking `Renderer.GetMaterials` weed-appearance path with equivalent non-instantiating shared-material assignment.
- Stop destroying assigned materials; the 1.0.3 cleanup approach could destroy shared assets and produce magenta visuals.
- Compare the game's desired product-shelf representation with the currently displayed representation.
- Skip a complete shelf clear, repack, and reinitialization when product identity, quality, packaging, and visible counts are unchanged.
- Treat separate runtime instances of visually identical products as equivalent, avoiding rebuilds caused only by worker transfers changing object identity.
- Fall back to the original game refresh for mixed or specialised non-product storage.
- Replace high-volume material tracking with aggregate once-per-minute optimization counters.

## 1.0.3 — rejected test build

- Treat exact before/after renderer-reference changes as owned material instances even when Unity omits `(Instance)` and `DontSave` markers.
- Destroy superseded materials immediately when a live visual applies another appearance.
- Continue destroying the currently assigned instances when their visual owner is removed.
- Release all remaining tracked instances immediately when the gameplay scene unloads.

## 1.0.2 — diagnostic test build

- Track materials by their `WeedVisualsSetter` owner even when no parent `StoredItem` is available during appearance setup.
- Retain the direct `StoredItem.Destroy` cleanup when the storage owner can be resolved.
- Add once-per-minute counters for hook calls, owner resolution, changed references, runtime markers, captures, releases, and private memory.
- Sweep destroyed visual owners every five seconds.

## 1.0.1 — diagnostic test build

- Use Unity's non-generic parent-component lookup to avoid an Il2CppInterop generic-cast failure.
- Suppress duplicate diagnostic exceptions so a failure cannot create log spam or frame hitches.

## 1.0.0

- Track runtime material instances created by `WeedVisualsSetter.ApplyVisuals`.
- Release tracked materials when their owning `StoredItem` is destroyed.
- Sweep materials whose owner was destroyed through an alternate Unity path.
- Report aggregate capture/release counts and process private memory once per minute while active.
