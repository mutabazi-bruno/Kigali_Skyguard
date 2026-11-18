using System;
using Skyguard.Core;
using UnityEngine;

namespace Skyguard.Presentation
{
    public class PooledEffect : MonoBehaviour, IPoolable
    {
        [SerializeField] float lifetime = 2f;

        ParticleSystem[] systems;
        float age;

        public Action ReturnToPool { private get; set; }

        void Awake() => systems = GetComponentsInChildren<ParticleSystem>();

        void Update()
        {
            age += Time.deltaTime;
            if (age >= lifetime)
                ReturnToPool();
        }

        public void OnSpawn()
        {
            age = 0f;
            foreach (var system in systems)
            {
                system.Clear();
                system.Play();
            }
        }

        public void OnDespawn()
        {
            foreach (var system in systems)
                system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
