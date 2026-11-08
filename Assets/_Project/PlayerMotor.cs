using UnityEngine;
using UnityEngine.InputSystem;

namespace StillSky.Player
{
    /// <summary>
    /// Moves the player relative to where it faces (yaw comes from PlayerLook).
    /// Handles walk, sprint, jump and gravity through a CharacterController.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference sprintAction;

        [Header("Movement")]
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float sprintSpeed = 7f;
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -20f;

        private CharacterController controller;
        private float verticalVelocity;

        /// <summary>Horizontal speed this frame, for the Animator later.</summary>
        public float CurrentSpeed { get; private set; }
        public bool IsGrounded => controller.isGrounded;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            moveAction.action.Enable();
            jumpAction.action.Enable();
            sprintAction.action.Enable();
        }

        private void OnDisable()
        {
            moveAction.action.Disable();
            jumpAction.action.Disable();
            sprintAction.action.Disable();
        }

        private void Update()
        {
            bool grounded = controller.isGrounded;

            // Keep a small downward force so isGrounded stays reliable on slopes.
            if (grounded && verticalVelocity < 0f)
                verticalVelocity = -2f;

            Vector2 input = moveAction.action.ReadValue<Vector2>();
            Vector3 move = transform.right * input.x + transform.forward * input.y;
            move = Vector3.ClampMagnitude(move, 1f); // no faster diagonals

            float speed = sprintAction.action.IsPressed() ? sprintSpeed : walkSpeed;

            if (grounded && jumpAction.action.WasPressedThisFrame())
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity); // v = sqrt(2gh)

            verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = move * speed + Vector3.up * verticalVelocity;
            controller.Move(velocity * Time.deltaTime);

            CurrentSpeed = move.magnitude * speed;
        }
    }
}