using TMPro;
using UnityEngine;

namespace Skyguard.UI
{
    public class SubtitlePanel : MonoBehaviour
    {
        [SerializeField] TMP_Text text;
        [SerializeField] CanvasGroup group;
        [SerializeField] float fadeSpeed = 4f;

        float visibleUntil;

        void Awake() => group.alpha = 0f;

        public void Show(string line, float duration)
        {
            text.text = line;
            visibleUntil = Time.unscaledTime + duration;
        }

        void Update()
        {
            float target = Time.unscaledTime < visibleUntil ? 1f : 0f;
            group.alpha = Mathf.MoveTowards(group.alpha, target, Time.unscaledDeltaTime * fadeSpeed);
        }
    }
}
