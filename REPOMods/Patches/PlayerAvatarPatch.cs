using BepInEx.Logging;
using HarmonyLib;
using Photon.Pun;
using REPOMods;
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
            if (!PhotonNetwork.IsMasterClient)
                return;

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
                GeneralUtil.SpawnDuckAt(__instance.transform.position, 1);
            }
        }

        [HarmonyPatch("ReviveRPC")]
        [HarmonyPostfix]
        static void ReviveRPCPatch(PlayerAvatar __instance)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            int actorNumber = __instance.photonView.OwnerActorNr;

            if (actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                mls.LogInfo($"Handling local player respawn: {actorNumber}");

                var duckController = GeneralUtil.FindDuckController(actorNumber);

                if (!PublicVars.CanSpawnDuck && !PublicVars.DuckDied)
                {
                    mls.LogMessage("Player revived while duck is spawned and alive");
                    GeneralUtil.ReattatchCameraToPlayer();
                    GeneralUtil.RemoveSpawnedControllableDuck(duckController);
                    PublicVars.DuckDied = true;
                }
                else if (!PublicVars.CanSpawnDuck && PublicVars.DuckDied)
                {
                    mls.LogMessage("Player revived while duck was spawned and died");
                    GeneralUtil.ReattatchCameraToPlayer();
                }

                PublicVars.DuckCleanupInProgress = false;
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
            if (!PhotonNetwork.IsMasterClient)
                return;

            if (__instance.GetInstanceID() != PlayerAvatar.instance.GetInstanceID())
            {
                return;
            }

            mls.LogMessage("New Level, allow being duck again");
            PublicVars.CanSpawnDuck = true;
            PublicVars.DuckDied = false;
            PublicVars.DuckCleanupInProgress = false;
        }
    }
}
