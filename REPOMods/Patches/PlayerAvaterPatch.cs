using BepInEx.Logging;
using HarmonyLib;
using OpJosModREPO.Util;
using REPOMods;
using System.Collections.Generic;
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
            if (__instance.GetInstanceID() != PlayerAvatar.instance.GetInstanceID())
            {
                return;
            }

            mls.LogMessage("Player is dead, spawning duck");

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

            // Move the duck to the player after delay
            DelayUtility.RunAfterDelay(5f, () =>
            {
                GeneralUtil.MoveDuckToPos(__instance.transform.position);
            });

            DelayUtility.RunAfterDelay(30f, () =>
            {
                GeneralUtil.ControlClosestDuck(__instance.gameObject.transform.position);
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

            mls.LogMessage("Player revived, releasing duck control.");
            GeneralUtil.ReleaseDuckControlToPlayer();
        }
    }
}
