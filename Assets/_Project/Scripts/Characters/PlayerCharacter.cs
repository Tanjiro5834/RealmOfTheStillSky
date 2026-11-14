using StillSky.Player;
using UnityEngine;

namespace StillSky.Characters
{
    /// <summary>
    /// Kael. Coordinates the player components on death and respawn.
    /// Movement, look, combat and dodge stay in their own components.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerCharacter : Character
    {
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private PlayerLook look;
        [SerializeField] private PlayerCombat combat;
        [SerializeField] private PlayerDodge dodge;

        private CharacterController controller;

        protected override void Awake()
        {
            base.Awake(); // always call base: it sets up Health
            controller = GetComponent<CharacterController>();
        }

        protected override void OnDamaged(float amount, GameObject source)
        {
            Debug.Log($"Kael took {amount} from {source.name} ({Health.Current}/{Health.Max})");
        }

        protected override void OnDeath()
        {
            Debug.Log("Kael has fallen.");
            SetControlsEnabled(false);
            // Later: drop Embers here, then respawn at the last campfire.
        }

        public void Respawn(Vector3 position)
        {
            // CharacterController overrides transform.position unless disabled first.
            controller.enabled = false;
            transform.position = position;
            controller.enabled = true;

            Health.ResetHealth();
            SetControlsEnabled(true);
        }

        private void SetControlsEnabled(bool value)
        {
            motor.enabled = value;
            look.enabled = value;
            combat.enabled = value;
            dodge.enabled = value;
        }
    }
}