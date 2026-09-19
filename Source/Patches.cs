using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppScheduleOne.Product;
using System;
using UnityEngine;

namespace StorageMaterialLeakFix
{
    [HarmonyPatch(
        typeof(WeedVisualsSetter),
        nameof(WeedVisualsSetter.ApplyVisuals),
        new[] { typeof(ProductDefinition) })]
    internal static class WeedVisualsSetterApplyVisualsPatch
    {
        // Schedule I's original runtime path calls Renderer.GetMaterials(). Unity
        // instantiates private material copies for that API, but the game then
        // replaces those copies with definition-owned materials without destroying
        // them. Reproduce the same assignment with the non-instantiating shared
        // material API and skip the leaking original method.
        private static bool Prefix(WeedVisualsSetter __instance, ProductDefinition definition)
        {
            try
            {
                WeedDefinition weedDefinition =
                    (definition as Il2CppObjectBase)?.TryCast<WeedDefinition>();

                if (weedDefinition == null)
                {
                    PatchDiagnostics.RecordMaterialFallback();
                    return true;
                }

                var meshSettings = __instance.Meshes;
                if (meshSettings == null)
                {
                    PatchDiagnostics.RecordMaterialFallback();
                    return true;
                }

                for (int settingIndex = 0; settingIndex < meshSettings.Length; settingIndex++)
                {
                    WeedVisualsSetter.MeshMaterialSettings setting = meshSettings[settingIndex];
                    if (setting == null || setting.Mesh == null || setting.Materials == null)
                        continue;

                    var sharedMaterials = setting.Mesh.sharedMaterials;
                    if (sharedMaterials == null || sharedMaterials.Length != setting.Materials.Count)
                    {
                        PatchDiagnostics.WarnOnce(
                            "material-count",
                            $"Shared material count does not match appearance settings for {setting.Mesh.name}; " +
                            "falling back to the original game method for this visual.");
                        PatchDiagnostics.RecordMaterialFallback();
                        return true;
                    }

                    for (int materialIndex = 0; materialIndex < sharedMaterials.Length; materialIndex++)
                    {
                        Material material = weedDefinition.GetMaterial(setting.Materials[materialIndex]);
                        sharedMaterials[materialIndex] = material;
                    }

                    setting.Mesh.sharedMaterials = sharedMaterials;
                }

                if (__instance.VisualsContainer != null)
                    __instance.VisualsContainer.gameObject.SetActive(true);

                PatchDiagnostics.RecordSharedMaterialApplication();
                return false;
            }
            catch (Exception ex)
            {
                PatchDiagnostics.WarnOnce("shared-material-apply", ex.ToString());
                PatchDiagnostics.RecordMaterialFallback();
                return true;
            }
        }
    }
}
