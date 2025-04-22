using BepInEx.Logging;
using OpJosModREPO.Util;
using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace OpJosModREPO.IAmDuckyHostOnly
{
    public static class GeneralUtil
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        public static EnemyDuck FindClosestDuck(Vector3 pos)
        {
            EnemyDuck cloestDuck = null;
            float closestDistance = float.MaxValue;

            foreach (var enemy in GameObject.FindObjectsOfType<EnemyDuck>())
            {
                float distance = Vector3.Distance(enemy.gameObject.transform.position, pos);

                if (distance < closestDistance)
                {
                    cloestDuck = enemy;
                    closestDistance = distance;
                }
            }

            return cloestDuck;
        }

        public static EnemyDuck FindClosestDuckWithoutController(Vector3 pos)
        {
            EnemyDuck cloestDuck = null;
            float closestDistance = float.MaxValue;

            foreach (var enemy in GameObject.FindObjectsOfType<EnemyDuck>())
            {
                if (FindDuckController(enemy) != null) //has a controller skip it
                    continue;

                float distance = Vector3.Distance(enemy.gameObject.transform.position, pos);

                if (distance < closestDistance)
                {
                    cloestDuck = enemy;
                    closestDistance = distance;
                }
            }

            return cloestDuck;
        }

        public static DuckPlayerController FindDuckController(EnemyDuck duck)
        {
            foreach (var controller in GameObject.FindObjectsOfType<DuckPlayerController>())
            {
                if (controller.thisDuck.GetInstanceID() == duck.GetInstanceID())
                {
                    return controller;
                }
            }

            return null;
        }

        public static DuckPlayerController FindDuckController(int? actorNumber)
        {
            foreach (var controller in GameObject.FindObjectsOfType<DuckPlayerController>())
            {
                if (controller.controlActorNumber == actorNumber)
                {
                    return controller;
                }
            }

            return null;
        }

        public static EnemyDuck FindDuck(int? actorNumber)
        {
            foreach (var controller in GameObject.FindObjectsOfType<DuckPlayerController>())
            {
                if (controller.controlActorNumber == actorNumber)
                {
                    return controller.thisDuck;
                }
            }

            return null;
        }

        public static List<Enemy> FindCloseEnemies(Vector3 pos, float range)
        {
            List<Enemy> result = new List<Enemy>();

            foreach (var enemy in GameObject.FindObjectsOfType<Enemy>())
            {
                float distance = Vector3.Distance(enemy.gameObject.transform.position, pos);

                if (distance <= range)
                {
                    result.Add(enemy);
                }
            }

            return result;
        }

        public static void MoveDuckToPos(Vector3 pos) 
        {
            EnemyDuck enemyDuck = GeneralUtil.FindClosestDuckWithoutController(pos);
            GameObject duckGameObject = enemyDuck?.gameObject;
            if (duckGameObject == null)
            {

                mls.LogError("No duck found without a controller to teleport.");
                return;
            }
            else
            {
                mls.LogMessage($"Found closest duck at {duckGameObject.transform.position}, moving it to player.");

                enemyDuck.enabled = false;
                enemyDuck.currentState = EnemyDuck.State.Idle;
                ReflectionUtils.SetFieldValue(enemyDuck, "playerTarget", null);

                NavMeshAgent agent = duckGameObject.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    agent.SetDestination(pos);
                }

                //prevents duck from despawning
                EnemyParent enemyParent = ReflectionUtils.GetFieldValue<EnemyParent>(enemyDuck.enemy, "EnemyParent");
                enemyParent.SpawnedTimer = float.PositiveInfinity;

                mls.LogMessage($"Duck moving towards {duckGameObject.transform.position}");
            }
        }

        public static void ControlClosestDuck(Vector3 pos, int actorNumber)
        {
            //player is dead, and it is not the host setting up someone elses controller
            if (!ReflectionUtils.GetFieldValue<bool>(PlayerAvatar.instance, "deadSet") && PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
            {
                mls.LogWarning("Player is not dead, cannot control duck.");
                return;
            }

            EnemyDuck closestDuck = FindClosestDuck(pos);
            if (closestDuck != null)
            {
                mls.LogInfo($"Found closest duck at {closestDuck.gameObject.transform.position}, transferring control to player.");

                // Transfer control: Add PlayerController to Duck
                BreakDuckEnemyAI(closestDuck);
                DuckPlayerController duckPlayerController = closestDuck.gameObject.GetComponent<DuckPlayerController>();
                if (duckPlayerController == null)
                {
                    duckPlayerController = closestDuck.gameObject.AddComponent<DuckPlayerController>();
                }
                duckPlayerController.Setup(actorNumber, closestDuck);

                mls.LogInfo("Control transferred to the duck.");
            }
            else
            {
                mls.LogInfo("No duck found to transfer control.");
            }
        }

        public static void BreakDuckEnemyAI(EnemyDuck duck)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            if (duck == null)
            {
                mls.LogError("Duck is null, cannot break AI.");
                return;
            }

            // Disable AI component
            EnemyDuck duckAI = duck.GetComponent<EnemyDuck>();
            if (duckAI != null)
            {
                duckAI.enabled = false;
                duckAI.currentState = EnemyDuck.State.Idle;  // Prevent AI from overriding movement
            }

            EnemyRigidbody rb = duck.GetComponent<EnemyRigidbody>();
            if (rb != null)
            {
                rb.enabled = false; // prevent SetChaseTarget
            }

            NavMeshAgent agent = duck.gameObject.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.isStopped = true;  // Stop AI pathfinding
                agent.enabled = false;   // Disable NavMeshAgent
            }

            mls.LogInfo("Duck AI broken.");
        }

        public static void EnableDuckEnemyAI(EnemyDuck duck)
        {
            if (duck == null)
            {
                mls.LogError("Duck is null, cannot restore AI.");
                return;
            }

            // Disable AI component
            EnemyDuck duckAI = duck.GetComponent<EnemyDuck>();
            if (duckAI != null)
            {
                duckAI.enabled = true;
                duckAI.currentState = EnemyDuck.State.Roam;
            }

            EnemyRigidbody rb = duck.GetComponent<EnemyRigidbody>();
            if (rb != null)
            {
                rb.enabled = true;
            }

            NavMeshAgent agent = duck.gameObject.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.isStopped = false;
                agent.enabled = true;
            }

            mls.LogInfo("Duck AI restored.");
        }

        public static void RemoveSpawnedControllableDuck(DuckPlayerController duckController)
        {
            if (duckController == null)
            {
                mls.LogWarning("Duck controller is null, cannot destroy.");
                return;
            }

            GameObject.Destroy(duckController);
            mls.LogInfo("Duck controller destroyed.");

            if (PhotonNetwork.IsMasterClient)
            {
                PlayerController pc = PlayerController.instance;
                if (duckController.thisDuck != null)
                {
                    EnemyHealth healthComponent = ReflectionUtils.GetFieldValue<EnemyHealth>(duckController.thisDuck.enemy, "Health");
                    ReflectionUtils.InvokeMethod(healthComponent, "Death", new object[] { Vector3.zero });
                    mls.LogMessage("Killed controlled duck");
                }
            }
        }

        public static void ReattatchCameraToPlayer()
        {
            PlayerController pc = PlayerController.instance;

            Camera mainCam = Camera.main ?? GameObject.FindObjectOfType<Camera>();
            if (mainCam != null)
            {
                mainCam.tag = "MainCamera";
                mainCam.enabled = true;
                mainCam.gameObject.SetActive(true);

                // Restore camera parent and transform
                if (pc.cameraGameObject != null)
                {
                    mainCam.transform.SetParent(pc.cameraGameObject.transform);
                    mainCam.transform.localPosition = Vector3.zero;
                    mainCam.transform.localRotation = Quaternion.identity;
                    mainCam.transform.localScale = Vector3.one;
                }

                mls.LogInfo("Camera moved back to player.");
            }
            else
            {
                mls.LogWarning("No main camera found when trying to move back to player.");
            }

            if (SpectateCamera.instance != null)
            {
                SpectateCamera.instance.StopSpectate();
                mls.LogInfo("Stopped spectate camera.");
            }

            pc.enabled = true;

            if (pc.cameraGameObject != null)
                pc.cameraGameObject.SetActive(true);

            if (pc.cameraGameObjectLocal != null)
                pc.cameraGameObjectLocal.SetActive(true);

            // Restore camera aim
            DelayUtility.RunAfterDelay(0.25f, () =>
            {
                CameraAim.Instance?.CameraAimSpawn(pc.transform.eulerAngles.y);
            });
        }

        public static void ReleaseDuckControlToSpectate()
        {
            if (PublicVars.DuckCleanupInProgress)
            {
                mls.LogInfo("Duck cleanup already in progress — skipping duplicate call of ReleaseDuckControlToSpectate");
                return;
            }

            PublicVars.DuckCleanupInProgress = true;
            ReattatchCameraToPlayer();

            DuckPlayerController duckController = GameObject.FindObjectOfType<DuckPlayerController>();
            if (duckController != null)
            {
                GameObject.Destroy(duckController);
                mls.LogInfo("Duck controller destroyed.");
            }

            if (PlayerAvatar.instance != null && ReflectionUtils.GetFieldValue<bool>(PlayerAvatar.instance, "deadSet"))
            {
                mls.LogInfo("Calling SetSpectate to enter true spectator mode...");
                PlayerAvatar.instance.SetSpectate();

                DelayUtility.RunAfterDelay(0.25f, () =>
                {
                    SpectateCamera cam = SpectateCamera.instance;
                    if (cam != null)
                    {
                        bool isInNormal = ReflectionUtils.InvokeMethod<bool>(cam, "CheckState", new object[] { Enum.Parse(typeof(SpectateCamera.State), "Normal") });

                        Transform spectatePoint = PlayerAvatar.instance?.spectatePoint;
                        if (spectatePoint != null)
                        {
                            if (!isInNormal)
                            {
                                ReflectionUtils.InvokeMethod(cam, "UpdateState", new object[] { Enum.Parse(typeof(SpectateCamera.State), "Normal") });
                                mls.LogWarning("SpectateCamera was not in Normal state — forcing it.");
                            }

                            Camera mainCam = ReflectionUtils.GetFieldValue<Camera>(cam, "MainCamera");
                            Transform followTransform = cam.normalTransformDistance;

                            if (mainCam != null && followTransform != null)
                            {
                                mainCam.transform.SetParent(followTransform);
                                mainCam.transform.localPosition = Vector3.zero;
                                mainCam.transform.localRotation = Quaternion.identity;
                                mainCam.tag = "MainCamera";
                                mainCam.enabled = true;
                                mainCam.gameObject.SetActive(true);
                                mainCam.clearFlags = CameraClearFlags.Skybox;
                                mainCam.backgroundColor = Color.black;
                            }
                            else
                            {
                                mls.LogWarning("Missing MainCamera or follow transform during spectate setup.");
                            }
                        }
                    }
                    else
                    {
                        mls.LogWarning("SpectateCamera.instance was null.");
                    }

                    PublicVars.DuckCleanupInProgress = false;
                });
            }
            else
            {
                mls.LogWarning("PlayerAvatar.instance was null when trying to spectate.");
                PublicVars.DuckCleanupInProgress = false;
            }
        }

        public static void SpawnDuckAt(Vector3 spawnPos, int actorNumber)
        {
            mls.LogMessage($"Spawning duck at {spawnPos}");

            string duckPrefabPath = "Enemies/Enemy - Duck";
            GameObject duckPrefab = Resources.Load<GameObject>(duckPrefabPath);
            if (duckPrefab == null)
            {
                mls.LogError($"Duck prefab not found at path: {duckPrefabPath}");
                return;
            }

            EnemySetup duckSetup = ScriptableObject.CreateInstance<EnemySetup>();
            duckSetup.spawnObjects = new List<GameObject> { duckPrefab };

            //GameObject duck = PhotonNetwork.Instantiate("Enemies/Enemy - Duck", position, rotation);
            ReflectionUtils.InvokeMethod(LevelGenerator.Instance, "EnemySpawn", new object[] { duckSetup, spawnPos });
            mls.LogInfo("Duck spawned successfully.");

            EnemyDuck duck = null;
            // Move the duck to the player after delay
            DelayUtility.RunAfterDelay(10f, () =>
            {
                duck = GeneralUtil.FindClosestDuckWithoutController(spawnPos);
                GeneralUtil.MoveDuckToPos(spawnPos);
            });

            //take over the duck
            DelayUtility.RunUntil(() =>
            {
                if (duck == null)
                    return false;

                var dist = Vector3.Distance(duck.transform.position, spawnPos);
                mls.LogMessage($"Duck distance: {dist} from goal");
                return dist < 1.5f;
            }, () =>
            {
                GeneralUtil.ControlClosestDuck(spawnPos, actorNumber);
                Photon.Realtime.Player targetPlayer = PhotonNetwork.CurrentRoom.Players.ContainsKey(actorNumber)
                    ? PhotonNetwork.CurrentRoom.Players[actorNumber]
                    : null;

                if (targetPlayer == null)
                {
                    mls.LogError($"Target player with actor number {actorNumber} not found.");
                    return;
                }
            }, timeoutSeconds: 60f, onTimeout: () =>
            {
                mls.LogWarning("Duck never reached goal, attempting to control anyway...");
                GeneralUtil.ControlClosestDuck(spawnPos, actorNumber);
            });
        }
    }
}
