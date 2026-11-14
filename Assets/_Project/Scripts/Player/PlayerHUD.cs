using StillSky.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StillSky.UI
{
    /// <summary>Health bar, stamina bar and Ember counter.</summary>
    public class PlayerHUD : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] private Health health;
        [SerializeField] private Stamina stamina;
        [SerializeField] private EmberWallet wallet;

        [Header("UI")]
        [SerializeField] private Slider healthBar;   // Min 0, Max 1
        [SerializeField] private Slider staminaBar;  // Min 0, Max 1
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

        // Bars poll every frame because stamina changes continuously.
        private void Update()
        {
            healthBar.value = health.Current / health.Max;
            staminaBar.value = stamina.Current / stamina.Max;
        }

        // Embers only change on events, so no polling needed.
        private void UpdateEmbers(int embers)
        {
            emberText.text = embers.ToString("N0");
        }
    }
}