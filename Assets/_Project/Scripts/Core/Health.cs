using System;
using UnityEngine;

namespace StillSky.Core
{
    /// <summary>Shared health for the player and every enemy.</summary>
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 100f;

        public float Current { get; private set; }
        public float Max => maxHealth;
        public bool IsDead => Current <= 0f;

        /// <summary>Set by PlayerDodge during i-frames.</summary>
        public bool Invulnerable { get; set; }

        public event Action<float, GameObject> Damaged; // amount, source
        public event Action Died;

        private void Awake()
        {
            Current = maxHealth;
        }

        public void TakeDamage(float amount, GameObject source)
        {
            if (IsDead || Invulnerable || amount <= 0f) return;

            Current = Mathf.Max(0f, Current - amount);
            Damaged?.Invoke(amount, source);

            if (IsDead) Died?.Invoke();
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f) return;
            Current = Mathf.Min(maxHealth, Current + amount);
        }

        public void ResetHealth()
        {
            Current = maxHealth;
        }

        public void Initialize(float max)
        {
            maxHealth = max;
            Current = max;
        }
    }
}