using System;

namespace Skyguard.Core
{
    public interface IPoolable
    {
        // the pool hands every item its own way back home
        Action ReturnToPool { set; }
        void OnSpawn();
        void OnDespawn();
    }
}
