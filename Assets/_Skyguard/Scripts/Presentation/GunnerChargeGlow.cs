using Skyguard.Drones;
using UnityEngine;

namespace Skyguard.Presentation
{
    // the gunner's eye brightens while it charges, the visual half of the warning tone
    public class GunnerChargeGlow : MonoBehaviour
    {
        [SerializeField] GunnerDrone gunner;
        [SerializeField] Renderer eye;
        [SerializeField] int eyeMaterialIndex;
        [SerializeField] Color idle = new(1f, 0.8f, 0.1f);
        [SerializeField] Color charged = new(1f, 0.2f, 0.05f);
        [SerializeField] float maxIntensity = 12f;

        static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        MaterialPropertyBlock block;

        void Awake() => block = new MaterialPropertyBlock();

        void LateUpdate()
        {
            float t = gunner.ChargeProgress;
            float flicker = t > 0.7f ? Mathf.PingPong(Time.time * 20f, 1f) : 1f;
            var colour = Color.Lerp(idle, charged, t) * Mathf.Lerp(2f, maxIntensity, t * flicker);
            // only the eye's material slot, the rest of the body keeps its normal look
            eye.GetPropertyBlock(block, eyeMaterialIndex);
            block.SetColor(EmissionColorId, colour);
            eye.SetPropertyBlock(block, eyeMaterialIndex);
        }
    }
}
