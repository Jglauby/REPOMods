using BepInEx.Logging;
using HarmonyLib;
using OpJosModREPO.Util;
using Photon.Pun;
using System.Media;
using UnityEngine;

namespace OpJosModREPO.IAmDucky.Patches
{
    [HarmonyPatch(typeof(PlayerAvatar))]
    internal class PlayerAvaterPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("SetSpectate")]
        [HarmonyPostfix]
        static void SetSpectatePatch(PlayerAvatar __instance)
        {
            mls.LogMessage("Player is dead");
            //set to duck here?
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void UpdatePatch(PlayerAvatar __instance)
        {
            //if (!PhotonNetwork.IsMasterClient) return;

            if (SemiFunc.InputDown(InputKey.Chat))
            {
                GameObject[] allPrefabs = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (GameObject obj in allPrefabs)
                {
                    if (obj.name.ToLower().Contains("duck")) // Filter by "duck" if possible
                    {
                        mls.LogMessage("Possible Duck Prefab: " + obj.name);
                    }
                }
                //Enemy - Duck | Duck monster
            }

            if (SemiFunc.InputDown(InputKey.Jump))
            {
                string duckPrefabName = "Enemies/Duck monster"; // Corrected prefab name
                Vector3 spawnPos = __instance.transform.position;
                mls.LogMessage("Spawning duck at " + spawnPos);

                GameObject duck = PhotonNetwork.InstantiateRoomObject(duckPrefabName, spawnPos, Quaternion.identity);

                if (duck != null)
                {
                    mls.LogMessage("Duck spawned");
                }
                else
                {
                    mls.LogMessage("Duck not spawned");
                }
            }
        }
    }
}
