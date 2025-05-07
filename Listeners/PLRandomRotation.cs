using Godot;
using Munglo.Commons;

namespace Munglo.WeaponsSystem.Listeners;

[GlobalClass]
public partial class PLRandomRotation : PLBase, IProjectileListener
{
    public override void OnFire(IProjectile tr, Vector3 velocity, ref DamagePackage package)
    {
        tr.GloabalRotation = tr.GloabalRotation.Rotated(Vector3.Forward, Mathf.Pi*2);
    }
}// EOF CLASS