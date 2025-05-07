using Godot;
using Munglo.Commons;

namespace Munglo.WeaponsSystem;
/// <summary>
/// This is a nonacting class. Use it for mockup but implement your own recoil class.
/// Easist way is to inherit from this and override AddRecoil
/// Remember to pass weapon as IWeapon to constructor "WeaponRecoil(IWeapon wpn) : base(wpn)"
/// </summary>
public class StandardRecoil : IRecoil
{
    public Vector3 direction = Vector3.Back * 0.1f;
    public float fovChange = -1.0f;
    public float duration = 0.2f;
    public Curve weightCurve; // TODO fix curve change from Unity to Godot

    private IWeapon weapon;

    public StandardRecoil(IWeapon wpn)
    {
        weapon = wpn;
        weapon.Recoil = this;
    }
    public virtual void AddRecoil()
    {

    }
}// EOF CLASS