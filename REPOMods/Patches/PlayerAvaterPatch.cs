using BepInEx.Logging;
using HarmonyLib;
using OpJosModREPO.Util;
using Photon.Pun;
using System.Media;
using UnityEngine;

namespace OpJosModREPO.IAmDucky.Patches
{
    [HarmonyPatch(typeof(PlayerAvatar))]
    internal class PlayerAvaterPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("SetSpectate")]
        [HarmonyPostfix]
        static void SetSpectatePatch(PlayerAvatar __instance)
        {
            mls.LogMessage("Player is dead");
            //set to duck here?
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void UpdatePatch(PlayerAvatar __instance)
        {
            if (SemiFunc.InputDown(InputKey.Jump))
            {
                //spawn duck monster at location
            }
        }
    }
}
