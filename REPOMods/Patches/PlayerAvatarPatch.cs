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

            mls.LogInfo($"handling player respawn {actorNumber}");
            DuckPlayerController duckController = GeneralUtil.FindDuckController(actorNumber);
            if (duckController == null)
            {
                mls.LogWarning($"No duck controller found for actor {actorNumber}");
                return;
            }

            //host and not this person duck controller
            if (PhotonNetwork.IsMasterClient && actorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                GeneralUtil.RemoveSpawnedControllableDuck(duckController);
            }
            else if (actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {//local and its this person duck controller
                if (PublicVars.CanSpawnDuck == false && PublicVars.DuckDied == false)//have spawned a duck, and its alive
                {
                    mls.LogMessage("Player revived while duck is spawned and alive");
                    GeneralUtil.ReattatchCameraToPlayer();
                    GeneralUtil.RemoveSpawnedControllableDuck(duckController);
                    PublicVars.DuckDied = true; //any other death needs to just readjust camera on respawn... thats why we set this to true
                }
                else if (PublicVars.CanSpawnDuck == false && PublicVars.DuckDied == true)
                {
                    mls.LogMessage("Player revived while duck was spawned and died");
                    GeneralUtil.ReattatchCameraToPlayer();
                }
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
        }
    }
}
