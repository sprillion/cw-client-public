using UnityEngine;

public abstract class PooledObject : MonoBehaviour
{
    protected ObjectPool pool;

    public void SetPool(ObjectPool pool)
    {
        this.pool = pool;
    }

    public virtual void OnGetted()
    {
        
    }

    public virtual void Release()
    {
        pool.ReturnToPool(this);
    }
}
