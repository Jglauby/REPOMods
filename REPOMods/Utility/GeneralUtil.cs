using BepInEx.Logging;
using OpJosModREPO.Controllers.IAmEnemy;
using OpJosModREPO.IAmEnemy.Networking;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

namespace OpJosModREPO.IAmEnemy.Util
{
    public static class GeneralUtil
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        public static Enemy FindClosestEnemy(Vector3 pos, EnemyTypes type)
        {
            Type targetType = EnemyMaps.GetEnemyType(type);
            if (targetType == null)
            {
                mls.LogWarning($"Unknown enemy type: {type}");
                return null;
            }

            Enemy closest = null;
            float closestDist = float.MaxValue;

            foreach (var enemy in GameObject.FindObjectsOfType<Enemy>())
            {
                if (enemy == null || enemy.GetComponent(targetType) == null)
                    continue;

                if (HasController(enemy))
                    continue;

                float dist = Vector3.Distance(enemy.transform.position, pos);
                if (dist < closestDist)
                {
                    closest = enemy;
                    closestDist = dist;
                }
            }

            return closest;
        }

        public static Enemy FindClosestEnemyWithoutController(Vector3 pos, EnemyTypes type)
        {
            Enemy closest = null;
            float closestDist = float.MaxValue;

            foreach (var enemy in GameObject.FindObjectsOfType<Enemy>())
            {
                var actualType = EnemyMaps.GetEnemyTypeFromInstance(enemy);
                if (actualType == null || actualType != type)
                    continue;

                if (HasController(enemy))
                    continue;

                float dist = Vector3.Distance(enemy.transform.position, pos);
                if (dist < closestDist)
                {
                    closest = enemy;
                    closestDist = dist;
                }
            }

            return closest;
        }

        public static bool HasController(Enemy enemy)
        {
            foreach (var controller in GameObject.FindObjectsOfType<EnemyControllerBase>())
            {
                if (controller != null && controller.thisEnemyGameObject != null 
                    && controller.thisEnemyGameObject.GetInstanceID() == enemy?.gameObject.GetInstanceID())
                    return true;
            }

            return false;
        }

        public static EnemyControllerBase FindEnemyController(int? actorNumber)
        {
            foreach (var controller in GameObject.FindObjectsOfType<EnemyControllerBase>())
            {
                if (controller.controlActorNumber == actorNumber)
                {
                    return controller;
                }
            }

            return null;
        }

        public static EnemyControllerBase FindEnemyController(Enemy enemy)
        {
            foreach (var controller in GameObject.FindObjectsOfType<EnemyControllerBase>())
            {
                if (controller.thisEnemyEnemy.GetInstanceID() == enemy.GetInstanceID())
                {
                    return controller;
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

        public static void MoveEnemyToPos(Enemy enemy, Vector3 pos)
        {
            if (enemy == null)
            {
                mls.LogError("No enemy provided to move.");
                return;
            }

            GameObject enemyObject = enemy.gameObject;
            if (enemyObject == null)
            {
                mls.LogError("Enemy's GameObject is null.");
                return;
            }

            mls.LogMessage($"Found enemy at {enemyObject.transform.position}, moving it to {pos}.");

            // Try disabling enemy AI if supported
            var duckAI = enemy.GetComponent<EnemyDuck>();
            if (duckAI != null)
            {
                duckAI.enabled = false;
                duckAI.currentState = EnemyDuck.State.Idle;
                ReflectionUtils.SetFieldValue(duckAI, "playerTarget", null);
            }

            // Set NavMesh destination
            NavMeshAgent agent = enemyObject.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.SetDestination(pos);
            }
            else
            {
                mls.LogWarning("Enemy has no NavMeshAgent; cannot set destination.");
            }

            // Prevent despawn
            EnemyParent enemyParent = ReflectionUtils.GetFieldValue<EnemyParent>(enemy, "EnemyParent");
            if (enemyParent != null)
            {
                enemyParent.SpawnedTimer = float.PositiveInfinity;
            }
            else
            {
                mls.LogWarning("Could not access EnemyParent to prevent despawning.");
            }

            mls.LogMessage($"Enemy moving toward {pos}.");
        }

        public static void ControlClosestEnemy(Vector3 pos, int actorNumber, EnemyTypes enemyType)
        {
            //player is dead, and it is not the host setting up someone elses controller
            if (!ReflectionUtils.GetFieldValue<bool>(PlayerAvatar.instance, "deadSet") && PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
            {
                mls.LogWarning("Player is not dead, cannot control enemy.");
                return;
            }

            Enemy closestEnemy = FindClosestEnemy(pos, enemyType);
            if (closestEnemy != null)
            {
                mls.LogInfo($"Found closest duck at {closestEnemy.gameObject.transform.position}, transferring control to player.");

                // Transfer control: Add PlayerController to enemy
                BreakEnemyAI(closestEnemy);
                Type controllerType = EnemyMaps.GetControllerType(enemyType);
                if (controllerType == null)
                {
                    mls.LogError($"No controller mapped for enemy type {enemyType}.");
                    return;
                }

                if (closestEnemy.gameObject.GetComponent(controllerType) is MonoBehaviour controller)
                {
                    mls.LogInfo("Controller already exists, using existing one.");
                }
                else
                {
                    controller = (MonoBehaviour)closestEnemy.gameObject.AddComponent(controllerType);
                }

                Type expectedEnemyComponent = EnemyMaps.GetEnemyType(enemyType);
                Component specificEnemy = closestEnemy.GetComponent(expectedEnemyComponent);
                if (specificEnemy == null)
                {
                    mls.LogError($"Enemy is missing expected component of type {expectedEnemyComponent}.");
                    return;
                }
                
                ReflectionUtils.InvokeMethod(controller, "Setup", new object[] { actorNumber, specificEnemy });

                mls.LogInfo("Control transferred to the duck.");
            }
            else
            {
                mls.LogInfo("No duck found to transfer control.");
            }
        }

        public static void BreakEnemyAI(Enemy enemy)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            if (enemy == null)
            {
                mls.LogError("enemy is null, cannot break AI.");
                return;
            }

            mls.LogInfo($"Breaking AI for enemy of type {enemy.GetType().Name}");

            MonoBehaviour foundAI = null;
            FieldInfo stateField = null;

            foreach (var comp in enemy.GetComponents<MonoBehaviour>())
            {
                if (comp == null) continue;
                stateField = comp.GetType().GetField("currentState", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (stateField != null)
                {
                    foundAI = comp;
                    break;
                }
            }

            if (foundAI != null)
            {
                foundAI.enabled = false;
                mls.LogInfo($"Disabled component {foundAI.GetType().Name}");

                try
                {
                    Type enumType = stateField.FieldType;
                    object idleValue = Enum.Parse(enumType, "Idle");
                    ReflectionUtils.SetFieldValue(foundAI, "currentState", idleValue);
                }
                catch (Exception ex)
                {
                    mls.LogWarning($"Could not set currentState to Idle: {ex.Message}");
                }
            }
            else
            {
                mls.LogWarning("Could not find currentState field.");
            }

            EnemyRigidbody enemyrb = enemy.GetComponentInChildren<EnemyRigidbody>(true);
            if (enemyrb != null)
            {
                enemyrb.enabled = false;
                Rigidbody rb = ReflectionUtils.GetFieldValue<Rigidbody>(enemyrb, "rb");

                rb.drag = 5000f;
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                rb.useGravity = true;
            }
            else
            {
                mls.LogWarning("No EnemyRigidbody found.");
            }

            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.isStopped = true;
                agent.enabled = false;
                mls.LogInfo("Disabled NavMeshAgent.");
            }

            mls.LogInfo("Enemy AI break complete.");
        }

        public static void EnableEnemyAI(Enemy enemy)
        {
            if (enemy == null)
            {
                mls.LogError("enemy is null, cannot restore AI.");
                return;
            }

            mls.LogInfo($"Restoring AI for enemy of type {enemy.GetType().Name}");

            MonoBehaviour foundAI = null;
            FieldInfo stateField = null;

            foreach (var comp in enemy.GetComponents<MonoBehaviour>())
            {
                if (comp == null) continue;
                stateField = comp.GetType().GetField("currentState", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (stateField != null)
                {
                    foundAI = comp;
                    break;
                }
            }

            if (foundAI != null)
            {
                foundAI.enabled = true;
                mls.LogInfo($"Enabled component {foundAI.GetType().Name}");

                try
                {
                    Type enumType = stateField.FieldType;
                    object roamValue = Enum.Parse(enumType, "Roam");
                    ReflectionUtils.SetFieldValue(foundAI, "currentState", roamValue);
                }
                catch (Exception ex)
                {
                    mls.LogWarning($"Could not set currentState to Roam: {ex.Message}");
                }
            }
            else
            {
                mls.LogWarning("Could not find currentState field.");
            }

            EnemyRigidbody enemyrb = enemy.GetComponentInChildren<EnemyRigidbody>(true);
            if (enemyrb != null)
            {
                enemyrb.enabled = true;
                Rigidbody rb = ReflectionUtils.GetFieldValue<Rigidbody>(enemyrb, "rb");

                rb.constraints = RigidbodyConstraints.FreezeAll;
                rb.useGravity = false;
                rb.drag = 0;
            }
            else
            {
                mls.LogWarning("No EnemyRigidbody found.");
            }

            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.isStopped = false;
                agent.enabled = true;
                agent.ResetPath();
                mls.LogInfo("Re-enabled NavMeshAgent.");
            }

            mls.LogInfo("Enemy AI restore complete.");
        }

        public static void RemoveSpawnedControllableEnemy(EnemyControllerBase enemyController)
        {
            if (enemyController == null)
            {
                mls.LogWarning("Duck controller is null, cannot destroy.");
                return;
            }

            GameObject.Destroy(enemyController);
            mls.LogInfo("Duck controller destroyed.");

            if (PhotonNetwork.IsMasterClient)
            {
                PlayerController pc = PlayerController.instance;
                if (enemyController.thisEnemyGameObject != null)
                {
                    EnemyHealth healthComponent = ReflectionUtils.GetFieldValue<EnemyHealth>(enemyController.thisEnemyEnemy, "Health");
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

        public static void ReleaseEnemyControlToSpectate()
        {
            if (PublicVars.EnemyCleanupInProgress)
            {
                mls.LogInfo("Enemy cleanup already in progress — skipping duplicate call of ReleaseEnemyControlToSpectate");
                return;
            }

            PublicVars.EnemyCleanupInProgress = true;
            ReattatchCameraToPlayer();

            // Look for and destroy *any* custom controller using the EnemyMaps
            foreach (var kvp in EnemyMaps.ControllerTypes)
            {
                Type controllerType = kvp.Value;
                MonoBehaviour controllerInstance = GameObject.FindObjectOfType(controllerType) as MonoBehaviour;
                if (controllerInstance != null)
                {
                    GameObject.Destroy(controllerInstance);
                    mls.LogInfo($"Destroyed enemy controller of type {controllerType.Name}.");
                }
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

                    PublicVars.EnemyCleanupInProgress = false;
                });
            }
            else
            {
                mls.LogWarning("PlayerAvatar.instance was null when trying to spectate.");
                PublicVars.EnemyCleanupInProgress = false;
            }
        }

        public static void SpawnEnemyAt(Vector3 spawnPos, int actorNumber, EnemyTypes enemyType)
        {
            mls.LogMessage($"Spawning enemy at {spawnPos}");

            string enemyPrefabPath = EnemyMaps.GetPrefabPath(enemyType);
            GameObject enemyPrefab = Resources.Load<GameObject>(enemyPrefabPath);
            if (enemyPrefab == null)
            {
                mls.LogError($"Enemy prefab not found at path: {enemyPrefab}");
                return;
            }

            GameObject gameObject = ((GameManager.instance.gameMode != 0) ? PhotonNetwork.InstantiateRoomObject("Enemies/" + enemyPrefab.name, spawnPos, Quaternion.identity, 0)
                : UnityEngine.Object.Instantiate(enemyPrefab, spawnPos, Quaternion.identity));
            EnemyParent component = gameObject.GetComponent<EnemyParent>();

            if ((bool)component)
            {
                ReflectionUtils.SetFieldValue(component, "SetupDone", true);
                gameObject.GetComponentInChildren<Enemy>().EnemyTeleported(spawnPos);
                ReflectionUtils.SetFieldValue(LevelGenerator.Instance, "EnemiesSpawnTarget", ReflectionUtils.GetFieldValue<int>(LevelGenerator.Instance, "EnemiesSpawnTarget") + 1);
                EnemyDirector.instance.FirstSpawnPointAdd(component);
            }
            mls.LogInfo("Enemy spawned successfully.");

            Enemy targetEnemy = null;
            // Move the enemy to the player after delay
            DelayUtility.RunAfterDelay(10f, () =>
            {
                targetEnemy = FindClosestEnemyWithoutController(spawnPos, enemyType);
                MoveEnemyToPos(targetEnemy, spawnPos);
            });

            //take over the enemy
            DelayUtility.RunUntil(() =>
            {
                if (targetEnemy == null)
                    return false;

                var dist = Vector3.Distance(targetEnemy.transform.position, spawnPos);
                mls.LogMessage($"Duck distance: {dist} from goal");
                return dist < 1.5f;
            }, () =>
            {
                GeneralUtil.ControlClosestEnemy(spawnPos, actorNumber, EnemyTypes.Duck);
                Photon.Realtime.Player targetPlayer = PhotonNetwork.CurrentRoom.Players.ContainsKey(actorNumber)
                    ? PhotonNetwork.CurrentRoom.Players[actorNumber]
                    : null;

                if (targetPlayer == null)
                {
                    mls.LogError($"Target player with actor number {actorNumber} not found.");
                    return;
                }

                EnemySpawnerNetwork.Instance.ControlEnemy(spawnPos, actorNumber, EnemyTypes.Duck);
            }, timeoutSeconds: 60f, onTimeout: () =>
            {
                mls.LogWarning("Duck never reached goal, attempting to control anyway...");
                GeneralUtil.ControlClosestEnemy(spawnPos, actorNumber, EnemyTypes.Duck);
                EnemySpawnerNetwork.Instance.ControlEnemy(spawnPos, actorNumber, EnemyTypes.Duck);
            });
        }
    }
}
