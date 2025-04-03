using BepInEx.Logging;
using OpJosModREPO.IAmDucky.Networking;
using OpJosModREPO.Util;
using Photon.Pun;
using REPOMods;
using System;
using System.Collections.Generic;
using System.Threading;
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

        private EnemyRigidbody erb;
        private Rigidbody rb;

        private Vector3 moveDirection;
        public float moveSpeed = 6f;
        public float turnSpeed = 3f;
        public float jumpForce = 0.5f;
        public Transform cameraTransform;
        public float mouseSensitivity = 0.25f;
        private float cameraPitch = 0f;

        public Vector3 cameraOffset = new Vector3(0, 1.75f, -1.75f); 
        public float cameraSmoothSpeed = 15f;

        public EnemyDuck thisDuck = null;
        public int controlActorNumber;
        private float attackCooldown;

        private bool isYourDuck = false; //if false means you are host, no client has this controller if it isn't for them
        private bool isHost = false;

        private float syncTimer = 0f;
        private float syncInterval = 0.375f;

        private Vector3 targetLookDirection;
        private bool shouldJump = false;
        private bool slowFall = false;

        public void Setup(int actorNumber, EnemyDuck duck)
        {
            controlActorNumber = actorNumber;
            thisDuck = duck;
            isHost = PhotonNetwork.IsMasterClient;

            erb = ReflectionUtils.GetFieldValue<EnemyRigidbody>(thisDuck.enemy, "Rigidbody");
            rb = ReflectionUtils.GetFieldValue<Rigidbody>(erb, "rb");

            rb.drag = 5000f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rb.useGravity = true;

            if (PhotonNetwork.LocalPlayer.ActorNumber == controlActorNumber) //is your duck
            {
                isYourDuck = true;
                PlayerController.instance.enabled = false;
                Camera.main.transform.SetParent(duck.gameObject.transform);

                Camera.main.transform.localPosition = cameraOffset;
                Camera.main.transform.localRotation = Quaternion.identity;

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            cameraTransform = Camera.main.transform; // Get the main camera
        }

        public void UpdateMovementAndRotation(Vector3 movement, Vector3 camForward, bool jump)
        {
            moveDirection = movement;
            targetLookDirection = camForward;

            if (jump)
            {
                TriggerJump();
            }
        }

        void Start()
        {
        }

        void Update()
        {
            if (!isYourDuck)
                return;

            Vector2 moveInput = Keyboard.current != null
                ? new Vector2(Keyboard.current.aKey.isPressed ? -1 : Keyboard.current.dKey.isPressed ? 1 : 0,
                    Keyboard.current.sKey.isPressed ? -1 : Keyboard.current.wKey.isPressed ? 1 : 0)
                : Vector2.zero;

            moveDirection = transform.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y).normalized);

            // Handle mouse look
            float mouseX = Mouse.current.delta.x.ReadValue() * mouseSensitivity;
            float mouseY = Mouse.current.delta.y.ReadValue() * mouseSensitivity;

            cameraPitch -= mouseY;
            cameraPitch = Mathf.Clamp(cameraPitch, -60f, 60f); // Prevent flipping

            transform.Rotate(Vector3.up * mouseX); // Rotate duck
            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0, 0); // Rotate camera

            //send move data to host rpc
            shouldJump = Keyboard.current.spaceKey.wasPressedThisFrame == true ? true : shouldJump;
            if (!isHost)
            {
                syncTimer += Time.deltaTime;
                if (syncTimer >= syncInterval)
                {
                    Vector3 camForward = cameraTransform.forward;
                    camForward.y = 0f;
                    camForward.Normalize();

                    DuckSpawnerNetwork.Instance.SendDuckMovement(moveDirection, camForward, controlActorNumber, shouldJump);
                    shouldJump = false;
                    syncTimer = 0f;
                }
            }

            handleInput();
        }

        void FixedUpdate()
        {
            if (isYourDuck || isHost)
            {
                if (targetLookDirection != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(targetLookDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
                }
            }

            if (isHost)
            {
                erb.DisableFollowPosition(0.5f, 50f);
                rb.AddForce(new Vector3(moveDirection.x * moveSpeed, 0, moveDirection.z * moveSpeed), ForceMode.Acceleration);

                if (moveDirection.x == 0 && moveDirection.z == 0)
                {
                    Vector3 velocity = rb.velocity;
                    Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);

                    // Dampen the horizontal speed gradually (like friction)
                    horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, Time.fixedDeltaTime * 5f);

                    // Apply the damped velocity back
                    rb.velocity = new Vector3(horizontalVelocity.x, velocity.y, horizontalVelocity.z);
                }

                //sticks object camera is on to rigidbody thats moving
                thisDuck.gameObject.transform.position = rb.transform.position;


                //if (slowFall)
                //{
                //    rb.AddForce(Physics.gravity * .01f, ForceMode.Acceleration);
                //}
            }

            if (isYourDuck)
            {
                cameraTransform.position = new Vector3(cameraTransform.position.x, rb.transform.position.y + cameraOffset.y, cameraTransform.position.z);

                if (attackCooldown > 0f)
                    attackCooldown -= Time.fixedDeltaTime;

                if (attackCooldown <= 0f)
                {
                    attackNearbyEnemies();
                    attackCooldown = 0.75f;
                }
            }
        }

        private void handleInput()
        {
            if (controlActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)//dont listen to keys if not your duck
                return;

            if (Keyboard.current.spaceKey.wasPressedThisFrame && PhotonNetwork.IsMasterClient)
            {
                TriggerJump();
            }

            if (Keyboard.current.cKey.wasPressedThisFrame)
            {
                mls.LogInfo("Reseting control of duck");
                if (PhotonNetwork.IsMasterClient)
                {
                    GeneralUtil.ControlClosestDuck(cameraTransform.position, 1);
                }
                else
                {
                    DuckSpawnerNetwork.Instance.ResetDuckControl(cameraTransform.position, controlActorNumber);
                }
            }

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
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

            if (Keyboard.current.kKey.wasPressedThisFrame)
            {
                EnemyHealth healthComponent = ReflectionUtils.GetFieldValue<EnemyHealth>(thisDuck.enemy, "Health");
                ReflectionUtils.InvokeMethod(healthComponent, "Death", new object[] { Vector3.zero });
                mls.LogMessage("Killed controlled duck");
            }
        }

        private void attackNearbyEnemies()
        {
            if (thisDuck.currentState == EnemyDuck.State.AttackStart)
            {
                List<Enemy> closeEnemies = GeneralUtil.FindCloseEnemies(thisDuck.transform.position, 2.25f);
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
                                healthComponent.Hurt(25, hurtDir);
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

        private void TriggerJump()
        {
            if (thisDuck == null) return;

            Enemy enemy = thisDuck.enemy;
            if (enemy == null) return;

            object enemyJump = ReflectionUtils.GetFieldValue<object>(enemy, "Jump");
            if (enemyJump == null) return;

            ReflectionUtils.InvokeMethod(enemyJump, "StuckTrigger", new object[] { Vector3.up });
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}
