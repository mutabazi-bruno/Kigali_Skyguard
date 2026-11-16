using System;
using System.Collections.Generic;
using UnityEngine;

namespace Skyguard.Core
{
    // typed pub/sub. gameplay publishes, ui/audio/fx/haptics subscribe, and neither side knows the other exists
    public static class EventBus
    {
        static readonly Dictionary<Type, Delegate> handlers = new();

        public static void Subscribe<T>(Action<T> handler) where T : struct
        {
            handlers.TryGetValue(typeof(T), out var existing);
            handlers[typeof(T)] = Delegate.Combine(existing, handler);
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            if (!handlers.TryGetValue(typeof(T), out var existing))
                return;

            var remaining = Delegate.Remove(existing, handler);
            if (remaining == null)
                handlers.Remove(typeof(T));
            else
                handlers[typeof(T)] = remaining;
        }

        public static void Publish<T>(T evt) where T : struct
        {
            if (handlers.TryGetValue(typeof(T), out var handler))
                ((Action<T>)handler).Invoke(evt);
        }

        // stale handlers would survive between play sessions when domain reload is turned off
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Clear() => handlers.Clear();
    }
}
