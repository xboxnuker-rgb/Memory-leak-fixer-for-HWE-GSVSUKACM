using MelonLoader;
using System;

namespace StorageMaterialLeakFix
{
    public sealed class StorageMaterialLeakFixMod : MelonMod
    {
        public const string ModName = "Storage Material Leak Fix";
        public const string Version = "1.1.1";
        public const string ModDescription = "Prevents leaking runtime materials when Schedule I refreshes weed storage visuals.";

        private DateTime _nextStatusUtc;

        public override void OnInitializeMelon()
        {
            _nextStatusUtc = DateTime.UtcNow.AddMinutes(1);

            MelonLogger.Msg("Loaded storage material leak fix for Schedule I 0.4.6f13.");
            MelonLogger.Msg("Weed appearances use non-instantiating shared material assignment; storage refresh behavior is unchanged.");
        }

        public override void OnUpdate()
        {
            DateTime now = DateTime.UtcNow;

            if (now >= _nextStatusUtc)
            {
                _nextStatusUtc = now.AddMinutes(1);
                PatchDiagnostics.LogStatus();
            }
        }
    }
}
