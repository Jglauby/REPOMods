using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace OpJosModREPO.Tourettes.Patches
{
    [HarmonyPatch(typeof(PlayerAvatar))]
    internal class PlayerAvaterPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        private static float nextExecutionTime = 0f;
        private static System.Random rng = new System.Random();

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void UpdatePatch(PlayerAvatar __instance)
        {
            if (Time.time > nextExecutionTime)
            {
                mls.LogInfo("said random phrase!");
                sayPhrase();

                nextExecutionTime = Time.time + rng.Next(1 * 60, 6 * 60);
            }
        }

        static void sayPhrase()
        {

        }
    }
}
