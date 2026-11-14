using System;
using UnityEngine;

namespace StillSky.Core
{
    /// <summary>Kael's Embers: earned from enemies, spent at campfires and shops, dropped on death.</summary>
    public class EmberWallet : MonoBehaviour
    {
        public int Embers { get; private set; }

        public event Action<int> Changed; // new total

        public void Add(int amount)
        {
            if (amount <= 0) return;
            Embers += amount;
            Changed?.Invoke(Embers);
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0 || Embers < amount) return false;
            Embers -= amount;
            Changed?.Invoke(Embers);
            return true;
        }

        /// <summary>Empties the wallet and returns what was in it (used on death).</summary>
        public int TakeAll()
        {
            int amount = Embers;
            Embers = 0;
            Changed?.Invoke(Embers);
            return amount;
        }
    }
}