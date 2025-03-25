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

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void UpdatePatch(PlayerAvatar __instance)
        {
            if (SemiFunc.InputDown(InputKey.Jump))
            {
                mls.LogMessage("Jump pressed");
            }
        }
    }
}
