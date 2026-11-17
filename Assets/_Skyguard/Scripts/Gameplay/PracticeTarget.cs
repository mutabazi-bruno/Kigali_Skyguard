using Skyguard.Core;
using UnityEngine;

namespace Skyguard.Gameplay
{
    public class PracticeTarget : MonoBehaviour, IDamageable
    {
        [SerializeField] Transform face;
        [SerializeField] float wobbleDegrees = 25f;

        float wobble;

        public void Show(Transform anchor)
        {
            transform.SetPositionAndRotation(anchor.position, anchor.rotation);
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);

        public void TakeDamage(DamageInfo hit)
        {
            if (hit.Source != DamageSource.Player)
                return;
            wobble = 1f;
            EventBus.Publish(new TargetHit { Position = hit.Point });
        }

        void Update()
        {
            if (wobble <= 0f)
                return;
            wobble = Mathf.Max(0f, wobble - Time.deltaTime * 2.5f);
            face.localRotation = Quaternion.Euler(Mathf.Sin(wobble * 20f) * wobbleDegrees * wobble, 0f, 0f);
        }
    }
}
