using System.Collections.Generic;
using UnityEngine;

namespace Skyguard.Core
{
    // no Instantiate/Destroy once a wave is running, everything comes from here
    public class ObjectPool<T> where T : Component, IPoolable
    {
        readonly T prefab;
        readonly Transform parent;
        readonly Stack<T> idle = new();

        public ObjectPool(T prefab, int prewarm, Transform parent)
        {
            this.prefab = prefab;
            this.parent = parent;
            for (int i = 0; i < prewarm; i++)
                idle.Push(Create());
        }

        public T Get(Vector3 position, Quaternion rotation)
        {
            var item = idle.Count > 0 ? idle.Pop() : Create();
            item.transform.SetPositionAndRotation(position, rotation);
            item.gameObject.SetActive(true);
            item.OnSpawn();
            return item;
        }

        public void Release(T item)
        {
            if (!item.gameObject.activeSelf)
                return;
            item.OnDespawn();
            item.gameObject.SetActive(false);
            idle.Push(item);
        }

        T Create()
        {
            var item = Object.Instantiate(prefab, parent);
            item.gameObject.SetActive(false);
            item.ReturnToPool = () => Release(item);
            return item;
        }
    }
}
