using System.Collections;
using StillSky.Characters;
using StillSky.Core;
using UnityEngine;

namespace StillSky.Player
{
    /// <summary>
    /// Souls-style death: Kael drops all Embers where he fell and respawns at the last campfire.
    /// Dying again before recovering them destroys the old drop.
    /// </summary>
    [RequireComponent(typeof(PlayerCharacter))]
    public class PlayerRespawn : MonoBehaviour
    {
        [SerializeField] private EmberWallet wallet;
        [SerializeField] private Stamina stamina;
        [SerializeField] private EmberDrop emberDropPrefab;
        [SerializeField] private float respawnDelay = 2.5f;

        private PlayerCharacter player;
        private Vector3 checkpoint;
        private EmberDrop activeDrop;

        public Vector3 Checkpoint => checkpoint;

        private void Awake()
        {
            player = GetComponent<PlayerCharacter>();
            checkpoint = transform.position; // until the first campfire rest
        }

        private void OnEnable()
        {
            player.Died += OnPlayerDied;
        }

        private void OnDisable()
        {
            player.Died -= OnPlayerDied;
        }

        public void SetCheckpoint(Vector3 position)
        {
            checkpoint = position;
        }

        private void OnPlayerDied(Character _)
        {
            StartCoroutine(RespawnRoutine(transform.position));
        }

        private IEnumerator RespawnRoutine(Vector3 deathPosition)
        {
            int lost = wallet != null ? wallet.TakeAll() : 0;
            if(lost == 0) Debug.Log("Died with 0 embers - no drop.");

            yield return new WaitForSeconds(respawnDelay);

            // Died before recovering the last drop: those Embers are gone for good.
            if (activeDrop != null) Destroy(activeDrop.gameObject);

            // Spawn the drop right before teleporting, so Kael isn't standing on it and picks it up instantly.
            if (lost > 0 && emberDropPrefab != null)
            {
                activeDrop = Instantiate(emberDropPrefab, deathPosition + Vector3.up * 0.5f, Quaternion.identity);
                activeDrop.Init(lost, wallet);
            }

            player.Respawn(checkpoint);
            if (stamina != null) stamina.Refill();
        }
    }
}