using BepInEx.Logging;
using HarmonyLib;
using REPOMods;
using UnityEngine;
using UnityEngine.InputSystem;

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
            if (__instance.GetInstanceID() != PlayerAvatar.instance.GetInstanceID())
                return;

            if (Time.time > nextExecutionTime && __instance.isActiveAndEnabled)
            {
                mls.LogInfo("said random phrase!");
                __instance.ChatMessageSend(Phrases.GetRandomPhrase(), false);
            
                nextExecutionTime = Time.time + rng.Next(30, 5 * 60); //30, 5*60
            }

            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                mls.LogInfo("said random phrase!");
                __instance.ChatMessageSend(Phrases.GetRandomPhrase(), false);
            }
        }
    }
}
