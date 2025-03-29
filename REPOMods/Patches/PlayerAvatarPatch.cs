using BepInEx.Logging;
using HarmonyLib;
using OpJosModREPO.IAmDucky.Networking;
using OpJosModREPO.Util;
using Photon.Pun;
using REPOMods;
using System.Collections.Generic;
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
            if (PublicVars.CanSpawnDuck == false)
            {
                mls.LogInfo("Can't spawn duck again, set to spectate");
                GeneralUtil.ReleaseDuckControlToSpectate();
                return;
            }

            PublicVars.CanSpawnDuck = false;
            if (PhotonNetwork.IsMasterClient)
            {
                mls.LogMessage("Player is dead, spawning duck as host");
                GeneralUtil.SpawnDuckAt(__instance.transform.position);
            }
            else
            {
                mls.LogMessage("Player is dead, sending spawn duck request to host");
                mls.LogInfo($"[CLIENT] Sending duck spawn request. My actor number: {PhotonNetwork.LocalPlayer.ActorNumber}");
                DuckSpawnerNetwork.Instance.RequestDuckSpawn(__instance.transform.position);
            }

            DelayUtility.RunAfterDelay(20f, () =>
            {
                GeneralUtil.ControlClosestDuck(__instance.transform.position);
            });
        }

        [HarmonyPatch("Revive")]
        [HarmonyPostfix]
        static void RevivePatch(PlayerAvatar __instance)
        {
            if (__instance.GetInstanceID() != PlayerAvatar.instance.GetInstanceID())
            {
                return;
            }

            if (PublicVars.CanSpawnDuck == false && PublicVars.DuckDied == false)//have spawned a duck, and its alive
            { 
                mls.LogMessage("Player revived while duck is spawned and alive");
                GeneralUtil.ReattatchCameraToPlayer();
                GeneralUtil.RemoveSpawnedControllableDuck();
                PublicVars.DuckDied = true; //any other death needs to just readjust camera on respawn... thats why we set this to true
            }
            else if (PublicVars.CanSpawnDuck == false && PublicVars.DuckDied == true)
            {
                mls.LogMessage("Player revived while duck was spawned and died");
                GeneralUtil.ReattatchCameraToPlayer();
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
            PublicVars.CanSpawnDuck = true;
            PublicVars.DuckDied = false;

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
