using Skyguard.Core;
using TMPro;
using UnityEngine;

namespace Skyguard.UI
{
    public class WaveBanner : MonoBehaviour
    {
        [SerializeField] TMP_Text headline;
        [SerializeField] TMP_Text detail;
        [SerializeField] CanvasGroup group;
        [SerializeField] float showFor = 3f;

        float visibleUntil;

        void Awake() => group.alpha = 0f;

        void OnEnable()
        {
            EventBus.Subscribe<WaveStarted>(OnWaveStarted);
            EventBus.Subscribe<WaveCleared>(OnWaveCleared);
            EventBus.Subscribe<UpgradeChosen>(OnUpgradeChosen);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<WaveStarted>(OnWaveStarted);
            EventBus.Unsubscribe<WaveCleared>(OnWaveCleared);
            EventBus.Unsubscribe<UpgradeChosen>(OnUpgradeChosen);
        }

        void OnWaveStarted(WaveStarted e) =>
            Flash(e.Endless ? $"ROUND {e.Wave}" : $"WAVE {e.Wave} / {e.TotalWaves}", e.Endless ? "Endless" : WaveHint(e.Wave));

        void OnWaveCleared(WaveCleared e) => Flash("WAVE CLEARED", e.TowerBonus ? "Tower above 50%  +500" : "");
        void OnUpgradeChosen(UpgradeChosen e) => Flash(e.Title.ToUpperInvariant(), "Upgrade installed");

        static string WaveHint(int wave) => wave switch
        {
            1 => "Scouts. Stop them before they reach the tower",
            3 => "Gunners shoot back. Block or dodge!",
            5 => "Shielded drones. Hit them from the side",
            10 => "The Carrier. Hit its glowing weak points",
            _ => ""
        };

        void Flash(string title, string sub)
        {
            headline.text = title;
            detail.text = sub;
            visibleUntil = Time.unscaledTime + showFor;
        }

        void Update()
        {
            float target = Time.unscaledTime < visibleUntil ? 1f : 0f;
            group.alpha = Mathf.MoveTowards(group.alpha, target, Time.unscaledDeltaTime * 3f);
        }
    }
}
