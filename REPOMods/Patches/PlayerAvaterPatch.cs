using BepInEx.Logging;
using HarmonyLib;
using System.Media;

namespace OpJosModREPO.RainbowPlayer.Patches
{
    [HarmonyPatch(typeof(PlayerAvatar))]
    internal class PlayerAvaterPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        private static bool swapColor = false;
        private static int curColor = 0;

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void UpdatePatch(PlayerAvatar __instance)
        {
            if (SemiFunc.InputDown(InputKey.Jump))
            {
                mls.LogMessage("swaping color");
                swapColor = true;
            }

            if (swapColor)
            {
                try
                {
                    __instance.PlayerAvatarSetColor(curColor);
                    curColor++;
                }
                catch
                {
                    __instance.PlayerAvatarSetColor(0);
                    curColor = 0;
                }
            }
        }
    }
}
