using System;
using Skyguard.Flow;
using UnityEngine;
using UnityEngine.UI;

namespace Skyguard.UI
{
    // first launch only: which hand holds the gun, standing or seated, snap turn on or off
    public class ComfortPanel : MonoBehaviour
    {
        [SerializeField] Button handButton;
        [SerializeField] Button positionButton;
        [SerializeField] Button snapTurnButton;
        [SerializeField] Button continueButton;

        public event Action Done;

        static GameManager Game => GameManager.Instance;

        void Awake()
        {
            UiButtons.Hook(handButton, () => Change(() => Game.Settings.leftHanded = !Game.Settings.leftHanded));
            UiButtons.Hook(positionButton, () => Change(() => Game.Settings.seated = !Game.Settings.seated));
            UiButtons.Hook(snapTurnButton, () => Change(() => Game.Settings.snapTurn = !Game.Settings.snapTurn));
            UiButtons.Hook(continueButton, () =>
            {
                Game.CompleteComfortSetup();
                Done?.Invoke();
            });
        }

        void OnEnable() => Refresh();

        void Change(Action edit)
        {
            edit();
            Game.ApplySettings();
            Refresh();
        }

        void Refresh()
        {
            var s = Game.Settings;
            UiButtons.SetLabel(handButton, s.leftHanded ? "Blaster hand: Left" : "Blaster hand: Right");
            UiButtons.SetLabel(positionButton, s.seated ? "Playing: Seated" : "Playing: Standing");
            UiButtons.SetLabel(snapTurnButton, s.snapTurn ? "Snap turn: On" : "Snap turn: Off");
        }
    }
}
