using BepInEx.Logging;
using HarmonyLib;
using OpJosModREPO.Util;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
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

        //Enemies/Enemy - Duck
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void UpdatePatch(PlayerAvatar __instance)
        {
            if (SemiFunc.InputDown(InputKey.Jump) && SemiFunc.IsMasterClient()) // Ensure only the host spawns
            {
                string duckPrefabPath = "Enemies/Enemy - Duck";

                GameObject duckPrefab = Resources.Load<GameObject>(duckPrefabPath);
                if (duckPrefab == null)
                {
                    mls.LogError($"Duck prefab not found at path: {duckPrefabPath}");
                    return;
                }

                var spawnPos = __instance.transform.position;

                mls.LogMessage($"Spawning duck at {spawnPos}");

                // Spawn the duck at the chosen spawn location
                EnemySetup duckSetup = ScriptableObject.CreateInstance<EnemySetup>();
                duckSetup.spawnObjects = new List<GameObject> { duckPrefab };

                RunManager.instance.EnemiesSpawnedRemoveStart();
                ReflectionUtils.InvokeMethod(LevelGenerator.Instance, "EnemySpawn", new object[] { duckSetup, spawnPos });
                RunManager.instance.EnemiesSpawnedRemoveEnd();

                EnemyDirector.instance.DebugResult();
                mls.LogInfo("Duck spawned successfully.");
            }
        }
    }
}
