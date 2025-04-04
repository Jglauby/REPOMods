using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.InputSystem;

namespace OpJosModREPO.TTSPranks.Patches
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

            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                mls.LogInfo("Activated Flash Bang");
                PlayerAvatar.instance.ChatMessageSend(Pranks.FlashBang, false);
            }
        }
    }
}
