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

            if (Keyboard.current.pKey.wasPressedThisFrame && SemiFunc.IsMasterClient()) // Ensure only the host spawns
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

            if (SemiFunc.InputDown(InputKey.Chat))
            {
                GameObject closestDuck = GeneralUtil.FindClosestDuck(__instance.transform.position)?.gameObject;

                if (closestDuck != null)
                {
                    mls.LogMessage($"Found closest duck at {closestDuck.transform.position}, moving it to player.");

                    // Disable AI component
                    EnemyDuck duckAI = closestDuck.GetComponent<EnemyDuck>();
                    if (duckAI != null)
                    {
                        duckAI.enabled = false;
                        duckAI.currentState = EnemyDuck.State.Idle;  // Prevent AI from overriding movement
                    }
                    
                    //enemy has some teleport funciton, try using that

                    NavMeshAgent agent = closestDuck.GetComponent<NavMeshAgent>();
                    if (agent != null)
                    {
                        agent.Warp(__instance.transform.position); 
                    }

                    mls.LogMessage($"Duck moving towards {closestDuck.transform.position}");
                }
                else
                {
                    mls.LogError("No duck found to teleport.");
                }
            }

            if (Keyboard.current.leftCtrlKey.wasPressedThisFrame)
            {
                GeneralUtil.ControlClosestDuck(__instance.gameObject.transform.position);
            }
        }
    }
}
