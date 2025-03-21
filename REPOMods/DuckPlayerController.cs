using BepInEx.Logging;
using REPOMods;
using System;
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
        private Transform cameraTransform;
        public float mouseSensitivity = 0.25f;
        private float cameraPitch = 0f;

        public Vector3 cameraOffset = new Vector3(0, 1.5f, -1.5f); 
        public float cameraSmoothSpeed = 10f;

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
        }

        private void handleInput()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                mls.LogInfo("Jumping!");
            }

            if (Keyboard.current.rightCtrlKey.isPressed && Keyboard.current.cKey.wasPressedThisFrame)
            {
                mls.LogInfo("Reseting control of duck");
                GeneralUtil.ControlClosestDuck(cameraTransform.position);
            }

            if (Keyboard.current.ctrlKey.isPressed && Keyboard.current.lKey.wasPressedThisFrame)
            {
                mls.LogInfo("Leave duck form, if dead spectate");
                GeneralUtil.ReleaseDuckControl();
            }

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                mls.LogInfo("toggle attack mode");
                EnemyDuck thisDuck = GeneralUtil.FindClosestDuck(cameraTransform.position);

                if (thisDuck.currentState == EnemyDuck.State.AttackStart)
                {
                    thisDuck.currentState = EnemyDuck.State.Idle;
                }
                else
                {
                    thisDuck.currentState = EnemyDuck.State.AttackStart;
                }
            }
        }
    }

}
