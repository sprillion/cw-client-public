using UnityEngine;

public abstract class PooledObject : MonoBehaviour
{
    protected ObjectPool pool;
    private Vector3 _originalLocalScale;

    public void SetPool(ObjectPool pool)
    {
        this.pool = pool;
        _originalLocalScale = transform.localScale;
    }

    public void SetParentPreserveScale(Transform parent)
    {
        transform.SetParent(parent, false);
        transform.localScale = _originalLocalScale;
    }

    public void RestoreLocalScale()
    {
        transform.localScale = _originalLocalScale;
    }

    public virtual void OnGetted()
    {

    }

    public virtual void Release()
    {
        pool.ReturnToPool(this);
    }
}
