using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ObjectPool 
{
    private readonly PooledObject _prefab;

    private readonly Queue<PooledObject> _pool;

    private readonly Transform _parent;
    private readonly DiContainer _container;


    public ObjectPool(PooledObject prefab, int startCount, Transform parent = null, DiContainer diContainer = null)
    {
        _prefab = prefab;
        _container = diContainer;
        if (parent == null)
        {
            parent = new GameObject(prefab.name).transform;
        }
        _parent = parent;

        _pool = new Queue<PooledObject>();
        for (int i = 0; i < startCount; i++)
        {
            _pool.Enqueue(CreateObject());
        }
    }

    public PooledObject GetObject()
    {
        if (_pool.Count == 0)
        {
            _pool.Enqueue(CreateObject());
        }
        var obj = _pool.Dequeue();
        obj.gameObject.SetActive(true);
        obj.OnGetted();
        return obj;
    }
    
    public T GetObject<T>() where T : PooledObject
    {
        return GetObject() as T;
    }

    public void ReturnToPool(PooledObject obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(_parent);
        _pool.Enqueue(obj);
    }

    private PooledObject CreateObject()
    {
        PooledObject obj = Object.Instantiate(_prefab, _parent);
        obj.SetPool(this);
        obj.gameObject.SetActive(false);
        _container?.InjectGameObject(obj.gameObject);
        return obj;
    }
}
