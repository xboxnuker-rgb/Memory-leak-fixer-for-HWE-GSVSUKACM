# Schedule I storage-material leak crash report

## Summary

Schedule I `0.4.6f13` repeatedly grew to approximately 41–42 GiB of committed memory and then closed without a useful managed exception. Two independent dumps failed at the same Unity native allocator address while the game was instantiating a renderer material for a weed product displayed in storage.

The evidence identifies a base-game storage-rendering defect that high-frequency worker automation can amplify. Harder Working Employees was active during reproduction, but no HWE method appears in the resolved crashing stack.

## Environment

- Schedule I `0.4.6f13`, Windows x64 IL2CPP build
- Unity `2022.3.62f2`
- Windows 11 with 32 GB installed RAM
- Direct3D 11
- Final exception: `0xc0000005`, write access violation
- Faulting module: `UnityPlayer.dll+0x355BE0`

## Memory evidence

Windows Resource Exhaustion Detector recorded Schedule I consuming:

- 44,201,488,384 bytes, approximately 41.2 GiB;
- 45,294,247,936 bytes, approximately 42.2 GiB.

Increasing the page file can delay this failure but cannot correct unbounded allocation growth.

## Resolved native stack

```text
DynamicHeapAllocator::CreateTLSFBlock
DynamicHeapAllocator::Allocate
MemoryManager::Allocate
std::_Tree::_Copy_nodes<FastPropertyName, float>
UnityPropertySheet::operator=
Material::GetInstantiatedMaterial
Renderer::GetAndAssignInstantiatedMaterial
RendererScripting::GetMaterialArray
Renderer_CUSTOM_CopyMaterialArray
```

The final allocator failure occurs while Unity attempts to create another roughly 16 MiB TLSF heap block. It is a consequence of memory exhaustion, not the original allocation source.

## Resolved Schedule I path

```text
MoveItemBehaviour coroutine MoveNext()
→ ITransitEntity.InsertItemIntoInput(ItemInstance, NPC)
→ ItemSlot.SetStoredItem(ItemInstance, Boolean)
→ StorageVisualizer.RefreshVisuals()
→ WeedVisualsSetter.ApplyVisuals(ProductDefinition)
→ Renderer material-array access
→ Material::GetInstantiatedMaterial
→ Unity native allocator
```

Recovered IL2CPP logic shows that `StorageVisualizer.RefreshVisuals()` repacks and reinitializes active stored-item displays after slot changes. `WeedVisualsSetter.ApplyVisuals()` reads the renderer material array through the instantiating API, replaces the returned references with definition-owned materials, and assigns the array back. The newly instantiated copies are discarded without destruction.

## Observed trigger

1. Multiple employees repeatedly insert and remove weed products from storage.
2. Slot changes refresh and reinitialize the live storage display.
3. Every affected appearance call creates disposable material copies.
4. Native memory and system commit climb rather than reaching a plateau.
5. Windows reports resource exhaustion near 40+ GiB.
6. Unity faults while allocating another material property sheet.

The product near the analysed failure was `granddaddypurple`, but the defect applies to the shared `WeedVisualsSetter` path rather than one product definition.

## Expected behavior

Storage-rendering memory should stabilize after obsolete visuals are replaced. Repeated item transfers should not produce an unbounded number of native material instances.

## Patch approach

Version 1.1.1 reproduces the game's material selection through `Renderer.sharedMaterials`, which reads and assigns references without first instantiating private copies. It deliberately leaves storage refresh frequency, representation packing and worker behavior unchanged.

The patch does not mutate shared material properties. It assigns the same definition-owned material references selected by `WeedDefinition.GetMaterial`.

## Runtime evidence

The initial heavy-save v1.1.1 stress run exceeded 145,000 patched appearance applications with zero fallbacks while private memory stabilized around 13 GiB. The same activity previously produced a continuing staircase toward the 41–42 GiB crash point.

## Limitations

- The original crash was reproduced in a modded session, not a minimal vanilla installation.
- Automation changes trigger frequency and therefore time-to-failure.
- A future game build may change the affected method or fix it upstream.
- Dumps may contain personal information and should be transferred privately rather than committed to this repository.
