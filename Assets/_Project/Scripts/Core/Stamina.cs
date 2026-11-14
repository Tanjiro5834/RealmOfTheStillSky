using UnityEngine;

namespace StillSky.Core
{
    /// <summary>
    /// Souls-style stamina: an action is allowed while stamina is above 0,
    /// even if it costs more than what's left (it just drops to 0).
    /// Regen waits a short delay after the last use.
    /// </summary>
    public class Stamina : MonoBehaviour
    {
        [SerializeField] private float max = 100f;
        [SerializeField] private float regenPerSecond = 30f;
        [SerializeField] private float regenDelay = 0.8f;

        public float Current { get; private set; }
        public float Max => max;

        private float lastUseTime = float.NegativeInfinity;

        private void Awake()
        {
            Current = max;
        }

        private void Update()
        {
            if (Current < max && Time.time - lastUseTime >= regenDelay)
                Current = Mathf.Min(max, Current + regenPerSecond * Time.deltaTime);
        }

        /// <summary>Returns false (and spends nothing) when stamina is empty.</summary>
        public bool TryUse(float amount)
        {
            if (Current <= 0f) return false;

            Current = Mathf.Max(0f, Current - amount);
            lastUseTime = Time.time;
            return true;
        }
    }
}