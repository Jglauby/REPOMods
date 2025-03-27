using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.InputSystem;

namespace OpJosModREPO.BeeMovie.Patches
{
    [HarmonyPatch(typeof(PlayerAvatar))]
    internal class PlayerAvatarPatch
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
            if (__instance.GetInstanceID() != PlayerAvatar.instance.GetInstanceID())
                return;

            if (Keyboard.current.bKey.wasPressedThisFrame)
            {
                mls.LogInfo("starting bee script");
                __instance.StartCoroutine(BeeMovie.PlayBeeMovie(__instance));
            }
        }
    }
}
