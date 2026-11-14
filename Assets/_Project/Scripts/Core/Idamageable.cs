using UnityEngine;

namespace StillSky.Core
{
    public interface IDamageable
    {
        void TakeDamage(float amount, GameObject source);
    }
}