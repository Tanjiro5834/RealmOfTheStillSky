using StillSky.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StillSky.UI
{
    public class PlayerHUD : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] private Health health;
        [SerializeField] private Stamina stamina;
        [SerializeField] private EmberWallet wallet;

        [Header("UI")]
        [SerializeField] private Slider healthBar;   
        [SerializeField] private Slider staminaBar;  
        [SerializeField] private TMP_Text emberText;

        private void OnEnable()
        {
            wallet.Changed += UpdateEmbers;
            UpdateEmbers(wallet.Embers);
        }

        private void OnDisable()
        {
            wallet.Changed -= UpdateEmbers;
        }

        private void Update()
        {
            healthBar.value = health.Current / health.Max;
            staminaBar.value = stamina.Current / stamina.Max;
        }

        private void UpdateEmbers(int embers)
        {
            emberText.text = embers.ToString("N0");
        }
    }
}