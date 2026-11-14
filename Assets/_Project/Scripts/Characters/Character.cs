using System;
using StillSky.Core;
using UnityEngine;

namespace StillSky.Characters
{
    /// <summary>
    /// Base class for every living thing: Kael, enemies, bosses, dummies.
    /// Owns identity (stats, faction) and the life cycle; abilities stay in components.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public abstract class Character : MonoBehaviour
    {
        [SerializeField] protected CharacterStats stats;

        public Health Health { get; private set; }
        public CharacterStats Stats => stats;
        public Faction Faction => stats != null ? stats.faction : Faction.Neutral;
        public string DisplayName => stats != null ? stats.displayName : name;
        public bool IsAlive => !Health.IsDead;

        /// <summary>Raised after OnDeath; quests, spawners and UI listen to this.</summary>
        public event Action<Character> Died;

        protected virtual void Awake()
        {
            Health = GetComponent<Health>();
            if (stats != null) Health.Initialize(stats.maxHealth);
        }

        protected virtual void OnEnable()
        {
            Health.Damaged += HandleDamaged;
            Health.Died += HandleDied;
        }

        protected virtual void OnDisable()
        {
            Health.Damaged -= HandleDamaged;
            Health.Died -= HandleDied;
        }

        public bool IsHostileTo(Character other)
        {
            if (other == null || other == this) return false;
            if (Faction == Faction.Neutral || other.Faction == Faction.Neutral) return false;
            return other.Faction != Faction;
        }

        // Template method: the base wires events, subclasses fill in the reaction.
        private void HandleDamaged(float amount, GameObject source) => OnDamaged(amount, source);

        private void HandleDied()
        {
            OnDeath();
            Died?.Invoke(this);
        }

        protected virtual void OnDamaged(float amount, GameObject source) { }

        /// <summary>Every character MUST decide what dying means.</summary>
        protected abstract void OnDeath();
    }
}