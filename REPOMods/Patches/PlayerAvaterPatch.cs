using BepInEx.Logging;
using HarmonyLib;
using OpJosModREPO.Util;
using Photon.Pun;
using REPOMods;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

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
            //ensure only the host hits this patch
            if (__instance.photonView == null || !__instance.photonView.IsMine)
            {
                return;
            }

            if (!Keyboard.current.rightCtrlKey.isPressed)//all commands require holding left ctrl
            {
                return;
            }

            if (Keyboard.current.sKey.wasPressedThisFrame && SemiFunc.IsMasterClient()) // Ensure only the host spawns
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

                // Move the duck to the player after delay
                DelayUtility.RunAfterDelay(5f, () =>
                {
                    GeneralUtil.MoveDuckToPos(__instance.transform.position);
                });
            }

            if (Keyboard.current.cKey.wasPressedThisFrame)
            {
                GeneralUtil.ControlClosestDuck(__instance.gameObject.transform.position);
            }

            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                //make it follow player? need some good way to make duck follow you, or make it stay put, and when u control duck those get removed
            }
        }
    }
}
