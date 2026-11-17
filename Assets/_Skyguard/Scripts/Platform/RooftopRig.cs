using System.Collections;
using Skyguard.Core;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace Skyguard.Platform
{
    // there is no artificial locomotion. between towers the screen goes black for a second and you're somewhere else
    public class RooftopRig : MonoBehaviour
    {
        [SerializeField] XROrigin origin;
        [SerializeField] ScreenFader fader;
        [SerializeField] Rooftop[] rooftops;
        [SerializeField] float fadeTime = 0.5f;
        [SerializeField] float blackTime = 1f;

        public int CurrentIndex { get; private set; } = -1;
        public Rooftop Current => rooftops[Mathf.Max(CurrentIndex, 0)];
        public bool IsMoving { get; private set; }
        public int Count => rooftops.Length;

        public Rooftop Get(int index) => rooftops[index];

        public IEnumerator MoveTo(int index)
        {
            IsMoving = true;
            yield return fader.FadeTo(1f, fadeTime);
            PlaceAt(index);
            yield return new WaitForSecondsRealtime(blackTime);
            yield return fader.FadeTo(0f, fadeTime);
            IsMoving = false;
        }

        public void PlaceAt(int index)
        {
            var roof = rooftops[index];
            var anchor = roof.PlayerAnchor.position;

            // face the tower, then slide the rig so the player's head ends up right above the anchor
            var towards = roof.Tower.transform.position - anchor;
            towards.y = 0f;
            origin.MatchOriginUpCameraForward(Vector3.up, towards.normalized);

            var head = origin.Camera.transform.position;
            var feet = new Vector3(head.x, origin.transform.position.y, head.z);
            origin.transform.position += anchor - feet;

            CurrentIndex = index;
            EventBus.Publish(new RooftopChanged { Index = index });
        }
    }
}
