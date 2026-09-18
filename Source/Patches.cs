using HarmonyLib;
using Il2CppScheduleOne.Product;
using Il2CppScheduleOne.Storage;

namespace StorageMaterialLeakFix
{
    [HarmonyPatch(
        typeof(WeedVisualsSetter),
        nameof(WeedVisualsSetter.ApplyVisuals),
        new[] { typeof(ProductDefinition) })]
    internal static class WeedVisualsSetterApplyVisualsPatch
    {
        private static void Prefix(WeedVisualsSetter __instance, out MaterialSnapshot __state)
        {
            __state = MaterialLifetimeTracker.CaptureBefore(__instance);
        }

        private static void Postfix(WeedVisualsSetter __instance, MaterialSnapshot __state)
        {
            MaterialLifetimeTracker.CaptureAfter(__instance, __state);
        }
    }

    [HarmonyPatch(typeof(StoredItem), nameof(StoredItem.Destroy))]
    internal static class StoredItemDestroyPatch
    {
        private static void Prefix(StoredItem __instance)
        {
            MaterialLifetimeTracker.Release(__instance, "StoredItem.Destroy");
        }
    }
}

