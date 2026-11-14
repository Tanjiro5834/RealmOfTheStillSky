using UnityEngine;

namespace StillSky.Core
{
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
        public void Refill()
        {
            Current = max;
        }
        public bool TryUse(float amount)
        {
            if (Current <= 0f) return false;

            Current = Mathf.Max(0f, Current - amount);
            lastUseTime = Time.time;
            return true;
        }
    }
}