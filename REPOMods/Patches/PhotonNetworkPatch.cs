using BepInEx.Logging;
using HarmonyLib;
using OpJosModREPO.Util;
using Photon.Pun;
using System.Media;
using UnityEngine;

namespace OpJosModREPO.IAmDucky.Patches
{
    [HarmonyPatch(typeof(PhotonNetwork))]
    internal class PhotonNetworkPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        //[HarmonyPatch(typeof(PhotonNetwork), "InstantiateRoomObject")]
        //[HarmonyPrefix]
        //static void LogSpawnedObjects(string prefabName, Vector3 position, Quaternion rotation, byte group)
        //{
        //    mls.LogMessage($"Photon is spawning: {prefabName} at {position}");
        //}
    }
}
