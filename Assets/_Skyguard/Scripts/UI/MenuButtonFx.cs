using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Skyguard.UI
{
    // when the ray lands on a button its glow sweeps in and the label slides over a little
    public class MenuButtonFx : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] Graphic glow;
        [SerializeField] RectTransform label;
        [SerializeField] float glowAlpha = 0.4f;
        [SerializeField] float slide = 10f;
        [SerializeField] float speed = 12f;

        Selectable button;
        Vector2 labelHome;
        bool hovered;
        float amount;

        void Awake()
        {
            button = GetComponent<Selectable>();
            labelHome = label.anchoredPosition;
            SetAmount(0f);
        }

        void OnDisable()
        {
            // a panel can close under the ray, it shouldn't come back lit up
            hovered = false;
            SetAmount(0f);
        }

        public void OnPointerEnter(PointerEventData eventData) => hovered = button.IsInteractable();

        public void OnPointerExit(PointerEventData eventData) => hovered = false;

        void Update()
        {
            float goal = hovered ? 1f : 0f;
            if (Mathf.Approximately(amount, goal)) return;
            // unscaled, the pause menu runs with time stopped
            SetAmount(Mathf.MoveTowards(amount, goal, speed * Time.unscaledDeltaTime));
        }

        void SetAmount(float value)
        {
            amount = value;
            var colour = glow.color;
            colour.a = glowAlpha * value;
            glow.color = colour;
            // the glow is pivoted on its left edge, so growing it reads as a sweep across the button
            glow.rectTransform.localScale = new Vector3(Mathf.Lerp(0.2f, 1f, value), 1f, 1f);
            label.anchoredPosition = labelHome + new Vector2(slide * value, 0f);
        }
    }
}
