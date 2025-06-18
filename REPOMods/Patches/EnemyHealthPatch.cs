using BepInEx.Logging;
using HarmonyLib;
using OpJosModREPO.Controllers.IAmEnemy;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;
using UnityEngine;

namespace OpJosModREPO.IAmEnemy.Patches
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
            if (enemy == null) return;

            if (PublicVars.EnemyCleanupInProgress)
            {
                mls.LogInfo("Enemy cleanup already in progress — skipping DeathRPC patch.");
                return;
            }

            EnemyControllerBase enemyController = GeneralUtil.FindEnemyController(enemy);
            if (enemyController == null)
            {
                mls.LogWarning("No PlayerController found for enemy. Skipping DeathRPC handling.");
                return;
            }

            if (PhotonNetwork.LocalPlayer.ActorNumber == enemyController.controlActorNumber && ReflectionUtils.GetFieldValue<bool>(PlayerAvatar.instance, "deadSet")) //is your enemy
            {
                mls.LogInfo("enemy dying is enemy being controlled, release control of enemy");
                GeneralUtil.ReleaseEnemyControlToSpectate();
            }
            else if (PhotonNetwork.IsMasterClient) //destory relevant controller if host
            {
                GameObject.Destroy(enemyController);
                mls.LogInfo($"Player{enemyController.controlActorNumber}'s enemy controller destroyed.");
            }
        }
    }
}
