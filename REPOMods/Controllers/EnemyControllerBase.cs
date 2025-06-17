using BepInEx.Logging;
using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Networking;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OpJosModREPO.Controllers.IAmEnemy
{
    public class EnemyControllerBase: MonoBehaviour
    {
        protected static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        public int controlActorNumber;
        public bool isInBlendMode; //used for host, host controllers of other peoples ducks use this

        protected EnemyRigidbody erb;
        protected Rigidbody rb;
        protected bool isYourEnemy = false; //if false means you are host, no client has this controller if it isn't for them
        protected bool isHost = false;
        protected float attackCooldown;

        private GameObject thisEnemyGameObject;
        private Enemy thisEnemyEnemy;
        private Transform thisEnemyTransform;
        private Vector3 moveDirection;
        private float moveSpeed = 3f;
        private float turnSpeed = 3f;
        private float jumpForce = 0.5f;
        private Transform cameraTransform;
        private float mouseSensitivity = 0.25f;
        private float cameraPitch = 0f;
        private Vector3 cameraOffset = new Vector3(0, 1.75f, -1.75f);
        private float cameraSmoothSpeed = 15f;
        private float syncTimer = 0f;
        private float syncInterval = 0.375f;
        private Vector3 targetLookDirection;
        private bool shouldJump = false;
        private bool slowFall = false;
        private GameObject nightLight;

        protected void OnSetup(int actorNumber, GameObject enemyGameObject, Enemy thisEnemy, Transform enemyTransform)
        {
            controlActorNumber = actorNumber;
            isHost = PhotonNetwork.IsMasterClient;
            thisEnemyGameObject = enemyGameObject;
            thisEnemyEnemy = thisEnemy;
            thisEnemyTransform = enemyTransform;

            erb = ReflectionUtils.GetFieldValue<EnemyRigidbody>(thisEnemy, "Rigidbody");
            rb = ReflectionUtils.GetFieldValue<Rigidbody>(erb, "rb");

            if (PhotonNetwork.LocalPlayer.ActorNumber == controlActorNumber) //is your enemy
            {
                isYourEnemy = true;
                PlayerController.instance.enabled = false;
                Camera.main.transform.SetParent(enemyGameObject.transform);

                Camera.main.transform.localPosition = cameraOffset;
                Camera.main.transform.localRotation = Quaternion.identity;

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                AddGlobalLight();
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

        protected void UpdateLogic()
        {
            if (!isYourEnemy)
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

            transform.Rotate(Vector3.up * mouseX); // Rotate enemy
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

        protected void FixedUpdateLogic()
        {
            if (PublicVars.DuckInBlendMode || isInBlendMode)
                return;

            if (isYourEnemy || isHost)
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
                thisEnemyGameObject.transform.position = rb.transform.position;
            }

            if (isYourEnemy)
            {
                cameraTransform.position = new Vector3(cameraTransform.position.x, rb.transform.position.y + cameraOffset.y, cameraTransform.position.z);
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

            try
            {
                if (Keyboard.current[ConfigVariables.resetControlKey].wasPressedThisFrame)
                {
                    mls.LogInfo("Reseting control of enemy");
                    GeneralUtil.ControlClosestDuck(cameraTransform.position, controlActorNumber);
                }
            }
            catch { }

            try
            {
                if (Keyboard.current[ConfigVariables.selfDestructKey].wasPressedThisFrame)
                {
                    EnemyHealth healthComponent = ReflectionUtils.GetFieldValue<EnemyHealth>(thisEnemyEnemy, "Health");
                    ReflectionUtils.InvokeMethod(healthComponent, "Death", new object[] { Vector3.zero });
                    mls.LogMessage("Killed controlled duck");
                }
            }
            catch { }

            try
            {
                if (Keyboard.current[ConfigVariables.toggleBlendModeKey].wasPressedThisFrame)
                {
                    //toggle blend mode
                    if (PublicVars.DuckInBlendMode)
                    {
                        mls.LogInfo("Leaving Blend mode");
                        PublicVars.DuckInBlendMode = false;

                        if (PhotonNetwork.IsMasterClient)
                            GeneralUtil.BreakEnemyAI(thisEnemyEnemy);
                        else
                            DuckSpawnerNetwork.Instance.BreakDuckAI(controlActorNumber);
                    }
                    else
                    {
                        mls.LogInfo("Starting blend mode");
                        PublicVars.DuckInBlendMode = true;

                        if (PhotonNetwork.IsMasterClient)
                            GeneralUtil.EnableEnemyAI(thisEnemyEnemy);
                        else
                            DuckSpawnerNetwork.Instance.EnableDuckAI(controlActorNumber);
                    }
                }
            }
            catch { }
        }

        private void TriggerJump()
        {
            if (thisEnemyEnemy == null) return;

            object enemyJump = ReflectionUtils.GetFieldValue<object>(thisEnemyEnemy, "Jump");
            if (enemyJump == null) return;

            ReflectionUtils.InvokeMethod(enemyJump, "StuckTrigger", new object[] { Vector3.up });
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        private void AddGlobalLight()
        {
            if (nightLight != null) return;

            nightLight = new GameObject("DuckVisionLight");
            var light = nightLight.AddComponent<Light>();

            light.type = LightType.Directional;
            light.intensity = 1.25f;
            light.color = new Color(0.6f, 1f, 0.6f);

            light.transform.rotation = Quaternion.Euler(60f, -40f, 0f);
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.3f;
            light.range = 20f;

            GameObject.DontDestroyOnLoad(nightLight);
            nightLight.transform.SetParent(thisEnemyTransform.transform);
        }
    }
}
