using MelonLoader;
using System;

namespace StorageMaterialLeakFix
{
    public sealed class StorageMaterialLeakFixMod : MelonMod
    {
        public const string ModName = "Storage Material Leak Fix";
        public const string Version = "1.0.0";
        public const string ModDescription = "Releases runtime weed materials when Schedule I storage visuals are destroyed.";

        private DateTime _nextSweepUtc;
        private DateTime _nextStatusUtc;

        public override void OnInitializeMelon()
        {
            _nextSweepUtc = DateTime.UtcNow.AddSeconds(10);
            _nextStatusUtc = DateTime.UtcNow.AddMinutes(1);

            MelonLogger.Msg("Loaded targeted storage-material lifetime patch for Schedule I 0.4.6f13.");
            MelonLogger.Msg("Product visuals remain enabled; only runtime material instances created by WeedVisualsSetter are tracked.");
        }

        public override void OnUpdate()
        {
            DateTime now = DateTime.UtcNow;

            if (now >= _nextSweepUtc)
            {
                _nextSweepUtc = now.AddSeconds(10);
                MaterialLifetimeTracker.SweepDestroyedOwners();
            }

            if (now >= _nextStatusUtc)
            {
                _nextStatusUtc = now.AddMinutes(1);
                MaterialLifetimeTracker.LogStatusIfActive();
            }
        }

        public override void OnDeinitializeMelon()
        {
            MaterialLifetimeTracker.ReleaseAll("mod deinitialization");
        }
    }
}

