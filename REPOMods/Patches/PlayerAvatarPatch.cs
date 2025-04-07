using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.InputSystem;

namespace OpJosModREPO.modname.Patches
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
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                mls.LogMessage("Jump pressed");
            }
        }
    }
}
