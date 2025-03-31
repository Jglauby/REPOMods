using BepInEx.Logging;
using HarmonyLib;
using OpJosModREPO.Util;
using Photon.Pun;
using REPOMods;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using UnityEngine;

namespace OpJosModREPO.IAmDucky.Patches
{
    [HarmonyPatch(typeof(EnemyHealth))]
    internal class EnemyHealthPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("Death")]
        [HarmonyPrefix]
        static void DeathPatch(EnemyHealth __instance)
        {
            Enemy enemy = ReflectionUtils.GetFieldValue<Enemy>(__instance, "enemy");
            DuckPlayerController duckController = GameObject.FindObjectOfType<DuckPlayerController>();
            if (duckController != null)
            {
                EnemyDuck duck = duckController.thisDuck;
                if (enemy.GetInstanceID() == duck.enemy.GetInstanceID() && 
                    ReflectionUtils.GetFieldValue<bool>(PlayerAvatar.instance, "deadSet"))//and player is dead
                {
                    mls.LogInfo("Duck dying is duck being controlled, release control of duck");
                    PublicVars.DuckDied = true;
                    GeneralUtil.ReleaseDuckControlToSpectate();
                }
            }
        }
    }
}
