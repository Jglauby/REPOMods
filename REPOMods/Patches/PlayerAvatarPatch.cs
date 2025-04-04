using BepInEx.Logging;
using HarmonyLib;
using OPJosMod;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

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

            try
            {
                if (Keyboard.current[ConfigVariables.flashBangKey].wasPressedThisFrame)
                {
                    mls.LogInfo("Activated Flash Bang");
                    Pranks.PlayPrank(Pranks.FlashBang, ConfigVariables.flashBangTime);
                }
            } catch { }

            try
            {
                if (Keyboard.current[ConfigVariables.domainExpansionKey].wasPressedThisFrame)
                {
                    mls.LogInfo("Activated Domain Expansion");
                    Pranks.PlayPrank(Pranks.DomainExpansion, ConfigVariables.domainExpansionTime);
                }
            } catch { }

            try
            {
                if (Keyboard.current[ConfigVariables.heartEyesKey].wasPressedThisFrame)
                {
                    mls.LogInfo("Heart Eyes");
                    Pranks.PlayPrank(Pranks.HeartEyes, ConfigVariables.heartEyesTime);
                }
            } catch { }

            try
            {
                if (Keyboard.current[ConfigVariables.questionPingKey].wasPressedThisFrame)
                {
                    mls.LogInfo("Question Ping");
                    Pranks.PlayPrank(Pranks.QuestionPing, ConfigVariables.questionPingTime);
                }
            }
            catch { }
        }
    }
}
