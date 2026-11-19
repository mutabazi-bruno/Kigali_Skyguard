using System;
using System.Collections.Generic;
using Skyguard.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Skyguard.Drones
{
    public enum DroneState { Inactive, Active, Dying }

    [RequireComponent(typeof(Rigidbody))]
    public abstract class Drone : MonoBehaviour, IDamageable, IPoolable
    {
        [SerializeField] protected DroneConfig config;
        [SerializeField] float fallTime = 3f;
        [SerializeField] float rotorSpeed = 1800f;

        protected DroneContext context;
        protected int health;

        Rigidbody body;
        TrailRenderer trail;
        readonly List<Transform> rotors = new();
        float dyingFor;

        public DroneType Type => config.type;
        public DroneConfig Config => config;
        public DroneState State { get; private set; }
        public bool IsAlive => State == DroneState.Active;
        public Action ReturnToPool { private get; set; }

        protected float Speed => config.speed * context.SpeedMultiplier;

        protected virtual void Awake()
        {
            body = GetComponent<Rigidbody>();
            trail = GetComponent<TrailRenderer>();
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                if (t.name.StartsWith("rotor"))
                    rotors.Add(t);
            }
        }

        public void Init(DroneContext droneContext)
        {
            context = droneContext;
            health = config.maxHealth;
            State = DroneState.Active;
            // pooled drones would otherwise draw a streak from where they died to the new spawn point
            if (trail != null)
                trail.Clear();
            OnInit();
        }

        protected virtual void OnInit() { }

        public void Tick(float deltaTime)
        {
            if (State == DroneState.Dying)
            {
                dyingFor += deltaTime;
                if (dyingFor >= fallTime)
                    ReturnToPool();
                return;
            }

            foreach (var rotor in rotors)
                rotor.Rotate(0f, rotorSpeed * deltaTime, 0f, Space.Self);
            Behave(deltaTime);
        }

        protected abstract void Behave(float deltaTime);

        public virtual void TakeDamage(DamageInfo hit)
        {
            if (!IsAlive || hit.Source != DamageSource.Player)
                return;
            ApplyDamage(hit);
        }

        protected void ApplyDamage(DamageInfo hit)
        {
            health -= hit.Amount;
            EventBus.Publish(new DroneHit { Position = hit.Point, Type = Type });
            if (health <= 0)
                Die(hit.Direction);
        }

        // shot down: switch to physics and let it tumble out of the sky
        protected void Die(Vector3 push)
        {
            State = DroneState.Dying;
            dyingFor = 0f;
            body.isKinematic = false;
            body.useGravity = true;
            body.linearVelocity = push * 4f + Vector3.up * 2f;
            body.angularVelocity = Random.insideUnitSphere * 6f;
            EventBus.Publish(new DroneDestroyed { Position = transform.position, Type = Type, Points = config.points, ByPlayer = true });
        }

        // reached the tower: no falling, it just goes off on impact
        protected void Crash()
        {
            context.Tower.TakeDamage(new DamageInfo
            {
                Amount = config.towerDamage,
                Point = transform.position,
                Direction = transform.forward,
                Source = DamageSource.Drone
            });
            State = DroneState.Dying;
            EventBus.Publish(new DroneDestroyed { Position = transform.position, Type = Type, ByPlayer = false });
            ReturnToPool();
        }

        public void Despawn() => ReturnToPool();

        protected void FlyTowards(Vector3 target, float deltaTime)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, Speed * deltaTime);
        }

        protected void TurnTowards(Vector3 point, float degreesPerSecond, float deltaTime)
        {
            var direction = point - transform.position;
            if (direction.sqrMagnitude < 0.001f)
                return;
            var goal = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, goal, degreesPerSecond * deltaTime);
        }

        public virtual void OnSpawn()
        {
            body.isKinematic = true;
            body.useGravity = false;
            dyingFor = 0f;
        }

        public virtual void OnDespawn() => State = DroneState.Inactive;
    }
}
