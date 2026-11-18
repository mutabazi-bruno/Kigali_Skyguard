using System;
using Skyguard.Core;
using UnityEngine;

namespace Skyguard.Presentation
{
    [RequireComponent(typeof(AudioSource))]
    public class PooledAudio : MonoBehaviour, IPoolable
    {
        AudioSource source;

        public Action ReturnToPool { private get; set; }

        void Awake() => source = GetComponent<AudioSource>();

        public void Play(AudioClip clip, float volume, float pitch, bool spatial)
        {
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.spatialBlend = spatial ? 1f : 0f;
            source.Play();
        }

        void Update()
        {
            if (!source.isPlaying && !AudioListener.pause)
                ReturnToPool();
        }

        public void OnSpawn() { }
        public void OnDespawn() => source.Stop();
    }
}
