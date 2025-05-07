using Godot;
using Munglo.Commons;
using Munglo.WeaponsSystem;
[GlobalClass]
public partial class PoolableObjectBase : WeaponSystemNode, IPoolableObject
{
    [Export] private int poolIndex = -1;
    [Export] private bool isInUse = false;
    public bool IsInUse { get => isInUse; set => isInUse = value; }
    public int PoolIndex { get => poolIndex; set => poolIndex = value; }

    public virtual void SetupReferences()
    {
        //if (GetComponent<AIObject>())
        //{
        //    if (GetComponent<Munglo.AI.AIUnit>())
        //    {
        //        GetComponent<Munglo.AI.AIUnit>().SetupReferences();
        //    }
        //}
    }

    public virtual void ResetObject()
    {

    }
    public virtual void ReturnToPool()
    {
        //ObjectPool.Instance.ReturnObject(this);
    }
}// EOF CLASS
