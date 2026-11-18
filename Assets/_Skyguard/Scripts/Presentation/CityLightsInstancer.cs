using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skyguard.Presentation
{
    // hundreds of little houses on the hills, drawn with gpu instancing instead of one gameobject each
    public class CityLightsInstancer : MonoBehaviour
    {
        [SerializeField] Mesh mesh;
        [SerializeField] Material material;
        [SerializeField] List<Matrix4x4> placements = new();
        [SerializeField] Bounds bounds = new(Vector3.zero, Vector3.one * 2000f);

        const int BatchSize = 1023;

        readonly List<Matrix4x4[]> batches = new();
        RenderParams renderParams;

        public void SetPlacements(List<Matrix4x4> matrices) => placements = matrices;

        void Start()
        {
            for (int i = 0; i < placements.Count; i += BatchSize)
                batches.Add(placements.GetRange(i, Mathf.Min(BatchSize, placements.Count - i)).ToArray());

            renderParams = new RenderParams(material)
            {
                worldBounds = bounds,
                shadowCastingMode = ShadowCastingMode.Off,
                receiveShadows = false
            };
        }

        void Update()
        {
            foreach (var batch in batches)
                Graphics.RenderMeshInstanced(renderParams, mesh, 0, batch);
        }
    }
}
