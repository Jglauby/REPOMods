using BepInEx.Logging;
using HarmonyLib;
using OpJosModREPO.Util;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using UnityEngine;

namespace OpJosModREPO.IAmDucky.Patches
{
    [HarmonyPatch(typeof(Enemy))]
    internal class EnemyPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("EnemyTeleported")]
        [HarmonyPrefix]
        static bool PreventDuckTeleport(Enemy __instance)
        {
            if (__instance.gameObject.name.Contains("duck"))
            {
                mls.LogInfo("Preventing duck from teleporting.");
                return false; // Skip original method execution
            }
            return true; // Allow for other enemies
        }

    }
}
