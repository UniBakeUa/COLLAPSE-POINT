using System;
using UnityEngine;
using Zenject;

namespace _Game.CodeBase.Core.ObjectPoolModule.Scripts
{
    public class ObjectPool<T> : MonoMemoryPool<T> where T : Component, IPoolable<IMemoryPool>, IDisposable
    {
        protected override void OnCreated(T item)
        {
            base.OnCreated(item);
        }

        protected override void OnSpawned(T item)
        {
            base.OnSpawned(item);
            Spawned(item);
            item.gameObject.SetActive(true);
        }

        protected virtual void Spawned(T item) { }

        protected override void OnDespawned(T item)
        {
            item.gameObject.SetActive(false);
            Despawned(item);
            base.OnDespawned(item);
        }
        protected virtual void Despawned(T item) { }
    }
}