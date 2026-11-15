using System.Collections;
using UnityEngine;

namespace StillSky.Core
{
    public class BodyTint : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private Renderer target;

        private MaterialPropertyBlock block;
        private Color original;
        private Color current;
        private Coroutine flashRoutine;

        private void Awake()
        {
            if (target == null) target = GetComponentInChildren<Renderer>();
            block = new MaterialPropertyBlock();
            original = target.sharedMaterial.GetColor(BaseColorId);
            current = original;
        }

        /// <summary>Persistent color until cleared or replaced.</summary>
        public void SetTint(Color color)
        {
            current = color;
            if (flashRoutine == null) Apply(color); // a running flash restores to this when it ends
        }

        public void ClearTint() => SetTint(original);

        /// <summary>Brief color pop, then back to the current tint.</summary>
        public void Flash(Color color, float duration)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine(color, duration));
        }

        private IEnumerator FlashRoutine(Color color, float duration)
        {
            Apply(color);
            yield return new WaitForSeconds(duration);
            flashRoutine = null;
            Apply(current);
        }

        private void Apply(Color color)
        {
            target.GetPropertyBlock(block);
            block.SetColor(BaseColorId, color);
            target.SetPropertyBlock(block);
        }
    }
}