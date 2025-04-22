using BepInEx.Logging;
using HarmonyLib;
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
            if (!PhotonNetwork.IsMasterClient)
                return;

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

                GeneralUtil.ReattatchCameraToPlayer();
                GeneralUtil.RemoveSpawnedControllableDuck(duckController);

                PublicVars.DuckCleanupInProgress = false;
                PublicVars.DuckInBlendMode = false; //ensures when duck spawns you dont spawn in blend mode
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
            PublicVars.TimesSpawnedDuck = 0;
            PublicVars.DuckCleanupInProgress = false;
            PublicVars.DuckInBlendMode = false;         
        }
    }
}
