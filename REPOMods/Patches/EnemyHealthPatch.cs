using BepInEx.Logging;
using HarmonyLib;
using OpJosModREPO.Util;
using Photon.Pun;
using UnityEngine;

namespace OpJosModREPO.IAmDuckyHostOnly.Patches
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
            if (!PhotonNetwork.IsMasterClient)
                return;

            Enemy enemy = ReflectionUtils.GetFieldValue<Enemy>(__instance, "enemy");
            if (enemy == null) return;
            EnemyDuck duck = enemy.GetComponent<EnemyDuck>();
            if (duck == null) return; //not duck that died

            if (PublicVars.DuckCleanupInProgress)
            {
                mls.LogInfo("Duck cleanup already in progress — skipping DeathRPC patch.");
                return;
            }

            DuckPlayerController ducksController = GeneralUtil.FindDuckController(duck);
            if (ducksController == null)
            {
                mls.LogWarning("No DuckPlayerController found for duck. Skipping DeathRPC handling.");
                return;
            }

            if (PhotonNetwork.LocalPlayer.ActorNumber == ducksController.controlActorNumber && ReflectionUtils.GetFieldValue<bool>(PlayerAvatar.instance, "deadSet")) //is your duck
            {
                mls.LogInfo("Duck dying is duck being controlled, release control of duck");
                GeneralUtil.ReleaseDuckControlToSpectate();
            }
        }
    }
}
