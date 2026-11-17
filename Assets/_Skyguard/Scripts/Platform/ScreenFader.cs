using System.Collections;
using UnityEngine;

namespace Skyguard.Platform
{
    // a black shell around the camera. fading this is gentler on first-timers than any kind of movement
    public class ScreenFader : MonoBehaviour
    {
        [SerializeField] Renderer shell;

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        MaterialPropertyBlock block;
        float alpha;

        void Awake()
        {
            block = new MaterialPropertyBlock();
            Apply(0f);
        }

        public IEnumerator FadeTo(float target, float duration)
        {
            float start = alpha;
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                Apply(Mathf.Lerp(start, target, t / duration));
                yield return null;
            }
            Apply(target);
        }

        void Apply(float value)
        {
            alpha = value;
            shell.enabled = alpha > 0.001f;
            block.SetColor(BaseColorId, new Color(0f, 0f, 0f, alpha));
            shell.SetPropertyBlock(block);
        }
    }
}
