using System;
using Skyguard.Core;
using UnityEngine;

namespace Skyguard.Weapons
{
    public abstract class Projectile : MonoBehaviour, IPoolable
    {
        [SerializeField] float speed = 60f;
        [SerializeField] float lifetime = 2.5f;
        [SerializeField] float radius = 0.04f;
        [SerializeField] protected int damage = 1;
        [SerializeField] protected LayerMask hitMask;

        TrailRenderer trail;
        Vector3 direction;
        float speedScale = 1f;
        float age;

        public Action ReturnToPool { private get; set; }
        protected TrailRenderer Trail => trail;

        protected virtual void Awake() => trail = GetComponentInChildren<TrailRenderer>();

        public void Launch(Vector3 position, Vector3 heading, float speedMultiplier = 1f)
        {
            direction = heading.normalized;
            speedScale = speedMultiplier;
            age = 0f;
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(direction));
            if (trail)
                trail.Clear();
        }

        void Update()
        {
            float step = speed * speedScale * Time.deltaTime;

            // cast the whole distance this frame covers, so a 60 m/s bolt can't skip past a drone between frames
            if (Physics.SphereCast(transform.position, radius, direction, out var hit, step, hitMask, QueryTriggerInteraction.Ignore))
            {
                if (OnImpact(hit, direction))
                {
                    transform.position = hit.point;
                    Finish();
                    return;
                }
                // went straight through, carry on from just past whatever it hit
                transform.position = hit.point + direction * 0.3f;
            }
            else
            {
                transform.position += direction * step;
            }

            age += Time.deltaTime;
            if (age >= lifetime)
                Finish();
        }

        // return true if the projectile stops here
        protected abstract bool OnImpact(RaycastHit hit, Vector3 heading);

        protected virtual void OnFinished() { }

        public void Recall() => ReturnToPool();

        void Finish()
        {
            OnFinished();
            ReturnToPool();
        }

        public virtual void OnSpawn() { }
        public virtual void OnDespawn() { }
    }
}
