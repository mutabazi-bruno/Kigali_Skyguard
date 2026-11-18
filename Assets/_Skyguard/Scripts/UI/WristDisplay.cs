using Skyguard.Core;
using Skyguard.Flow;
using Skyguard.Platform;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Skyguard.UI
{
    // turn your off-hand wrist up to see shield, score and a pause button
    public class WristDisplay : MonoBehaviour
    {
        [SerializeField] Transform head;
        [SerializeField] CanvasGroup group;
        [SerializeField] Image[] shieldPips;
        [SerializeField] TMP_Text scoreText;
        [SerializeField] TMP_Text waveText;
        [SerializeField] TMP_Text fpsText;
        [SerializeField] Button pauseButton;
        [SerializeField] FrameTimer frameTimer;
        [SerializeField] Color pipOn = new(0.18f, 0.9f, 0.84f);
        [SerializeField] Color pipOff = new(0.25f, 0.1f, 0.08f);
        [SerializeField] float showAbove = 0.55f;

        bool showFps;

        void Awake()
        {
            UiButtons.Hook(pauseButton, () => GameManager.Instance.Pause());
            group.alpha = 0f;
        }

        void OnEnable()
        {
            EventBus.Subscribe<ScoreChanged>(OnScore);
            EventBus.Subscribe<PlayerHit>(OnPlayerHit);
            EventBus.Subscribe<PlayerShieldRefilled>(OnRefilled);
            EventBus.Subscribe<WaveStarted>(OnWaveStarted);
            EventBus.Subscribe<SettingsChanged>(OnSettings);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<ScoreChanged>(OnScore);
            EventBus.Unsubscribe<PlayerHit>(OnPlayerHit);
            EventBus.Unsubscribe<PlayerShieldRefilled>(OnRefilled);
            EventBus.Unsubscribe<WaveStarted>(OnWaveStarted);
            EventBus.Unsubscribe<SettingsChanged>(OnSettings);
        }

        void LateUpdate()
        {
            // the canvas faces out of the back of the wrist, so it shows when that side points at the eyes
            var toHead = (head.position - transform.position).normalized;
            bool facing = Vector3.Dot(-transform.forward, toHead) > showAbove;
            group.alpha = Mathf.MoveTowards(group.alpha, facing ? 1f : 0f, Time.unscaledDeltaTime * 6f);
            group.interactable = group.alpha > 0.9f;
            group.blocksRaycasts = group.interactable;

            if (showFps)
                fpsText.text = $"{frameTimer.CurrentFps:F0} fps";
        }

        void OnScore(ScoreChanged e) => scoreText.text = e.Multiplier > 1 ? $"{e.Score:N0}  x{e.Multiplier}" : $"{e.Score:N0}";
        void OnPlayerHit(PlayerHit e) => SetPips(e.ShieldLeft);
        void OnRefilled(PlayerShieldRefilled e) => SetPips(e.Points);
        void OnWaveStarted(WaveStarted e) => waveText.text = e.Endless ? $"Round {e.Wave}" : $"Wave {e.Wave}/{e.TotalWaves}";

        void OnSettings(SettingsChanged e)
        {
            showFps = e.Settings.showFps;
            fpsText.gameObject.SetActive(showFps);
        }

        void SetPips(int points)
        {
            for (int i = 0; i < shieldPips.Length; i++)
                shieldPips[i].color = i < points ? pipOn : pipOff;
        }
    }
}
