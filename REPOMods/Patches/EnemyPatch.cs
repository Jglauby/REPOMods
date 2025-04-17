using BepInEx.Logging;
using HarmonyLib;
using OpJosModREPO.Util;
using Photon.Pun;
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

            if (PublicVars.DuckCleanupInProgress)
            {
                mls.LogInfo("Duck cleanup already in progress — skipping DeathRPC patch.");
                return;
            }

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
