using Munglo.Commons;
using Godot;

namespace Munglo.WeaponsSystem;
public interface iWeaponListener
{
    void OnFire(Transform3D projectile);
}// EOF INTERFACE