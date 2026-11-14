using UnityEngine;

namespace StillSky.Core
{
    /// <summary>Anything that can be hit: player, enemies, dummies, breakables.</summary>
    public interface IDamageable
    {
        void TakeDamage(float amount, GameObject source);
    }
}