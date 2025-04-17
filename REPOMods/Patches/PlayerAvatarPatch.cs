using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OpJosModREPO.Tourettes.Patches
{
    [HarmonyPatch(typeof(PlayerAvatar))]
    internal class PlayerAvatarPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        private static float nextExecutionTime = 0f;
        private static System.Random rng = new System.Random();

        public static bool isSpeakingBee = false;

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void UpdatePatch(PlayerAvatar __instance)
        {
            if (__instance.GetInstanceID() != PlayerAvatar.instance.GetInstanceID())
                return;

            if (Time.time > nextExecutionTime && __instance.isActiveAndEnabled && isSpeakingBee == false)
            {
                mls.LogInfo("said random phrase!");
                Phrases.SpeakRandomPhrase(__instance);
            
                nextExecutionTime = Time.time + rng.Next(ConfigVariables.lowestDelay, ConfigVariables.highestDelay);
            }

            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                mls.LogInfo("said random phrase!");
                Phrases.SpeakRandomPhrase(__instance);
            }
        }
    }
}
