using StillSky.Core;
using UnityEngine;

namespace StillSky.Characters
{
    /// <summary>
    /// Sword-and-shield enemy. Ember Scout, Hollow Knight, Royal Guard
    /// all use this class with different CharacterStats assets.
    /// </summary>
    public class MeleeEnemy : EnemyCharacter
    {
        [Header("Melee")]
        [SerializeField] private float baseDamage = 10f;
        [SerializeField] private float corpseLifetime = 3f;

        [Header("Feedback")]
        [SerializeField] private BodyTint tint;
        [SerializeField] private Color windupColor = new(1f, 0.35f, 0.05f);
        [SerializeField] private Color hitFlashColor = Color.white;
        [SerializeField] private float hitFlashTime = 0.08f;
        [SerializeField] private Color corpseColor = new(0.15f, 0.15f, 0.15f);

        protected override void Awake()
        {
            base.Awake();
            if (tint == null) tint = GetComponent<BodyTint>();
        }

        protected override void PerformAttack(Character target)
        {
            float multiplier = stats != null ? stats.damageMultiplier : 1f;
            // Goes through Health, so Kael's dodge i-frames are respected automatically.
            target.Health.TakeDamage(baseDamage * multiplier, gameObject);
        }

        protected override void OnWindupStart()
        {
            if (tint != null) tint.SetTint(windupColor);
        }

        protected override void OnWindupEnd()
        {
            if (tint != null) tint.ClearTint();
        }

        protected override void OnDamaged(float amount, GameObject source)
        {
            if (tint != null) tint.Flash(hitFlashColor, hitFlashTime);
        }

        protected override void OnDeath()
        {
            base.OnDeath(); // cancels any swing, stops the agent, grants Embers
            if (tint != null) tint.SetTint(corpseColor);
            Debug.Log($"{DisplayName} defeated");
            Destroy(gameObject, corpseLifetime);
        }
    }
}