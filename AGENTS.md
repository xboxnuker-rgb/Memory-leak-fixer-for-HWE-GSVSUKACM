# Agent Instructions

This repository is maintained by human and AI contributors. Preserve enough evidence for another contributor to continue without chat history.

## Before changing code

1. Read `README.md`, `CONTRIBUTING.md`, and `docs/HANDOVER.md`.
2. Inspect the working tree and preserve unrelated work.
3. Confirm the exact Schedule I backend and game version represented by the reference assemblies.
4. Review the crash report and the failed-version history before expanding any Harmony target.

## Project rules

- Never commit game assemblies, generated IL2CPP assemblies, MelonLoader binaries, logs, saves, credentials or local toolchains.
- Keep the patch limited to the proven material-instantiation path.
- Do not reintroduce the `StorageVisualizer.QueueRefresh` hook from v1.1.0.
- Do not destroy renderer materials; the v1.0.3 experiment demonstrated that assigned references may be shared assets.
- Treat IL2CPP and Mono as separate compatibility targets.
- Resolve Harmony targets by exact type, method name and parameter signature.
- Do not publish a release or Nexus page without owner approval and completed runtime checks.

## Verification

- Build with zero warnings and errors against matching references.
- Inspect the binary for `get_sharedMaterials` and `set_sharedMaterials`.
- Confirm the binary contains no `QueueRefresh`, `StorageVisualizer` or `UnityEngine.Object.Destroy` patch path.
- Record artifact hashes and runtime results in `docs/HANDOVER.md`.
- Keep build output and packages under ignored directories.
