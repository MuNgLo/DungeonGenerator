using Godot;
using Munglo.Commons;

namespace Munglo.WeaponsSystem.Listeners;

[GlobalClass]
public partial class  PLWeightedSpread : PLBase, IProjectileListener
{
    [Export] public bool debug = false;
    [Export] private float spread = 0.0f;
    [Export] private Curve _spreadCurve;
    public override void OnFire(IProjectile tr, Vector3 velocity, ref DamagePackage package)
    {
        if (debug) { GD.Print($"PLWeightedSpread::OnFire()"); }

        Quaternion spreadDelta = Quaternion.Identity;
        if (spread > 0.0f)
        {
            spreadDelta = Quaternion.FromEuler(WeightedDirection());
        }
        (tr as Node3D).Quaternion *= spreadDelta;
    }
    private Vector3 WeightedDirection()
    {
        Vector2 point = new Vector2((float)GD.RandRange(-1.0, 1.0), (float)GD.RandRange(-1.0, 1.0)).Normalized();
        Vector3 dir = new Vector3(point.X, point.Y, 0.0f) * spread;
        return dir * _spreadCurve.Sample((float)GD.RandRange(0.0, 1.0));
    }
}// EOF CLASS