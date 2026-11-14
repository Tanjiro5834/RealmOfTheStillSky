using System.Collections;
using StillSky.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StillSky.Player
{
    [RequireComponent(typeof(PlayerMotor))]
    public class PlayerDodge : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionReference dodgeAction;
        [SerializeField] private InputActionReference moveAction;

        [Header("References")]
        [SerializeField] private Stamina stamina;
        [SerializeField] private Health health;
        [SerializeField] private PlayerCombat combat;

        [Header("Tuning")]
        [SerializeField] private float dodgeSpeed = 9f;
        [SerializeField] private float duration = 0.35f;
        [SerializeField] private float iFrameStart = 0.05f;
        [SerializeField] private float iFrameEnd = 0.30f;
        [SerializeField] private float staminaCost = 20f;
        [SerializeField] private float cooldown = 0.15f;

        public bool IsDodging { get; private set; }

        private PlayerMotor motor;
        private float nextDodgeTime;

        private void Awake()
        {
            motor = GetComponent<PlayerMotor>();
        }

        private void OnEnable()
        {
            dodgeAction.action.Enable();
            dodgeAction.action.performed += OnDodge;
        }

        private void OnDisable()
        {
            dodgeAction.action.performed -= OnDodge;
            dodgeAction.action.Disable();

            StopAllCoroutines();
            EndDodge();
        }

        private void OnDodge(InputAction.CallbackContext _)
        {
            if (IsDodging || Time.time < nextDodgeTime || !motor.IsGrounded) return;
            if (combat != null && combat.IsAttacking) return;
            if (stamina != null && !stamina.TryUse(staminaCost)) return;

            StartCoroutine(DodgeRoutine());
        }

        private IEnumerator DodgeRoutine()
        {
            IsDodging = true;

            Vector2 input = moveAction.action.ReadValue<Vector2>();
            Vector3 direction = input.sqrMagnitude > 0.01f
                ? (transform.right * input.x + transform.forward * input.y).normalized
                : -transform.forward;

            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                if (health != null)
                    health.Invulnerable = t >= iFrameStart && t <= iFrameEnd;

                motor.SetOverrideVelocity(direction * dodgeSpeed);
                yield return null;
            }

            EndDodge();
            nextDodgeTime = Time.time + cooldown;
        }

        private void EndDodge()
        {
            IsDodging = false;
            if (motor != null) motor.SetOverrideVelocity(null);
            if (health != null) health.Invulnerable = false;
        }
    }
}