using Skyguard.Core;
using UnityEngine;

namespace Skyguard.Platform
{
    // the fps benchmark for the gdd. averages over the whole run and logs each wave so it shows up in logcat
    public class FrameTimer : MonoBehaviour
    {
        int runFrames;
        float runTime;
        int waveFrames;
        float waveTime;
        float waveWorst;
        int wave;

        public float AverageFps => runTime > 0f ? runFrames / runTime : 0f;
        public float CurrentFps { get; private set; }

        void OnEnable()
        {
            EventBus.Subscribe<WaveStarted>(OnWaveStarted);
            EventBus.Subscribe<WaveCleared>(OnWaveCleared);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<WaveStarted>(OnWaveStarted);
            EventBus.Unsubscribe<WaveCleared>(OnWaveCleared);
        }

        public void ResetRun()
        {
            runFrames = 0;
            runTime = 0f;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (dt <= 0f)
                return;

            CurrentFps = Mathf.Lerp(CurrentFps, 1f / dt, 0.1f);
            runFrames++;
            runTime += dt;
            waveFrames++;
            waveTime += dt;
            waveWorst = Mathf.Max(waveWorst, dt);
        }

        void OnWaveStarted(WaveStarted e)
        {
            wave = e.Wave;
            waveFrames = 0;
            waveTime = 0f;
            waveWorst = 0f;
        }

        void OnWaveCleared(WaveCleared _)
        {
            if (waveTime <= 0f)
                return;
            Debug.Log($"[perf] wave {wave}: avg {waveFrames / waveTime:F1} fps, worst frame {waveWorst * 1000f:F1} ms");
        }
    }
}
