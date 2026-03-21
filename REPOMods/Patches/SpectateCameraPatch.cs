using BepInEx.Logging;
using HarmonyLib;
using OpJosModREPO.Controllers.IAmEnemy;
using OpJosModREPO.IAmEnemy.UI;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;
using UnityEngine;

namespace OpJosModREPO.IAmEnemy.Patches
{
    [HarmonyPatch(typeof(SpectateCamera))]
    internal class SpectateCameraPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void UpdatePatch(SpectateCamera __instance)
        {
            //need to ensure its local player? maybe
            EnemySelector.Update();
        }
    }
}
