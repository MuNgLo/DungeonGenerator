using Godot;
using Munglo.Commons;
namespace Munglo.WeaponsSystem.Cartridges;

[GlobalClass]
public partial class PoolableCartridge : PoolableObjectBase, IPoolableObject
{
    public override void ReturnToPool()
    {
        //GD.Print($"{Core.WHO}PoolableCartridge::ReturnToPool()");
        CartridgeManager.ReturnObject(this);
    }
    public override void ResetObject()
    {
        IsInUse = false;
        ICartridge cart = GetParent() as ICartridge;
    }
}// EOF CLASS