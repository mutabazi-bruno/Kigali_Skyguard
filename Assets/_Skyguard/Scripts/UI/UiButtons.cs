using System;
using TMPro;
using UnityEngine.UI;

namespace Skyguard.UI
{
    public static class UiButtons
    {
        // every menu button clicks, so wiring the sound in one place saves repeating it everywhere
        public static Action ClickSound;

        public static void Hook(Button button, Action onClick)
        {
            button.onClick.AddListener(() =>
            {
                ClickSound?.Invoke();
                onClick();
            });
        }

        public static void SetLabel(Button button, string text) => button.GetComponentInChildren<TMP_Text>().text = text;
    }
}
