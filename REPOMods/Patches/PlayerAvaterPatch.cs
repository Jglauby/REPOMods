using BepInEx.Logging;
using HarmonyLib;
using System.Media;
using UnityEngine;

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
        private static float delay = 0.5f;
        private static float lastRan = 0f;

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void UpdatePatch(PlayerAvatar __instance)
        {
            if (Time.time - lastRan > delay)
            {
                mls.LogMessage("ChangingColor");
                lastRan = Time.time;
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
