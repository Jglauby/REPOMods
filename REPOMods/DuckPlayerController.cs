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

        public EnemyDuck thisDuck = null;
        public int controlActorNumber;
        private float attackCooldown;

        private bool isYourDuck = false; //if false means you are host, no client has this controller if it isn't for them
        private bool isHost = false;

        private float syncTimer = 0f;
        private float syncInterval = 0.4f;

        private float recievedMouseX;
        private bool shouldJump = false;

        public void Setup(int actorNumber, EnemyDuck duck)
        {
            controlActorNumber = actorNumber;
            thisDuck = duck;
            isHost = PhotonNetwork.IsMasterClient;

            if (PhotonNetwork.LocalPlayer.ActorNumber == controlActorNumber) //is your duck
            {
                SpectateCamera.instance.StopSpectate();
                ReflectionUtils.SetFieldValue(PlayerAvatar.instance, "spectating", false);

                isYourDuck = true;
                PlayerController.instance.enabled = false;
                Camera.main.transform.SetParent(duck.gameObject.transform);
                Camera.main.transform.localPosition = new Vector3(0, 1, -2);
                Camera.main.transform.localRotation = Quaternion.identity;

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

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
        }

        public void UpdateMovementAndRotation(Vector3 movement, float mouseX, bool jump)
        {
            moveDirection = movement;
            recievedMouseX = mouseX;

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

            shouldJump = Keyboard.current.spaceKey.wasPressedThisFrame == true ? true : shouldJump;
            if (!isHost)
            {
                syncTimer += Time.deltaTime;
                if (syncTimer >= syncInterval)
                {
                    float mouse = Mouse.current.delta.x.ReadValue() * mouseSensitivity;
                    DuckSpawnerNetwork.Instance.SendDuckMovement(moveDirection, mouse, controlActorNumber, shouldJump);
                    shouldJump = false;
                    syncTimer = 0f;
                }
            }

            // Handle mouse look
            float mouseX = Mouse.current.delta.x.ReadValue() * mouseSensitivity;
            float mouseY = Mouse.current.delta.y.ReadValue() * mouseSensitivity;

            cameraPitch -= mouseY;
            cameraPitch = Mathf.Clamp(cameraPitch, -60f, 60f); // Prevent flipping

            recievedMouseX = mouseX;
            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0, 0); // Rotate camera

            handleInput();          
        }

        void FixedUpdate()
        {
            if (isYourDuck || isHost)
            {
                transform.Rotate(Vector3.up * recievedMouseX); // Rotate duck
            }

            if (isHost)
            {
                rb.AddForce(new Vector3(moveDirection.x * moveSpeed, 0, moveDirection.z * moveSpeed), ForceMode.Acceleration);
            }

            if (isYourDuck)
            {
                cameraTransform.position = transform.position + transform.TransformDirection(cameraOffset);

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

                        if (angle < 50f || !PhotonNetwork.IsMasterClient) //dont care about direction if not host
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
        }
    }
}
