using System.Collections;
using UnityEngine;

namespace StillSky.Characters
{
    /// <summary>
    /// Test target: logs damage, flashes on hit, and refills its health after "dying".
    /// Now a Character, so it uses the same life cycle as enemies.
    /// </summary>
    public class TrainingDummy : Character
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor"); // URP Lit color

        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private Color flashColor = Color.red;
        [SerializeField] private float flashTime = 0.1f;
        [SerializeField] private float resetDelay = 2f;

        private MaterialPropertyBlock block;
        private Color originalColor;
        private Coroutine flashRoutine;

        protected override void Awake()
        {
            base.Awake();
            block = new MaterialPropertyBlock();
            originalColor = bodyRenderer.sharedMaterial.GetColor(BaseColorId);
        }

        protected override void OnDamaged(float amount, GameObject source)
        {
            Debug.Log($"{DisplayName} took {amount} from {source.name} ({Health.Current}/{Health.Max})");

            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(Flash());
        }

        protected override void OnDeath()
        {
            Debug.Log($"{DisplayName} destroyed. Resetting in {resetDelay}s");
            StartCoroutine(ResetAfterDelay());
        }

        private IEnumerator Flash()
        {
            SetColor(flashColor);
            yield return new WaitForSeconds(flashTime);
            SetColor(originalColor);
        }

        private IEnumerator ResetAfterDelay()
        {
            yield return new WaitForSeconds(resetDelay);
            Health.ResetHealth();
        }

        private void SetColor(Color color)
        {
            bodyRenderer.GetPropertyBlock(block);
            block.SetColor(BaseColorId, color);
            bodyRenderer.SetPropertyBlock(block);
        }
    }
}