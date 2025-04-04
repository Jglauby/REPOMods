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

            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                mls.LogInfo("Activated Flash Bang");
                Pranks.PlayPrank(Pranks.FlashBang, 2f);
            }

            if (Keyboard.current.jKey.wasPressedThisFrame)
            {
                mls.LogInfo("Activated Domain Expansion");
                Pranks.PlayPrank(Pranks.DomainExpansion, 6f);
            }

            if (Keyboard.current.hKey.wasPressedThisFrame)
            {
                mls.LogInfo("Heart Eyes");
                Pranks.PlayPrank(Pranks.HeartEyes, 1f);
            }

            if (Keyboard.current.gKey.wasPressedThisFrame)
            {
                mls.LogInfo("Question Ping");
                Pranks.PlayPrank(Pranks.QuestionPing, 1f);
            }
        }
    }
}
