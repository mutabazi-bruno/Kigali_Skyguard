using Skyguard.Core;
using UnityEngine;

namespace Skyguard.Weapons
{
    public class ProjectileLauncher : MonoBehaviour
    {
        [SerializeField] Projectile prefab;
        [SerializeField] int prewarm = 30;

        ObjectPool<Projectile> pool;

        void Awake() => pool = new ObjectPool<Projectile>(prefab, prewarm, transform);

        public Projectile Launch(Vector3 position, Vector3 direction, float speedMultiplier = 1f)
        {
            var projectile = pool.Get(position, Quaternion.LookRotation(direction));
            projectile.Launch(position, direction, speedMultiplier);
            return projectile;
        }

        // pooled projectiles stay parented here, so clearing the sky is just a walk over the children
        public void RecallAll()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (child.gameObject.activeSelf && child.TryGetComponent(out Projectile projectile))
                    projectile.Recall();
            }
        }
    }
}
