using BepInEx.Logging;
using OpJosModREPO.Util;
using Photon.Pun;
using REPOMods;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OpJosModREPO.IAmDucky
{
    public class DuckPlayerController : MonoBehaviour
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        private Rigidbody rb;
        private Vector3 moveDirection;
        public float moveSpeed = 5f;
        public float turnSpeed = 3f;
        public float jumpForce = 80f;
        public float gravity = 9.8f;
        public Transform cameraTransform;
        public float mouseSensitivity = 0.25f;
        private float cameraPitch = 0f;

        public Vector3 cameraOffset = new Vector3(0, 1.5f, -1.5f); 
        public float cameraSmoothSpeed = 10f;

        private EnemyDuck thisDuck = null;
        private float attackCooldown;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            cameraTransform = Camera.main.transform; // Get the main camera

            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();  // Ensure the duck has a Rigidbody
                rb.mass = 5f;
                rb.drag = 1.25f;
                rb.angularDrag = 0.5f;
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                rb.useGravity = true;
            }

            Cursor.lockState = CursorLockMode.Locked; // Hide cursor and lock to center
            Cursor.visible = false;
        }

        void Update()
        {
            // Handle mouse look
            float mouseX = Mouse.current.delta.x.ReadValue() * mouseSensitivity;
            float mouseY = Mouse.current.delta.y.ReadValue() * mouseSensitivity;

            cameraPitch -= mouseY;
            cameraPitch = Mathf.Clamp(cameraPitch, -60f, 60f); // Prevent flipping

            transform.Rotate(Vector3.up * mouseX); // Rotate duck
            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0, 0); // Rotate camera

            // Get movement input
            Vector2 moveInput = Keyboard.current != null
                ? new Vector2(Keyboard.current.aKey.isPressed ? -1 : Keyboard.current.dKey.isPressed ? 1 : 0,
                              Keyboard.current.sKey.isPressed ? -1 : Keyboard.current.wKey.isPressed ? 1 : 0)
                : Vector2.zero;

            moveDirection = transform.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y).normalized);

            handleInput();
        }

        void FixedUpdate()
        {
            // Apply movement
            rb.AddForce(new Vector3(moveDirection.x * moveSpeed, 0, moveDirection.z * moveSpeed), ForceMode.Acceleration);

            // Directly set the camera position behind the duck
            cameraTransform.position = transform.position + transform.TransformDirection(cameraOffset);

            if (thisDuck == null)
            {
                thisDuck = GeneralUtil.FindClosestDuck(cameraTransform.position);
            }

            if (attackCooldown > 0f)
                attackCooldown -= Time.fixedDeltaTime;

            if (attackCooldown <= 0f)
            {
                attackNearbyEnemies();
                attackCooldown = 0.75f;
            }
        }

        private void handleInput()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                EnemyDuck thisDuck = GeneralUtil.FindClosestDuck(cameraTransform.position);
                if (thisDuck == null) return;

                Enemy enemy = thisDuck.enemy;
                if (enemy == null) return;

                object enemyJump = ReflectionUtils.GetFieldValue<object>(enemy, "Jump");
                if (enemyJump == null) return;

                ReflectionUtils.InvokeMethod(enemyJump, "StuckTrigger", new object[] { Vector3.up });
            }

            if (Keyboard.current.cKey.wasPressedThisFrame)
            {
                mls.LogInfo("Reseting control of duck");
                GeneralUtil.ControlClosestDuck(cameraTransform.position);
            }

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                thisDuck = GeneralUtil.FindClosestDuck(cameraTransform.position);

                if (thisDuck.currentState == EnemyDuck.State.AttackStart)
                {
                    mls.LogInfo("Stopping duck attack mode");
                    ReflectionUtils.InvokeMethod(thisDuck, "UpdateState", new object[] { EnemyDuck.State.Idle });
                }
                else
                {
                    mls.LogInfo("Starting duck attack mode");
                    ReflectionUtils.InvokeMethod(thisDuck, "UpdateState", new object[] { EnemyDuck.State.AttackStart });
                }
            }

            //add a leave duck button
        }

        private void attackNearbyEnemies()
        {
            if (thisDuck.currentState == EnemyDuck.State.AttackStart)
            {
                List<Enemy> closeEnemies = GeneralUtil.FindCloseEnemies(thisDuck.transform.position, 2f);
                foreach (var enemy in closeEnemies)
                {
                    if (enemy != null && enemy.GetInstanceID() != thisDuck.enemy.GetInstanceID())//not controlled duck
                    {
                        Vector3 toEnemy = (enemy.transform.position - thisDuck.transform.position).normalized;
                        float angle = Vector3.Angle(thisDuck.transform.forward, toEnemy);

                        if (angle < 50f)
                        {
                            EnemyHealth healthComponent = ReflectionUtils.GetFieldValue<EnemyHealth>(enemy, "Health");
                            if (healthComponent != null)
                            {
                                // Get direction from duck to enemy
                                Vector3 hurtDir = (enemy.transform.position - thisDuck.transform.position).normalized;

                                // Call internal method "Hurt"
                                healthComponent.Hurt(20, hurtDir);
                            }
                            else
                            {
                                mls.LogError($"Health component not found for enemy: {enemy.name}");
                            }
                        }
                    }
                }
            }
        }
    }

}
