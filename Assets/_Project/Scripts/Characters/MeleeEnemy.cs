using UnityEngine;

namespace StillSky.Characters
{
    public class MeleeEnemy : EnemyCharacter
    {
        [Header("Melee")]
        [SerializeField] private float baseDamage = 10f;
        [SerializeField] private float corpseLifetime = 3f;

        protected override void PerformAttack(Character target)
        {
            float multiplier = stats != null ? stats.damageMultiplier : 1f;
            target.Health.TakeDamage(baseDamage * multiplier, gameObject);
        }

        protected override void OnDeath()
        {
            base.OnDeath();
            Debug.Log($"{DisplayName} defeated (+{(stats != null ? stats.emberReward : 0)} Embers)");
            Destroy(gameObject, corpseLifetime);
        }
    }
}