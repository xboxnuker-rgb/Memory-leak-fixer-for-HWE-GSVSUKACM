# Current handover

## v1.1.1 release candidate

- Status: `RUNTIME_SOAK_IN_PROGRESS`
- Target: Schedule I `0.4.6f13`, Unity `2022.3.62f2`, IL2CPP
- Loader: MelonLoader `0.7.0` Open-Beta, .NET 6
- Maintained repository: `https://github.com/xboxnuker-rgb/Memory-leak-fixer-for-HWE-GSVSUKACM`
- Branch: `main`

## Confirmed root cause

The native crash path reaches `Material::GetInstantiatedMaterial` from `WeedVisualsSetter.ApplyVisuals` during `StorageVisualizer.RefreshVisuals`. The original method reads `Renderer.materials`, discards the instantiated references while substituting product-definition materials, and repeats this during high-frequency live-storage updates.

See `docs/CRASH_REPORT.md` for the resolved stack and reproduction evidence.

## Current implementation

Version 1.1.1 patches only:

```text
Il2CppScheduleOne.Product.WeedVisualsSetter.ApplyVisuals(ProductDefinition)
```

It reproduces the original material mapping with `sharedMaterials`, activates the original visuals container, and skips the leaking original method. If the definition or renderer layout is unexpected, it records a fallback and allows the game method to run.

Do not reintroduce the v1.1.0 `StorageVisualizer.QueueRefresh` prefix. That IL2CPP hook caused `GameAssembly.dll` exception `0x80000003` at save initialization. Do not reintroduce the v1.0.3 material-destruction strategy; it could destroy shared/current assets and produce magenta visuals.

## Build evidence

- Build: zero warnings and zero errors
- Assembly version: `1.1.1.0`
- DLL SHA-256: `8BF8308E537CC9D51FEE62B15B7E125377FA9553C7086D26E4A38E7B133EA2CB`
- Package: `dist/StorageMaterialLeakFix-v1.1.1.zip`
- Package SHA-256: `16E72B42073EBD202B77E639878C896E78BEC9CD894FEE12F96CD03A2DDEA9E0`
- Binary inspection: contains `get_sharedMaterials` and `set_sharedMaterials`; contains no `QueueRefresh`, `StorageVisualizer` or material-destruction hook

## Runtime evidence

Initial full-mod-set soak on 2026-09-19:

- save loaded successfully past the former v1.1.0 crash point;
- more than 145,000 shared-material applications observed;
- zero material fallbacks;
- private memory stabilized around 12.6–13.1 GiB under deliberately heavy automation;
- no Storage Material Leak Fix exception;
- one unrelated DeliverySpotsPlus coroutine exception occurred during load;
- HWE, Improved Packagers PORTED and other worker systems remained active.

## Remaining release checks

- [ ] Exceed the former one-hour failure window under heavy activity.
- [ ] Sleep through the high-activity morning restart.
- [ ] Return to the main menu and confirm memory releases normally.
- [ ] Reload the same save and confirm the lower fresh-load baseline.
- [ ] Confirm weed products remain correctly rendered with no magenta materials.
- [ ] Quit cleanly and inspect the final log and Windows Application events.
- [ ] Replace README candidate wording with the stable direct-download link.
- [ ] Create and publish the GitHub release only after owner approval of the final result.
- [ ] Use `docs/NEXUS_UPLOAD.md` and `docs/NEXUS_DESCRIPTION.md` for the Nexus draft.

## Separate follow-up

HWE hitching is intentionally outside this patch. Profile worker navigation/recovery timing and log frequency in the HWE project after this release is frozen. Do not mix worker-performance hooks into the storage material fix.
