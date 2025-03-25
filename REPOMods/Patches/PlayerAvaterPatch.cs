using BepInEx.Logging;
using HarmonyLib;

namespace OpJosModREPO.godmode.Patches
{
    [HarmonyPatch(typeof(PlayerAvatar))]
    internal class PlayerAvaterPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("PlayerDeath")]
        [HarmonyPrefix]
        static bool PlayerDeathPatch(PlayerAvatar __instance)
        {
            if (__instance.GetInstanceID() == PlayerAvatar.instance.GetInstanceID())
            {
                mls.LogInfo("Don't kill player");
                return false;
            }

            return true;
        }
    }
}
