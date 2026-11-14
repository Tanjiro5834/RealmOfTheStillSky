using System;
using UnityEngine;

namespace StillSky.Core
{
    public class EmberWallet : MonoBehaviour
    {
        public int Embers { get; private set; }

        public event Action<int> Changed; 

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

        public int TakeAll()
        {
            int amount = Embers;
            Embers = 0;
            Changed?.Invoke(Embers);
            return amount;
        }
    }
}