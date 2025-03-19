using UnityEngine;
using UnityEngine.InputSystem;

namespace OpJosModREPO.IAmDucky
{
    public class DuckPlayerController : MonoBehaviour
    {
        private Rigidbody rb;
        private Vector3 moveDirection;
        public float moveSpeed = 5f;
        public float turnSpeed = 3f;
        public float jumpForce = 8f;
        public float gravity = 9.8f;
        private Transform cameraTransform;
        public float mouseSensitivity = 2f;
        private float cameraPitch = 0f;


        void Start()
        {
            rb = GetComponent<Rigidbody>();
            cameraTransform = Camera.main.transform; // Get the main camera

            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();  // Ensure the duck has a Rigidbody
                rb.mass = 5f;
                rb.drag = 1f;
                rb.angularDrag = 0.5f;
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }

            // Attach the camera behind the duck
            cameraTransform.SetParent(transform);
            cameraTransform.localPosition = new Vector3(0, 1.5f, -3f); // Adjust position
            cameraTransform.localRotation = Quaternion.identity;

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
        }

        void FixedUpdate()
        {
            // Apply movement
            rb.AddForce(new Vector3(moveDirection.x * moveSpeed, 0, moveDirection.z * moveSpeed), ForceMode.Acceleration);
        }
    }

}
