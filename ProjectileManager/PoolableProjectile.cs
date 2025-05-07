using Godot;
using Munglo.Commons;

namespace Munglo.WeaponsSystem.Projectiles;
[GlobalClass]
public partial class PoolableProjectile : PoolableObjectBase, IPoolableObject
{
    public override void ReturnToPool()
    {
        //GD.Print("PoolableProjectile:ReturnToPool()");
        ProjectileManager.ReturnObject(this);
    }
    public override void ResetObject()
    {
        IsInUse = false;
        GetParent<StandardProjectile>().ResetForPool();
    }
}// EOF CLASS