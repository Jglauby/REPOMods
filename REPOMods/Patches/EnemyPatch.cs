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

        [HarmonyPatch("DeathRPC")]
        [HarmonyPrefix]
        static void DeathRPCPatch(EnemyHealth __instance)
        {
            Enemy enemy = ReflectionUtils.GetFieldValue<Enemy>(__instance, "enemy");
            EnemyDuck duck = enemy.GetComponent<EnemyDuck>();
            if (duck == null) return; //not duck that died

            DuckPlayerController ducksController = GeneralUtil.FindDuckController(duck);

            if (PhotonNetwork.LocalPlayer.ActorNumber == ducksController.controlActorNumber && ReflectionUtils.GetFieldValue<bool>(PlayerAvatar.instance, "deadSet")) //is your duck
            {
                mls.LogInfo("Duck dying is duck being controlled, release control of duck");
                PublicVars.DuckDied = true;
                GeneralUtil.ReleaseDuckControlToSpectate();
            }
            else if (PhotonNetwork.IsMasterClient) //destory relevant controller if host
            {
                GameObject.Destroy(ducksController);
                mls.LogInfo($"Player{ducksController.controlActorNumber}'s Duck controller destroyed.");
            }
        }
    }
}
