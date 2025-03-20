﻿using BepInEx.Logging;
using OpJosModREPO.IAmDucky;
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

        public static void ControlClosestDuck()
        {
            GameObject closestDuck = FindClosestDuck(PlayerAvatar.instance.transform.position)?.gameObject;

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
    }
}
