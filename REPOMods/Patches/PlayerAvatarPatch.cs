using BepInEx.Logging;
using HarmonyLib;
using OpJosModREPO.IAmDucky.Networking;
using Photon.Pun;
using UnityEngine;

namespace OpJosModREPO.IAmDucky.Patches
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
            if (ConfigVariables.limitDucksPerLevel && PublicVars.TimesSpawnedDuck >= ConfigVariables.maxDucksPerLevel)
            {
                mls.LogInfo("Can't spawn duck again, set to spectate");
                GeneralUtil.ReleaseDuckControlToSpectate();
                return;
            }

            PublicVars.TimesSpawnedDuck += 1;
            if (PhotonNetwork.IsMasterClient)
            {
                mls.LogMessage("Player is dead, spawning duck as host");
                GeneralUtil.SpawnDuckAt(__instance.transform.position, 1);
            }
            else
            {
                mls.LogMessage("Player is dead, sending spawn duck request to host");
                mls.LogInfo($"[CLIENT] Sending duck spawn request. My actor number: {PhotonNetwork.LocalPlayer.ActorNumber}");
                DuckSpawnerNetwork.Instance.RequestDuckSpawn(__instance.transform.position);
            }
        }

        [HarmonyPatch("ReviveRPC")]
        [HarmonyPostfix]
        static void ReviveRPCPatch(PlayerAvatar __instance)
        {
            int actorNumber = __instance.photonView.OwnerActorNr;

            if (actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                mls.LogInfo($"Handling local player respawn: {actorNumber}");

                var duckController = GeneralUtil.FindDuckController(actorNumber);

                GeneralUtil.ReattatchCameraToPlayer();
                GeneralUtil.RemoveSpawnedControllableDuck(duckController);

                PublicVars.DuckCleanupInProgress = false;
                PublicVars.DuckInBlendMode = false; //ensures when duck spawns you dont spawn in blend mode
            }
            else if (PhotonNetwork.IsMasterClient)
            {
                mls.LogInfo($"[HOST] Cleaning up duck for revived player: {actorNumber}");
                var duckController = GeneralUtil.FindDuckController(actorNumber);
                GeneralUtil.RemoveSpawnedControllableDuck(duckController);
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

            mls.LogMessage("New Level, allow being duck again");
            PublicVars.TimesSpawnedDuck = 0;
            PublicVars.DuckCleanupInProgress = false;
            PublicVars.DuckInBlendMode = false;

            //setup duck spawner network
            if (DuckSpawnerNetwork.Instance == null)
            {
                GameObject netObj = new GameObject("DuckSpawnerNetwork");
                var spawner = netObj.AddComponent<DuckSpawnerNetwork>();

                PhotonView view = netObj.AddComponent<PhotonView>();

                if (PhotonNetwork.IsMasterClient)
                {
                    // Only the MasterClient is allowed to allocate a ViewID
                    view.ViewID = 1738;
                    mls.LogInfo($"[HOST] Allocated ViewID for DuckSpawnerNetwork: {view.ViewID}");
                }
                else
                {
                    // Use a hardcoded fallback ViewID (must match what host allocated)
                    view.ViewID = 1738;
                    mls.LogInfo($"[CLIENT] Using known ViewID for DuckSpawnerNetwork: {view.ViewID}");
                }

                GameObject.DontDestroyOnLoad(netObj);

                mls.LogInfo("DuckSpawnerNetwork initialized");
            }
        }
    }
}
