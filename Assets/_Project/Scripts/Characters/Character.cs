using System;
using System.Collections.Generic;
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
        // Registry of every active character: cheap lookups without scanning the scene.
        private static readonly List<Character> all = new();
        public static IReadOnlyList<Character> All => all;

        [SerializeField] protected CharacterStats stats;

        public Health Health { get; private set; }
        public CharacterStats Stats => stats;
        public Faction Faction => stats != null ? stats.faction : Faction.Neutral;
        public string DisplayName => stats != null ? stats.displayName : name;
        public bool IsAlive => !Health.IsDead;

        /// <summary>Who hit this character last (used for Ember rewards).</summary>
        protected GameObject LastDamageSource { get; private set; }

        /// <summary>Raised after OnDeath; quests, spawners, respawn and UI listen to this.</summary>
        public event Action<Character> Died;

        protected virtual void Awake()
        {
            Health = GetComponent<Health>();
            if (stats != null) Health.Initialize(stats.maxHealth);
        }

        protected virtual void OnEnable()
        {
            all.Add(this);
            Health.Damaged += HandleDamaged;
            Health.Died += HandleDied;
        }

        protected virtual void OnDisable()
        {
            all.Remove(this);
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
        private void HandleDamaged(float amount, GameObject source)
        {
            LastDamageSource = source;
            OnDamaged(amount, source);
        }

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