# Nexus Mods upload copy — Storage Material Leak Fix

This sheet is ready to copy into the Schedule I Nexus Mods upload form after the v1.1.1 lifecycle test passes.

## Main details

**Game**

Schedule I

**Mod name**

Storage Material Leak Fix - HWE and Automation Stability

**One-line summary**

Stops a confirmed Unity material-instance leak in live weed storage displays that can drive long automated sessions past 40 GiB and into a native crash.

**Version**

1.1.1

**Category**

Gameplay

**Suggested tags**

Bug Fixes, Performance, Quality of Life, Utilities for Players, AI-Generated Content

Select Nexus Mods' **AI-Generated Content** disclosure tag because Codex assisted the investigation, implementation and documentation. State clearly that the work was human-directed, reviewed, compiled and play-tested and that the mod includes no generated media assets.

Do **not** select **Nexus Mods Turns 25**; the 2026 event excludes generative-AI-assisted code.

**Author or team name**

GSVS UK ACM

## General-page field map

- **Mod Name:** `Storage Material Leak Fix - HWE and Automation Stability`
- **Game:** `Schedule I`
- **Category:** `Gameplay`
- **Mod version:** `1.1.1`
- **Author or team name:** `GSVS UK ACM`
- **Short description:** use the one-line summary above
- **Translation of another mod:** unchecked
- **Community tagging:** enabled
- **Download mirrors:** empty; Nexus hosts the verified main file and GitHub remains the source/release reference
- **Save/Publish:** save and preview the draft. Do not publish until the owner confirms the rendered page, file, requirements, permissions and images.

## Requirements

- Schedule I `0.4.6f13`, IL2CPP/main branch
- MelonLoader `0.7.0` Open-Beta
- Harder Working Employees is **not required**
- Mod Manager & Phone App is **not required**

## Full description

Use `docs/NEXUS_DESCRIPTION.md` as the polished master copy.

### Rich-text formatting plan

- Use the title as the largest purple heading.
- Format the one-line crash/fix summary beneath it as a callout.
- Keep “Not an HWE bug” prominent near the top.
- Use heading level 2 for major sections and heading level 3 for troubleshooting items.
- Keep the native call path in a monospace/code block.
- Use restrained purple and green accents matching the GSVS presentation.
- Separate major sections with horizontal rules.

## Main file

**File title**

Storage Material Leak Fix 1.1.1 - Schedule I 0.4.6f13 IL2CPP

**Upload filename**

`Storage-Material-Leak-Fix-v1.1.1-Schedule-I-f13.zip`

**File description**

```text
Main file for Schedule I 0.4.6f13 IL2CPP. Prevents live weed-storage refreshes from instantiating and discarding Unity material copies. Requires MelonLoader 0.7.0 Open-Beta. Extract into the Schedule I game directory; the ZIP contains Mods/StorageMaterialLeakFix.dll. HWE is compatible but not required.
```

**Changelog**

```text
1.1.1
- Replaced the leaking weed-appearance material-array read with non-instantiating shared-material assignment.
- Preserved the game's original product material selection and live storage visuals.
- Left storage refresh timing, packing, workers and routes unchanged.
- Removed the unsafe experimental QueueRefresh optimization from 1.1.0.
- Added aggregate once-per-minute verification counters.
```

## Permissions and disclosure

- **Original work/assets:** Original MIT-licensed patch by GSVS UK ACM. No third-party assets are bundled.
- **Other-site upload permission:** permitted under the included MIT License with the copyright and permission notice retained.
- **AI disclosure:** select **AI-Generated Content** and use the disclosure in the full description.
- **Anniversary event:** do not select **Nexus Mods Turns 25**.
- **Donation Points:** owner decision; there is no upstream author split requirement for this original patch.
- **Adult content:** no.
- **Paid content:** no.

## Image plan

Capture after the release candidate completes testing:

1. A normal, correctly rendered weed shelf under active worker use — primary image.
2. Task Manager memory graph showing the former runaway/crash shape for problem context.
3. A stable long-session memory graph under equivalent heavy activity.
4. MelonLoader counter line showing high application count, zero fallbacks and stable memory.
5. Windows Explorer showing `StorageMaterialLeakFix.dll` in `Schedule I/Mods`.

Do not use the experimental magenta-material screenshot as the primary image. If included in technical documentation, label it explicitly as a rejected development build rather than an effect of the release.

## Recommended links

- Source and bug reports: `https://github.com/xboxnuker-rgb/Memory-leak-fixer-for-HWE-GSVSUKACM`
- MelonLoader: `https://melonwiki.xyz/`
- GSVS project hub: `https://github.com/xboxnuker-rgb/GSVS-Projects-Hub`
- Maintainer support: `https://www.patreon.com/cw/GSVS_UK_ACM/shop`

## Pre-publish checklist

- [ ] Complete every runtime check in `docs/HANDOVER.md`.
- [ ] Confirm the uploaded DLL hash matches the tested DLL.
- [ ] Confirm the ZIP contains `Mods/StorageMaterialLeakFix.dll`, README, changelog, manifest and MIT licence.
- [ ] Select Gameplay plus Bug Fixes, Performance, Quality of Life, Utilities for Players and AI-Generated Content.
- [ ] Do not select Nexus Mods Turns 25.
- [ ] State clearly that HWE accelerates the base-game defect but is not required.
- [ ] Upload and caption the final screenshots; use the correct-rendering shelf as primary.
- [ ] Preview the rich-text page and test the download once.
- [ ] Leave final **Publish** approval to the owner.
