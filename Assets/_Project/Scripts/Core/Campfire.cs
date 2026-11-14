using StillSky.Characters;
using StillSky.Core;
using StillSky.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StillSky.World
{
    public class Campfire : MonoBehaviour
    {
        [SerializeField] private string campfireName = "Duskhollow";
        [SerializeField] private InputActionReference interactAction;
        [SerializeField] private float interactRange = 2.5f;
        [SerializeField] private Transform respawnPoint;  
        [SerializeField] private GameObject promptUI;     
        [SerializeField] private Light fireLight;         

        private PlayerCharacter player;
        private bool playerInRange;

        private void OnEnable()
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteract;
        }

        private void OnDisable()
        {
            interactAction.action.performed -= OnInteract;
        }

        private void Start()
        {
            foreach (Character c in Character.All)
            {
                if (c is PlayerCharacter p)
                {
                    player = p;
                    break;
                }
            }

            if (promptUI != null) promptUI.SetActive(false);
        }

        private void Update()
        {
            if (player == null) return;

            bool inRange = player.IsAlive &&
                           Vector3.Distance(player.transform.position, transform.position) <= interactRange;

            if (inRange == playerInRange) return;

            playerInRange = inRange;
            if (promptUI != null) promptUI.SetActive(playerInRange);
        }

        private void OnInteract(InputAction.CallbackContext _)
        {
            if (playerInRange) Rest();
        }

        private void Rest()
        {
            player.Health.ResetHealth();

            if (player.TryGetComponent(out Stamina stamina)) stamina.Refill();

            if (player.TryGetComponent(out PlayerRespawn respawn))
            {
                Vector3 point = respawnPoint != null
                    ? respawnPoint.position
                    : transform.position + transform.forward * 1.5f;
                respawn.SetCheckpoint(point);
            }

            if (fireLight != null) fireLight.enabled = true;

            Debug.Log($"Rested at {campfireName}. Checkpoint set.");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(transform.position, interactRange);
        }
    }
}