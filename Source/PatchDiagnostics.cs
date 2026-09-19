using MelonLoader;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StorageMaterialLeakFix
{
    internal static class PatchDiagnostics
    {
        private static readonly HashSet<string> ReportedWarnings = new HashSet<string>();

        private static long _sharedMaterialApplications;
        private static long _materialFallbacks;

        private static long _lastReportedApplications = -1;

        internal static void RecordSharedMaterialApplication() => _sharedMaterialApplications++;
        internal static void RecordMaterialFallback() => _materialFallbacks++;

        internal static void WarnOnce(string key, string message)
        {
            if (!ReportedWarnings.Add(key + "|" + message))
                return;

            MelonLogger.Warning(message);
        }

        internal static void LogStatus()
        {
            if (_sharedMaterialApplications == _lastReportedApplications)
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

            double gib = privateBytes / 1073741824d;
            MelonLogger.Msg(
                $"Storage material fix: shared material applies={_sharedMaterialApplications}, " +
                $"material fallbacks={_materialFallbacks}; private memory={gib:F2} GiB.");

            _lastReportedApplications = _sharedMaterialApplications;
        }
    }
}
