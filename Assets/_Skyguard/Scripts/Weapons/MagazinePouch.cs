using Skyguard.Core;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Skyguard.Weapons
{
    // belt pouch that always has a fresh magazine waiting
    public class MagazinePouch : MonoBehaviour
    {
        public const int StandardCapacity = 12;

        [SerializeField] XRSocketInteractor socket;
        [SerializeField] Magazine magazinePrefab;
        [SerializeField] int prewarm = 8;
        [SerializeField] float restockDelay = 0.3f;

        ObjectPool<Magazine> pool;

        public int Capacity { get; set; } = StandardCapacity;

        void Awake()
        {
            // pooled magazines live in the world, not under the belt, otherwise dropped ones would follow the player around
            var poolRoot = new GameObject("Magazine Pool").transform;
            pool = new ObjectPool<Magazine>(magazinePrefab, prewarm, poolRoot);
            socket.selectExited.AddListener(_ => Invoke(nameof(Restock), restockDelay));
        }

        void Start() => Restock();

        public Magazine TakeFresh(Transform at)
        {
            var magazine = pool.Get(at.position, at.rotation);
            magazine.Refill(Capacity);
            return magazine;
        }

        void Restock()
        {
            if (socket.hasSelection)
                return;
            var magazine = TakeFresh(socket.attachTransform);
            socket.interactionManager.SelectEnter((IXRSelectInteractor)socket, (IXRSelectInteractable)magazine.Grab);
        }
    }
}
