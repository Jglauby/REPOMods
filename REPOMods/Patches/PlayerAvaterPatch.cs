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
            if (SemiFunc.InputDown(InputKey.Jump))
            {
                EnemyDuck ducky = new EnemyDuck();
                EnemySetup enemy = new EnemySetup();
            }

            //EnemyDuck
        }

        private static void SpawnEnemy(EnemySetup enemySetup, Vector3 pos)
        {
            if ((Object)(object)enemySetup == (Object)null || enemySetup.spawnObjects == null || enemySetup.spawnObjects.Count == 0)
            {
                return;
            }
            GameObject val = enemySetup.spawnObjects[0];
            if ((Object)(object)val == (Object)null)
            {
                return;
            }
            GameObject val2 = ((!GameManager.Multiplayer() || !PhotonNetwork.IsMasterClient) ? Object.Instantiate<GameObject>(val, pos, Quaternion.identity) : PhotonNetwork.InstantiateRoomObject("Enemies/" + ((Object)val).name, pos, Quaternion.identity, (byte)0, (object[])null));
            if ((Object)(object)val2 == (Object)null)
            {
                return;
            }
            EnemyParent component = val2.GetComponent<EnemyParent>();
            if ((Object)(object)component == (Object)null)
            {
                return;
            }
        }
    }
}
