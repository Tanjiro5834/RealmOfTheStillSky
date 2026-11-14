using StillSky.Core;
using UnityEngine;

namespace StillSky.Player
{
    /// <summary>
    /// The Embers Kael leaves where he died. Walk into it to recover them.
    /// Uses a distance check instead of trigger physics: simple and predictable.
    /// </summary>
    public class EmberDrop : MonoBehaviour
    {
        [SerializeField] private float pickupRadius = 1.5f;
        [SerializeField] private float bobHeight = 0.15f;
        [SerializeField] private float bobSpeed = 2f;

        private int amount;
        private EmberWallet wallet;
        private Vector3 basePosition;

        private bool armed;

        public void Init(int embers, EmberWallet owner)
        {
            amount = embers;
            wallet = owner;
            basePosition = transform.position;
        }

        private void Update()
        {
            transform.position = basePosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);

            if (wallet == null) return;

            float dist = Vector3.Distance(wallet.transform.position, basePosition);
            if (!armed)
            {
                armed = dist > pickupRadius;
                return;
            }

            if(dist <= pickupRadius)
            {
                wallet.Add(amount);
                Debug.Log($"Recovered {amount} Embers");
                Destroy(gameObject);
            }
        }
    }
}