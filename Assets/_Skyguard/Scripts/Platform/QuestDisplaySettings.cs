using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Skyguard.Platform
{
    // turns on fixed foveated rendering. quest 2 already runs at 72 hz out of the box, which is our target
    public class QuestDisplaySettings : MonoBehaviour
    {
        [Range(0f, 1f)]
        [SerializeField] float foveation = 0.75f;

        readonly List<XRDisplaySubsystem> displays = new();

        void Start()
        {
            SubsystemManager.GetSubsystems(displays);
            // the edges of each lens get rendered at lower resolution, the eye can't tell out there
            foreach (var display in displays)
                display.foveatedRenderingLevel = foveation;
        }
    }
}
