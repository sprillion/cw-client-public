using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace infrastructure.factories
{
    public static class Pool
    {
        private static List<PooledObject> _pooledObjects;
        private static readonly Dictionary<Type, ObjectPool> _pools = new Dictionary<Type, ObjectPool>();

        private static DiContainer _container;

        private static Transform _parent;
        
        public static void Initialize(DiContainer container)
        {
            _pools?.Clear();
            _pooledObjects?.Clear();
            _container = container;
            _parent = new GameObject("Pools").transform;
            _pooledObjects = Resources.LoadAll<PooledObject>("").ToList();
        }

        public static T Get<T>() where T : PooledObject
        {
            if (_pools.TryGetValue(typeof(T), out var pool))
            {
                return pool.GetObject<T>();
            }
            
            CreatePool<T>();
            return _pools[typeof(T)].GetObject<T>();
        }
        
        public static void CreatePool<T>(int startCount = 0, Transform parent = null)
        {
            var prefab = _pooledObjects.First(o => o.GetType() == typeof(T));
            _pools.TryAdd(typeof(T), new ObjectPool(prefab, startCount, parent ?? _parent, _container));
        }
    }
}