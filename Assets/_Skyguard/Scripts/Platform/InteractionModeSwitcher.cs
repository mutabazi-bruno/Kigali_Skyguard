using Skyguard.Core;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Skyguard.Platform
{
    // pointer rays only show up when there's a menu to point at. during a fight you grab with your hands
    public class InteractionModeSwitcher : MonoBehaviour
    {
        [SerializeField] NearFarInteractor[] hands;

        void OnEnable() => EventBus.Subscribe<GameStateChanged>(OnStateChanged);
        void OnDisable() => EventBus.Unsubscribe<GameStateChanged>(OnStateChanged);

        void OnStateChanged(GameStateChanged e)
        {
            bool menu = e.State is GameStateId.MainMenu or GameStateId.Paused or GameStateId.Results;
            foreach (var hand in hands)
                hand.enableFarCasting = menu;
        }
    }
}
