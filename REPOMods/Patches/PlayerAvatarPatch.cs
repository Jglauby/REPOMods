using BepInEx.Logging;
using HarmonyLib;
using OpJosModREPO.Controllers.IAmEnemy;
using OpJosModREPO.IAmEnemy.Networking;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;
using System;
using UnityEngine;

namespace OpJosModREPO.IAmEnemy.Patches
{
    [HarmonyPatch(typeof(PlayerAvatar))]
    internal class PlayerAvatarPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("PlayerDeath")]
        [HarmonyPostfix]
        static void PlayerDeathPatch(PlayerAvatar __instance)
        {
            PublicVars.SetNextSpawnType();
            if (ConfigVariables.limitEnemiesPerLevel && PublicVars.TimesSpawnedEnemy >= ConfigVariables.maxEnemiesPerLevel)
            {
                mls.LogInfo("Can't spawn enemy again, set to spectate");
                GeneralUtil.ReleaseEnemyControlToSpectate();
                return;
            }

            PublicVars.TimesSpawnedEnemy += 1;
            if (PhotonNetwork.IsMasterClient)
            {
                mls.LogMessage("Player is dead, spawning enemy as host");
                GeneralUtil.SpawnEnemyAt(__instance.transform.position, 1, PublicVars.NextSpawnType);
            }
            else
            {
                mls.LogMessage("Player is dead, sending spawn enemy request to host");
                mls.LogInfo($"[CLIENT] Sending enemy spawn request. My actor number: {PhotonNetwork.LocalPlayer.ActorNumber}");
                EnemySpawnerNetwork.Instance.RequestSpawnEnemy(__instance.transform.position, PublicVars.NextSpawnType);
            }
        }

        //[HarmonyPatch("Jump")]
        //[HarmonyPostfix]
        //static void JumpPatch(PlayerAvatar __instance)
        //{
        //    LogAllEnemyPrefabsInScene();
        //}
        //public static void LogAllEnemyPrefabsInScene()
        //{
        //    var allGameObjects = GameObject.FindObjectsOfType<GameObject>();
        //    foreach (var obj in allGameObjects)
        //    {
        //        if (obj.scene.IsValid() && obj.name.StartsWith("Enemy - ", StringComparison.OrdinalIgnoreCase))
        //        {
        //            mls.LogWarning($"[EnemyPrefab] Name: {obj.name}, Path: {GetHierarchyPath(obj)}");
        //        }
        //    }
        //}
        //private static string GetHierarchyPath(GameObject obj)
        //{
        //    string path = obj.name;
        //    Transform current = obj.transform;
        //    while (current.parent != null)
        //    {
        //        current = current.parent;
        //        path = current.name + "/" + path;
        //    }
        //    return path;
        //}

        [HarmonyPatch("ReviveRPC")]
        [HarmonyPostfix]
        static void ReviveRPCPatch(PlayerAvatar __instance)
        {
            int actorNumber = __instance.photonView.OwnerActorNr;

            if (actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                mls.LogInfo($"Handling local player respawn: {actorNumber}");

                EnemyControllerBase enemyController = GeneralUtil.FindEnemyController(actorNumber);

                GeneralUtil.ReattatchCameraToPlayer();
                GeneralUtil.RemoveSpawnedControllableEnemy(enemyController);

                PublicVars.EnemyCleanupInProgress = false;
                PublicVars.EnemyInBlendMode = false; //ensures when enemy spawns you dont spawn in blend mode
            }
            else if (PhotonNetwork.IsMasterClient)
            {
                mls.LogInfo($"[HOST] Cleaning up enemy for revived player: {actorNumber}");
                EnemyControllerBase enemyController = GeneralUtil.FindEnemyController(actorNumber);
                GeneralUtil.RemoveSpawnedControllableEnemy(enemyController);
            }
        }

        [HarmonyPatch("LoadingLevelAnimationCompleted")]
        [HarmonyPostfix]
        static void LoadingLevelAnimationCompletedPatch(PlayerAvatar __instance)
        {
            if (__instance.GetInstanceID() != PlayerAvatar.instance.GetInstanceID())
            {
                return;
            }

            mls.LogMessage("New Level, allow being an enemy again");
            PublicVars.TimesSpawnedEnemy = 0;
            PublicVars.EnemyCleanupInProgress = false;
            PublicVars.EnemyInBlendMode = false;

            //setup enemy spawner network
            if (EnemySpawnerNetwork.Instance == null)
            {
                GameObject netObj = new GameObject("EnemySpawnerNetwork");
                var spawner = netObj.AddComponent<EnemySpawnerNetwork>();

                PhotonView view = netObj.AddComponent<PhotonView>();

                if (PhotonNetwork.IsMasterClient)
                {
                    // Only the MasterClient is allowed to allocate a ViewID
                    view.ViewID = 1738;
                    mls.LogInfo($"[HOST] Allocated ViewID for EnemySpawnerNetwork: {view.ViewID}");
                }
                else
                {
                    // Use a hardcoded fallback ViewID (must match what host allocated)
                    view.ViewID = 1738;
                    mls.LogInfo($"[CLIENT] Using known ViewID for EnemySpawnerNetwork: {view.ViewID}");
                }

                GameObject.DontDestroyOnLoad(netObj);

                mls.LogInfo("EnemySpawnerNetwork initialized");
            }
        }
    }
}
