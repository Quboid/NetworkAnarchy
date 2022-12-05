using NetworkAnarchy.Patches;
using QCommonLib;
using System.Reflection;

namespace NetworkAnarchy.Mods
{
    internal class CrossTheLine
    {
        private static Assembly ModAssembly;

        private static bool _IsEnabled;
        internal static bool IsEnabled => _IsEnabled;

        private static bool IsPatched = false;
        private static QPatcher Patcher;

        internal static void Initialise(QPatcher patcher)
        {
            Patcher = patcher;
            ModAssembly = QCommon.GetAssembly("mod", "buildanywhere");
            if (ModAssembly != null)
            {
                ModInfo.Log.Info($"NetworkAnarchy: Cross The Line found, skipping patch", "[NA60]");
                _IsEnabled = true;
                return;
            }

            _IsEnabled = false;

            if (!IsPatched)
            {
                Patcher.Prefix(typeof(GameAreaManager).GetMethod("QuadOutOfArea"), typeof(GAM_QuadOutOfArea).GetMethod("Prefix"));
                IsPatched = true;
                ModInfo.Log.Info($"NetworkAnarchy: Cross The Line not found, patch applied", "[NA61]");
            }
        }

        internal static void Deactivate()
        {
            if (IsPatched)
            {
                Patcher.Unpatch(typeof(GameAreaManager).GetMethod("QuadOutOfArea"), typeof(GAM_QuadOutOfArea).GetMethod("Prefix"));
                IsPatched = false;
                ModInfo.Log.Info($"NetworkAnarchy: Cross The Line unpatched", "[NA62]");
            }
        }
    }
}
