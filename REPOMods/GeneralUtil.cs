﻿using BepInEx.Logging;
using OpJosModREPO.IAmDucky;
using OpJosModREPO.Util;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace REPOMods
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
            GameObject closestDuck = GeneralUtil.FindClosestDuck(pos)?.gameObject;

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
                    agent.Warp(pos);
                }

                mls.LogMessage($"Duck moving towards {closestDuck.transform.position}");
            }
            else
            {
                mls.LogError("No duck found to teleport.");
            }
        }

        public static void ControlClosestDuck(Vector3 pos)
        {
            GameObject closestDuck = FindClosestDuck(pos)?.gameObject;

            if (closestDuck != null)
            {
                mls.LogInfo($"Found closest duck at {closestDuck.transform.position}, transferring control to player.");

                // Disable Player's control & collision
                PlayerController.instance.enabled = false;

                // Transfer control: Add PlayerController to Duck
                DuckPlayerController duckPlayerController = closestDuck.GetComponent<DuckPlayerController>();
                if (duckPlayerController == null)
                {
                    duckPlayerController = closestDuck.AddComponent<DuckPlayerController>();
                }

                // Set the player camera to follow the duck
                Camera.main.transform.SetParent(closestDuck.transform);
                Camera.main.transform.localPosition = new Vector3(0, 1, -2); // Adjust position
                Camera.main.transform.localRotation = Quaternion.identity;

                NavMeshAgent agent = closestDuck.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    agent.isStopped = true;  // Stop AI pathfinding
                    agent.enabled = false;   // Disable NavMeshAgent
                }

                // Log the transfer
                mls.LogInfo("Control transferred to the duck.");
            }
            else
            {
                mls.LogInfo("No duck found to transfer control.");
            }
        }

        public static void RemoveSpawnedControllableDuck()
        {
            // Now safely destroy the duck controller
            DuckPlayerController duckController = GameObject.FindObjectOfType<DuckPlayerController>();
            if (duckController != null)
            {
                GameObject.Destroy(duckController);
                mls.LogInfo("Duck controller destroyed.");
            }

            // Kill the duck after camera is restored
            PlayerController pc = PlayerController.instance;
            EnemyDuck closestDuck = FindClosestDuck(pc.transform.position);
            if (closestDuck != null)
            {
                EnemyHealth healthComponent = ReflectionUtils.GetFieldValue<EnemyHealth>(closestDuck.enemy, "Health");
                ReflectionUtils.InvokeMethod(healthComponent, "Death", new object[] { Vector3.zero });
                mls.LogMessage("Killed controlled duck");
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
            Camera mainCam = Camera.main ?? GameObject.FindObjectOfType<Camera>();

            if (mainCam != null)
            {
                mainCam.tag = "MainCamera";
                mainCam.enabled = true;
                mainCam.gameObject.SetActive(true);

                // Unparent and position the camera in a nice spot to spectate
                mainCam.transform.SetParent(null); // No parent
                mainCam.transform.position = new Vector3(0, 10, -10); // Or some fallback/default position
                mainCam.transform.rotation = Quaternion.Euler(30f, 0f, 0f); // Slight top-down view

                // Optionally look at a spot (like the duck's last position)
                EnemyDuck duck = FindClosestDuck(mainCam.transform.position);
                if (duck != null)
                {
                    Vector3 duckPos = duck.transform.position;
                    mainCam.transform.position = duckPos + new Vector3(0, 8f, -8f);
                    mainCam.transform.LookAt(duckPos + Vector3.up * 1f);
                }

                // Disable any follow/aim scripts
                CameraAim camAim = mainCam.GetComponent<CameraAim>();
                if (camAim != null)
                    camAim.enabled = false;

                mls.LogInfo("Camera switched to spectator mode.");
            }
            else
            {
                mls.LogWarning("No camera found to use for spectating.");
            }

            // Destroy Duck Controller if it exists
            DuckPlayerController duckController = GameObject.FindObjectOfType<DuckPlayerController>();
            if (duckController != null)
            {
                GameObject.Destroy(duckController);
                mls.LogInfo("Duck controller destroyed.");
            }
        }
    }
}
