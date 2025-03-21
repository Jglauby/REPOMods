using BepInEx.Logging;
using OpJosModREPO.IAmDucky;
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

        public static void ReleaseDuckControl()
        {
            PlayerController pc = PlayerController.instance;

            // Re-enable player control
            pc.enabled = true;

            // Reactivate camera system
            pc.cameraGameObject.SetActive(true);
            pc.cameraGameObjectLocal.SetActive(true);

            // Re-parent main camera to player camera holder
            Camera.main.transform.SetParent(pc.cameraGameObject.transform, worldPositionStays: false);
            Camera.main.transform.localPosition = Vector3.zero;
            Camera.main.transform.localRotation = Quaternion.identity;

            // Destroy the duck controller if it exists
            DuckPlayerController duckController = GameObject.FindObjectOfType<DuckPlayerController>();
            if (duckController != null)
            {
                GameObject.Destroy(duckController);
            }

            // Optional: Hide mouse
            //Cursor.lockState = CursorLockMode.Locked;
            //Cursor.visible = false;

            mls.LogInfo("Control released from the duck.");
        }
    }
}
