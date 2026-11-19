using UnityEngine;

namespace Skyguard.Waves
{
    // sits on a tower, forward points out over the city. drones appear somewhere on a ring around it
    public class SpawnArea : MonoBehaviour
    {
        [SerializeField] Vector2 radius = new(28f, 42f);
        [SerializeField] Vector2 height = new(4f, 16f);

        public Vector3 PickPoint(SpawnSide side)
        {
            float angle = side switch
            {
                SpawnSide.Front => Random.Range(-60f, 60f),
                SpawnSide.Sides => Random.Range(60f, 120f) * (Random.value < 0.5f ? -1f : 1f),
                _ => Random.Range(-180f, 180f)
            };

            var forward = transform.forward;
            forward.y = 0f;
            var direction = Quaternion.AngleAxis(angle, Vector3.up) * forward.normalized;
            return transform.position
                   + direction * Random.Range(radius.x, radius.y)
                   + Vector3.up * Random.Range(height.x, height.y);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.35f, 0.2f, 0.6f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * height.x, radius.x);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * height.y, radius.y);
        }
    }
}
