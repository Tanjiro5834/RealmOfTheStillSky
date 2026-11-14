using StillSky.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StillSky.Player
{
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

        [Header("Stamina (optional)")]
        [SerializeField] private Stamina stamina;
        [SerializeField] private float sprintCostPerSecond = 15f;

        private CharacterController controller;
        private float verticalVelocity;
        private Vector3? overrideVelocity;

        public float CurrentSpeed { get; private set; }
        public bool IsGrounded => controller.isGrounded;
        public bool IsSprinting { get; private set; }

        public bool InputLocked { get; set; }
        public void SetOverrideVelocity(Vector3? velocity) => overrideVelocity = velocity;

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
            if (grounded && verticalVelocity < 0f)
                verticalVelocity = -2f;

            Vector3 horizontal;

            if (overrideVelocity.HasValue)
            {
                horizontal = overrideVelocity.Value;
                IsSprinting = false;
            }
            else
            {
                Vector2 input = InputLocked ? Vector2.zero : moveAction.action.ReadValue<Vector2>();
                Vector3 move = transform.right * input.x + transform.forward * input.y;
                move = Vector3.ClampMagnitude(move, 1f); // no faster diagonals

                bool wantsSprint = sprintAction.action.IsPressed() && move.sqrMagnitude > 0.01f;
                IsSprinting = wantsSprint && (stamina == null || stamina.TryUse(sprintCostPerSecond * Time.deltaTime));

                horizontal = move * (IsSprinting ? sprintSpeed : walkSpeed);

                if (!InputLocked && grounded && jumpAction.action.WasPressedThisFrame())
                    verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            verticalVelocity += gravity * Time.deltaTime;

            controller.Move((horizontal + Vector3.up * verticalVelocity) * Time.deltaTime);

            CurrentSpeed = horizontal.magnitude;
        }
    }
}