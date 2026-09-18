using Il2CppScheduleOne.Product;
using Il2CppScheduleOne.Storage;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace StorageMaterialLeakFix
{
    internal sealed class MaterialSnapshot
    {
        internal readonly HashSet<int> ExistingMaterialIds = new HashSet<int>();
        internal StoredItem Owner;
    }

    internal sealed class OwnerMaterialRecord
    {
        internal readonly StoredItem Owner;
        internal readonly Dictionary<int, Material> Materials = new Dictionary<int, Material>();

        internal OwnerMaterialRecord(StoredItem owner)
        {
            Owner = owner;
        }
    }

    internal static class MaterialLifetimeTracker
    {
        private static readonly Dictionary<int, OwnerMaterialRecord> MaterialsByOwner =
            new Dictionary<int, OwnerMaterialRecord>();

        private static long _capturedTotal;
        private static long _releasedTotal;
        private static long _lastReportedCaptured;
        private static long _lastReportedReleased;

        internal static MaterialSnapshot CaptureBefore(WeedVisualsSetter setter)
        {
            var snapshot = new MaterialSnapshot();

            if (IsUnityNull(setter))
                return snapshot;

            try
            {
                snapshot.Owner = setter.GetComponentInParent<StoredItem>(true);
                CollectConfiguredMaterials(setter, snapshot.ExistingMaterialIds, null);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"Could not snapshot weed materials before visual update: {ex.Message}");
            }

            return snapshot;
        }

        internal static void CaptureAfter(WeedVisualsSetter setter, MaterialSnapshot snapshot)
        {
            if (snapshot == null || IsUnityNull(setter))
                return;

            try
            {
                if (IsUnityNull(snapshot.Owner))
                    snapshot.Owner = setter.GetComponentInParent<StoredItem>(true);

                if (IsUnityNull(snapshot.Owner))
                    return;

                var newMaterials = new List<Material>();
                CollectConfiguredMaterials(setter, null, newMaterials);

                for (int i = 0; i < newMaterials.Count; i++)
                {
                    Material material = newMaterials[i];
                    if (IsUnityNull(material))
                        continue;

                    int materialId = material.GetInstanceID();
                    if (snapshot.ExistingMaterialIds.Contains(materialId))
                        continue;

                    // The before/after difference proves the reference appeared during
                    // ApplyVisuals. The extra runtime-instance test prevents an asset
                    // assigned by the game from ever being treated as disposable.
                    if (!LooksLikeRuntimeInstance(material))
                        continue;

                    Register(snapshot.Owner, material);
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"Could not track weed materials after visual update: {ex.Message}");
            }
        }

        internal static void Release(StoredItem owner, string reason)
        {
            if (ReferenceEquals(owner, null))
                return;

            int ownerId;
            try
            {
                ownerId = owner.GetInstanceID();
            }
            catch
            {
                return;
            }

            ReleaseByOwnerId(ownerId, reason);
        }

        internal static void SweepDestroyedOwners()
        {
            if (MaterialsByOwner.Count == 0)
                return;

            var staleOwnerIds = new List<int>();

            foreach (KeyValuePair<int, OwnerMaterialRecord> pair in MaterialsByOwner)
            {
                bool stale;
                try
                {
                    stale = IsUnityNull(pair.Value.Owner) || pair.Value.Owner.Destroyed;
                }
                catch
                {
                    stale = true;
                }

                if (stale)
                    staleOwnerIds.Add(pair.Key);
            }

            for (int i = 0; i < staleOwnerIds.Count; i++)
                ReleaseByOwnerId(staleOwnerIds[i], "orphan sweep");
        }

        internal static void ReleaseAll(string reason)
        {
            if (MaterialsByOwner.Count == 0)
                return;

            var ownerIds = new List<int>(MaterialsByOwner.Keys);
            for (int i = 0; i < ownerIds.Count; i++)
                ReleaseByOwnerId(ownerIds[i], reason);
        }

        internal static void LogStatusIfActive()
        {
            if (_capturedTotal == _lastReportedCaptured && _releasedTotal == _lastReportedReleased)
                return;

            long privateBytes = 0;
            try
            {
                using Process process = Process.GetCurrentProcess();
                privateBytes = process.PrivateMemorySize64;
            }
            catch
            {
                // Memory reporting is diagnostic only.
            }

            int liveMaterialCount = 0;
            foreach (OwnerMaterialRecord record in MaterialsByOwner.Values)
                liveMaterialCount += record.Materials.Count;

            double gib = privateBytes / 1073741824d;
            MelonLogger.Msg(
                $"Material cleanup status: live tracked={liveMaterialCount}, " +
                $"captured={_capturedTotal}, released={_releasedTotal}, private memory={gib:F2} GiB.");

            _lastReportedCaptured = _capturedTotal;
            _lastReportedReleased = _releasedTotal;
        }

        private static void Register(StoredItem owner, Material material)
        {
            int ownerId = owner.GetInstanceID();
            int materialId = material.GetInstanceID();

            if (!MaterialsByOwner.TryGetValue(ownerId, out OwnerMaterialRecord record))
            {
                record = new OwnerMaterialRecord(owner);
                MaterialsByOwner.Add(ownerId, record);
            }

            if (record.Materials.ContainsKey(materialId))
                return;

            record.Materials.Add(materialId, material);
            _capturedTotal++;
        }

        private static void ReleaseByOwnerId(int ownerId, string reason)
        {
            if (!MaterialsByOwner.TryGetValue(ownerId, out OwnerMaterialRecord record))
                return;

            MaterialsByOwner.Remove(ownerId);

            int releasedNow = 0;
            foreach (Material material in record.Materials.Values)
            {
                try
                {
                    if (IsUnityNull(material) || !LooksLikeRuntimeInstance(material))
                        continue;

                    UnityEngine.Object.Destroy(material);
                    releasedNow++;
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning($"Could not release a tracked storage material: {ex.Message}");
                }
            }

            _releasedTotal += releasedNow;

            if (releasedNow > 0 && reason == "orphan sweep")
                MelonLogger.Msg($"Released {releasedNow} orphaned storage material instance(s).");
        }

        private static void CollectConfiguredMaterials(
            WeedVisualsSetter setter,
            HashSet<int> materialIds,
            List<Material> materials)
        {
            var meshSettings = setter.Meshes;
            if (meshSettings == null)
                return;

            for (int settingIndex = 0; settingIndex < meshSettings.Length; settingIndex++)
            {
                WeedVisualsSetter.MeshMaterialSettings setting = meshSettings[settingIndex];
                if (setting == null || IsUnityNull(setting.Mesh))
                    continue;

                // sharedMaterials returns the references currently assigned to the
                // renderer without causing Unity to instantiate another copy.
                var assignedMaterials = setting.Mesh.sharedMaterials;
                if (assignedMaterials == null)
                    continue;

                for (int materialIndex = 0; materialIndex < assignedMaterials.Length; materialIndex++)
                {
                    Material material = assignedMaterials[materialIndex];
                    if (IsUnityNull(material))
                        continue;

                    int id = material.GetInstanceID();
                    materialIds?.Add(id);
                    materials?.Add(material);
                }
            }
        }

        private static bool LooksLikeRuntimeInstance(Material material)
        {
            try
            {
                string materialName = material.name ?? string.Empty;
                bool instanceName = materialName.EndsWith(" (Instance)", StringComparison.Ordinal);
                bool nonPersistent = (material.hideFlags & HideFlags.DontSave) != 0;
                return instanceName || nonPersistent;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsUnityNull(UnityEngine.Object value)
        {
            return ReferenceEquals(value, null) || value == null;
        }
    }
}

