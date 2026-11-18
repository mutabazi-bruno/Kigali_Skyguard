using System.Collections.Generic;
using Skyguard.Core;
using Skyguard.Data;
using Skyguard.UI;
using UnityEngine;

namespace Skyguard.Presentation
{
    // the tower's assistant. queues her lines so they never talk over each other,
    // and adds the odd reaction line when something happens in a fight
    public class IshemaVoice : MonoBehaviour
    {
        [SerializeField] SubtitleTable table;
        [SerializeField] AudioSource voice;
        [SerializeField] SubtitlePanel subtitles;
        [SerializeField] float gapBetweenLines = 0.35f;
        [SerializeField] float reactionCooldown = 25f;

        readonly Queue<SubtitleTable.Line> queue = new();
        SubtitleLanguage language;
        float busyUntil;
        float nextReaction;
        bool towerWarned;

        void OnEnable()
        {
            EventBus.Subscribe<IshemaSpoke>(OnSpoke);
            EventBus.Subscribe<SettingsChanged>(OnSettings);
            EventBus.Subscribe<MagazineLoaded>(OnLoaded);
            EventBus.Subscribe<TowerDamaged>(OnTowerDamaged);
            EventBus.Subscribe<PlayerHit>(OnPlayerHit);
            EventBus.Subscribe<WaveStarted>(OnWaveStarted);
            EventBus.Subscribe<GameStateChanged>(OnStateChanged);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<IshemaSpoke>(OnSpoke);
            EventBus.Unsubscribe<SettingsChanged>(OnSettings);
            EventBus.Unsubscribe<MagazineLoaded>(OnLoaded);
            EventBus.Unsubscribe<TowerDamaged>(OnTowerDamaged);
            EventBus.Unsubscribe<PlayerHit>(OnPlayerHit);
            EventBus.Unsubscribe<WaveStarted>(OnWaveStarted);
            EventBus.Unsubscribe<GameStateChanged>(OnStateChanged);
        }

        void Update()
        {
            if (queue.Count == 0 || Time.unscaledTime < busyUntil)
                return;

            var line = queue.Dequeue();
            float length = 2.5f;
            if (line.voice != null)
            {
                voice.PlayOneShot(line.voice);
                length = line.voice.length;
            }
            subtitles.Show(line.Text(language), length + 0.6f);
            busyUntil = Time.unscaledTime + length + gapBetweenLines;
        }

        void Say(string key)
        {
            if (table.TryGet(key, out var line))
                queue.Enqueue(line);
        }

        void OnSpoke(IshemaSpoke e) => Say(e.Key);
        void OnSettings(SettingsChanged e) => language = e.Settings.language;

        void OnStateChanged(GameStateChanged e)
        {
            // stale tutorial lines shouldn't keep playing after a quit to menu
            if (e.State == GameStateId.MainMenu)
                queue.Clear();
        }

        void OnWaveStarted(WaveStarted e)
        {
            towerWarned = false;
            // countdown to dawn on the quieter waves
            if (e.Endless)
                return;
            if (e.Wave == 2) Say("hours_6");
            else if (e.Wave == 6) Say("hours_4");
            else if (e.Wave == 9) Say("hours_1");
        }

        void OnLoaded(MagazineLoaded e)
        {
            if (e.FastReload && React())
                Say("reload_fast");
        }

        void OnTowerDamaged(TowerDamaged e)
        {
            if (!towerWarned && e.Health * 10 < e.MaxHealth * 3)
            {
                towerWarned = true;
                Say("tower_low");
            }
        }

        void OnPlayerHit(PlayerHit e)
        {
            if (e.ShieldLeft == 1)
                Say("shield_low");
        }

        bool React()
        {
            if (Time.unscaledTime < nextReaction || queue.Count > 0)
                return false;
            nextReaction = Time.unscaledTime + reactionCooldown;
            return true;
        }
    }
}
