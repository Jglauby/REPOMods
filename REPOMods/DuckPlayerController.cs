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

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();  // Ensure the duck has a Rigidbody
                rb.mass = 5f;
                rb.drag = 1f;
                rb.angularDrag = 0.5f;
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
        }

        void Update()
        {
            // Get movement input
            Vector2 moveInput = Keyboard.current != null
            ? new Vector2(Keyboard.current.aKey.isPressed ? -1 : Keyboard.current.dKey.isPressed ? 1 : 0,
                      Keyboard.current.sKey.isPressed ? -1 : Keyboard.current.wKey.isPressed ? 1 : 0)
            : Vector2.zero;

            moveDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;

            // Rotate the duck to face the movement direction
            if (moveDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
        }

        void FixedUpdate()
        {
            // Apply movement
            rb.velocity = new Vector3(moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z * moveSpeed);
        }
    }

}
