using BepInEx.Logging;
using OpJosModREPO.IAmEnemy;
using OpJosModREPO.IAmEnemy.Networking;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;
using REPOMods;
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
        public bool isInBlendMode; //used for host, host controllers of other peoples enemies use this
        public GameObject thisEnemyGameObject;
        public Enemy thisEnemyEnemy;

        protected EnemyRigidbody erb;
        protected Rigidbody rb;
        protected bool isYourEnemy = false; //if false means you are host, no client has this controller if it isn't for them
        protected bool isHost = false;
        protected float attackCooldown;
        protected Transform cameraTransform;

        private float syncTimer = 0f;
        private float syncInterval = 0.375f;
        private Transform thisEnemyTransform;
        private Vector3 moveDirection;
        private float mouseSensitivity = 0.25f;
        private float cameraPitch = 0f;
        private Vector3 cameraOffset = new Vector3(0, 1.75f, -1.75f);
        private float cameraSmoothSpeed = 15f;
        private Vector3 targetLookDirection;
        private float lastJumpAt = -1f;
        private bool shouldJump = false;
        private bool slowFall = false;
        private GameObject nightLight;
        public float lastAttackTime = -Mathf.Infinity;
        public float timeSinceLastAttack
        {
            get
            {
                return Time.time - lastAttackTime;
            }
        }
        private float lastWalkInput = -1;
        private bool movingAnimationOverrides = false;

        //enemy specs
        private float moveSpeed = 2.7f;
        private float turnSpeed = 3f;
        private float jumpForce = 0.5f;
        public float attackDelay = 2f;
        public bool flyingEnemy = false;

        // --- New ground/step handling fields ---
        [SerializeField] private float groundCheckDistance = 0.7f;
        [SerializeField] private LayerMask groundLayers = ~0; // default everything
        [SerializeField] private float maxSlopeAngle = 50f; // angle considered still "walkable"
        [SerializeField] private float groundDrag = 6f;
        [SerializeField] private float airDrag = 0f;
        [SerializeField] private float stepHeight = 0.4f; // max height to step up (stairs)
        [SerializeField] private float stepCheckForward = 0.4f; // how far ahead to check for a step
        [SerializeField] private float stepSmooth = 7f; // smoothing when stepping up
        private Vector3 groundNormal = Vector3.up;
        private bool grounded = false;
        private float originalDrag = 0f;
        // -----------------------------------------

        protected void OnSetup(int actorNumber, GameObject enemyGameObject, Enemy thisEnemy, Transform enemyTransform, EnemySpecs specs)
        {
            controlActorNumber = actorNumber;
            isHost = PhotonNetwork.IsMasterClient;
            thisEnemyGameObject = enemyGameObject;
            thisEnemyEnemy = thisEnemy;
            thisEnemyTransform = enemyTransform;
            movingAnimationOverrides = specs.MovingAnimationOverrides;

            moveSpeed = specs.MoveSpeed;
            turnSpeed = specs.TurnSpeed;
            jumpForce = specs.JumpForce;
            attackDelay = specs.AttackDelay;
            flyingEnemy = specs.FlyingEnemy;

            erb = ReflectionUtils.GetFieldValue<EnemyRigidbody>(thisEnemy, "Rigidbody");
            rb = ReflectionUtils.GetFieldValue<Rigidbody>(erb, "rb");

            if (rb != null)
            {
                originalDrag = rb.drag;
            }

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

        // Reattach the main camera to this enemy (used when switching view back to enemy)
        public void ResetCameraToEnemy()
        {
            if (thisEnemyGameObject == null)
            {
                mls.LogWarning("Cannot reset camera to enemy: thisEnemyGameObject is null.");
                return;
            }

            Camera cam = Camera.main ?? GameObject.FindObjectOfType<Camera>();
            if (cam == null)
            {
                mls.LogWarning("No camera found to reset to enemy.");
                return;
            }

            // Disable player controller and attach camera to enemy
            if (PlayerController.instance != null)
                PlayerController.instance.enabled = false;

            cam.tag = "MainCamera";
            cam.enabled = true;
            cam.gameObject.SetActive(true);

            cam.transform.SetParent(thisEnemyGameObject.transform);
            cam.transform.localPosition = cameraOffset;
            cam.transform.localRotation = Quaternion.identity;
            cam.transform.localScale = Vector3.one;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            AddGlobalLight();

            cameraTransform = cam.transform;
            mls.LogInfo("Camera reattached to enemy.");
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

                    EnemySpawnerNetwork.Instance.SendEnemyMovement(moveDirection, camForward, controlActorNumber, shouldJump);
                    shouldJump = false;
                    syncTimer = 0f;
                }
            }

            handleInput();
            handleAnimations();
        }

        protected void FixedUpdateLogic()
        {
            if (PublicVars.EnemyInBlendMode || isInBlendMode)
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
                // Ensure the underlying enemy rigidbody doesn't snap back to AI position
                erb.DisableFollowPosition(0.5f, 50f);

                // Ground & step handling
                CheckGround();

                // Prevent sliding down slopes by projecting movement to ground plane and applying smoother acceleration
                if (rb != null)
                {
                    // Adjust drag so friction-like when grounded
                    rb.drag = grounded ? groundDrag : airDrag;

                    // Project move onto ground plane so we move along slope instead of into it
                    Vector3 desiredDir = Vector3.zero;
                    if (moveDirection != Vector3.zero)
                        desiredDir = Vector3.ProjectOnPlane(moveDirection, groundNormal).normalized;

                    // Try to step up small obstacles
                    if (desiredDir != Vector3.zero)
                        TryStepClimb(desiredDir);

                    Vector3 desiredVelocity = desiredDir * moveSpeed;
                    Vector3 currentVelocity = rb.velocity;
                    Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);

                    // Smooth acceleration toward desired horizontal velocity
                    Vector3 velocityChange = desiredVelocity - horizontalVelocity;
                    // tuning factor (higher = snappier)
                    float accelFactor = 10f;
                    Vector3 accel = velocityChange * accelFactor;

                    rb.AddForce(accel, ForceMode.Acceleration);

                    // If no input, apply damping similar to before
                    if (desiredDir == Vector3.zero)
                    {
                        Vector3 horizontalVelNow = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                        horizontalVelNow = Vector3.Lerp(horizontalVelNow, Vector3.zero, Time.fixedDeltaTime * 5.5f);
                        rb.velocity = new Vector3(horizontalVelNow.x, rb.velocity.y, horizontalVelNow.z);
                    }
                }

                //sticks object camera is on to rigidbody thats moving
                if (rb != null && thisEnemyGameObject != null)
                    thisEnemyGameObject.transform.position = rb.transform.position;
            }

            if (isYourEnemy)
            {
                cameraTransform.position = new Vector3(cameraTransform.position.x, rb.transform.position.y + cameraOffset.y, cameraTransform.position.z);
            }
        }

        private void handleInput()
        {
            if (controlActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)//dont listen to keys if not your enemy
                return;

            if (Keyboard.current.eKey.wasPressedThisFrame && PhotonNetwork.IsMasterClient)
            {
                ResetCameraToEnemy();
            }

            if (Keyboard.current.spaceKey.wasPressedThisFrame && PhotonNetwork.IsMasterClient)
            {
                TriggerJump();
            }

            try
            {
                if (Keyboard.current[ConfigVariables.selfDestructKey].wasPressedThisFrame)
                {
                    EnemyHealth healthComponent = ReflectionUtils.GetFieldValue<EnemyHealth>(thisEnemyEnemy, "Health");
                    ReflectionUtils.InvokeMethod(healthComponent, "Death", new object[] { Vector3.zero });
                    mls.LogMessage("Killed controlled enemy");
                }
            }
            catch { }

            try
            {
                if (Keyboard.current[ConfigVariables.toggleBlendModeKey].wasPressedThisFrame)
                {
                    //toggle blend mode
                    if (PublicVars.EnemyInBlendMode)
                    {
                        mls.LogInfo("Leaving Blend mode");
                        PublicVars.EnemyInBlendMode = false;

                        if (PhotonNetwork.IsMasterClient)
                            GeneralUtil.BreakEnemyAI(thisEnemyEnemy);
                        else
                            EnemySpawnerNetwork.Instance.BreakEnemyAI(controlActorNumber);
                    }
                    else
                    {
                        mls.LogInfo("Starting blend mode");
                        PublicVars.EnemyInBlendMode = true;

                        if (PhotonNetwork.IsMasterClient)
                            GeneralUtil.EnableEnemyAI(thisEnemyEnemy);
                        else
                            EnemySpawnerNetwork.Instance.EnableEnemyAI(controlActorNumber);
                    }
                }
            }
            catch { }

            if (flyingEnemy)
            {
                try
                {
                    if (Keyboard.current[Key.Space].wasPressedThisFrame)
                    {
                        if (isHost)
                        {
                            FlightMovement(1);
                        }
                        else
                        {
                            EnemySpawnerNetwork.Instance.TriggerFlightMovement(1, controlActorNumber);
                        }
                    }
                }
                catch { }

                try
                {
                    if (Keyboard.current[Key.LeftCtrl].wasPressedThisFrame)
                    {
                        if (isHost)
                        {
                            FlightMovement(0);
                        }
                        else
                        {
                            EnemySpawnerNetwork.Instance.TriggerFlightMovement(0, controlActorNumber);
                        }
                    }
                }
                catch { }
            }
        }

        private void handleAnimations()
        {
            if (!movingAnimationOverrides) return;

            try
            {
                if (Keyboard.current[Key.W].isPressed || Keyboard.current[Key.A].isPressed || Keyboard.current[Key.S].isPressed || Keyboard.current[Key.D].isPressed)
                {
                    int currSecond = Mathf.FloorToInt(Time.time);
                    if (currSecond % 2 == 0 && currSecond != lastWalkInput)
                    {
                        lastWalkInput = currSecond;
                        SetMoveAnimation();
                    }
                }
                else
                {
                    int currSecond = Mathf.FloorToInt(Time.time);
                    if (currSecond % 2 == 0 && currSecond != lastWalkInput)
                    {
                        SetStationaryAnimation();
                    }
                }
            }
            catch { }
        }

        public virtual void SetMoveAnimation()
        {
            mls.LogWarning("SetMoveAnimation was hit in base controller, it should be override");
        }

        public virtual void SetStationaryAnimation()
        {
            mls.LogWarning("SetStationaryAnimation was hit in base controller, it should be override");
        }

        public virtual void SpecialAttack(Vector3 pos, Vector3 rot)
        {
            mls.LogWarning("Special attack was hit in base controller, it should be override");
        }

        public virtual void SpecialMovement(int num)
        {
            mls.LogWarning("Special movement was hit in base controller, it should be override");
        }

        public void FlightMovement(int num)
        {
            //1 -> up, 0 -> down
            if (num == 1)
            {
                EnemyRigidbody erb = ReflectionUtils.GetFieldValue<EnemyRigidbody>(thisEnemyEnemy, "Rigidbody");
                Rigidbody rb = ReflectionUtils.GetFieldValue<Rigidbody>(erb, "rb");
                rb.AddForce(Vector3.up * 1f, ForceMode.Impulse);
            }
            else if (num == 0)
            {
                EnemyRigidbody erb = ReflectionUtils.GetFieldValue<EnemyRigidbody>(thisEnemyEnemy, "Rigidbody");
                Rigidbody rb = ReflectionUtils.GetFieldValue<Rigidbody>(erb, "rb");
                rb.AddForce(Vector3.down * 1f, ForceMode.Impulse);
            }
        }

        private void TriggerJump()
        {
            if (thisEnemyEnemy == null || (Time.time - lastJumpAt < 0.8f) || flyingEnemy) return;

            object enemyJump = ReflectionUtils.GetFieldValue<object>(thisEnemyEnemy, "Jump");
            if (enemyJump != null)
            {
                ReflectionUtils.InvokeMethod(enemyJump, "StuckTrigger", new object[] { Vector3.up });
            }
            else
            {
                lastJumpAt = Time.time;
                rb.useGravity = false;
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                DelayUtility.RunAfterDelay(0.55f, () =>
                {
                    rb.useGravity = true;
                });
            }
        }

        // --- New helper methods ---

        private void CheckGround()
        {
            if (rb == null) return;

            Vector3 origin = rb.position + Vector3.up * 0.1f;
            RaycastHit hit;
            grounded = false;
            groundNormal = Vector3.up;

            if (Physics.Raycast(origin, Vector3.down, out hit, groundCheckDistance, groundLayers, QueryTriggerInteraction.Ignore))
            {
                groundNormal = hit.normal;
                float angle = Vector3.Angle(hit.normal, Vector3.up);
                grounded = angle <= maxSlopeAngle;
            }
            else
            {
                grounded = false;
                groundNormal = Vector3.up;
            }
        }

        private void TryStepClimb(Vector3 desiredDir)
        {
            if (rb == null || desiredDir == Vector3.zero) return;

            // origin near feet
            Vector3 feet = rb.position + Vector3.up * 0.05f;
            Vector3 forward = transform.TransformDirection(desiredDir).normalized;

            // cast forward at feet level to detect obstacle within stepCheckForward
            RaycastHit hitForward;
            if (Physics.Raycast(feet, forward, out hitForward, stepCheckForward, groundLayers, QueryTriggerInteraction.Ignore))
            {
                // if obstacle is low enough to step
                // cast at stepHeight above feet to ensure space to step up
                Vector3 stepUpOrigin = rb.position + Vector3.up * (stepHeight + 0.05f);
                if (!Physics.Raycast(stepUpOrigin, forward, out _, stepCheckForward, groundLayers, QueryTriggerInteraction.Ignore))
                {
                    // perform a smooth upward MovePosition to "step" up
                    Vector3 targetPos = rb.position + Vector3.up * stepHeight;
                    Vector3 newPos = Vector3.Lerp(rb.position, targetPos, Time.fixedDeltaTime * stepSmooth);
                    rb.MovePosition(newPos);
                }
            }
        }

        private void AddGlobalLight()
        {
            if (nightLight != null) return;

            nightLight = new GameObject("EnemyVisionLight");
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
