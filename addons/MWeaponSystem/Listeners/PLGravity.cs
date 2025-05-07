using Godot;
using Munglo.Commons;

namespace Munglo.WeaponsSystem.Listeners;

[GlobalClass]
public partial class PLGravity : PLBase, IProjectileListener
{
    public override void OnTravelUpdate(ref Vector3 velocity, IProjectile proj, ref DamagePackage package, float framedistance)
    {
        ApplyGravity(ref velocity, proj.LastDelta);
    }
    public override void OnRangedAim(ref Vector3 velocity, IProjectile proj, float framedistance)
    {
        ApplyGravity(ref velocity, proj.LastDelta);
    }
    private void ApplyGravity(ref Vector3 velocity, float frameDelta)
    {
        velocity += ProjectSettings.GetSetting("physics/3d/default_gravity_vector").AsVector3() * (float)ProjectSettings.GetSetting("physics/3d/default_gravity").AsDouble() * frameDelta;
    }
}// EOF CLASS