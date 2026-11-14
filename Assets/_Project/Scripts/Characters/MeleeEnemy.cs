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

        protected override void PerformAttack(Character target)
        {
            float multiplier = stats != null ? stats.damageMultiplier : 1f;
            // Goes through Health, so Kael's dodge i-frames are respected automatically.
            target.Health.TakeDamage(baseDamage * multiplier, gameObject);
        }

        protected override void OnDeath()
        {
            base.OnDeath(); // state + Ember reward
            Debug.Log($"{DisplayName} defeated");
            Destroy(gameObject, corpseLifetime);
        }
    }
}