using BepInEx.Logging;
using HarmonyLib;

namespace OpJosModREPO.OpModTesting.Patches
{
    [HarmonyPatch(typeof(RoundDirector))]
    internal class RoundDirectorPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("StartRoundLogic")]
        [HarmonyPrefix]
        public static bool Prefix(ref int value)
        {
            value = 1; // Force haul goal to always be 1
            mls.LogInfo("StartRoundLogic haul goal overridden to 1.");
            return true; // Allow the original method to run with new value
        }
    }
}
