using UnityEngine;
using UnityEngine.UI;

namespace Skyguard.UI
{
    // slow drift across the key art behind the main menu title, so the menu doesn't sit dead still
    public class MenuBackdrop : MonoBehaviour
    {
        [SerializeField] RawImage art;
        [SerializeField] float zoom = 0.88f;
        [SerializeField] float period = 24f;

        void Update()
        {
            // unscaled, the menu also shows while the game is paused
            float t = Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f / period) * 0.5f + 0.5f;
            float slack = 1f - zoom;
            art.uvRect = new Rect(slack * t, slack * 0.5f, zoom, zoom);
        }
    }
}
