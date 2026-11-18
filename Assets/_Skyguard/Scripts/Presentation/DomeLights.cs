using UnityEngine;

namespace Skyguard.Presentation
{
    // the dome's panels sit on a rainbow strip in the atlas. scrolling the uvs walks the colours around it
    public class DomeLights : MonoBehaviour
    {
        [SerializeField] Renderer[] domeRenderers;
        [SerializeField] float cyclesPerMinute = 3f;

        static readonly int BaseMapId = Shader.PropertyToID("_BaseMap_ST");
        static readonly int EmissionMapId = Shader.PropertyToID("_EmissionMap_ST");

        Material material;

        void Start()
        {
            // one shared copy for both LODs, so the city atlas material itself is never touched
            material = new Material(domeRenderers[0].sharedMaterial);
            foreach (var r in domeRenderers)
                r.sharedMaterial = material;
        }

        void Update()
        {
            // the strip holds two hue cycles across the texture, so wrapping at 0.5 is seamless
            float offset = Time.time * cyclesPerMinute / 60f * 0.5f % 0.5f;
            var st = new Vector4(1f, 1f, offset, 0f);
            material.SetVector(BaseMapId, st);
            material.SetVector(EmissionMapId, st);
        }

        void OnDestroy() => Destroy(material);
    }
}
