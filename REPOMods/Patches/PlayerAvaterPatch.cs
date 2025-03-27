﻿using BepInEx.Logging;
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

        [HarmonyPatch("PlayerDeath")]
        [HarmonyPostfix]
        static void PlayerDeathPatch(PlayerAvatar __instance)
        {
            if(PublicVars.CanSpawnDuck == false)
            {
                mls.LogInfo("Can't spawn duck again, returning.");
                return;
            }

            PublicVars.CanSpawnDuck = false;
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

            DelayUtility.RunAfterDelay(15f, () =>
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
        }
    }
}
