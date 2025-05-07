using Godot;
using Munglo.Commons;

namespace Munglo.WeaponsSystem;
[GlobalClass]
public partial class PLBase : WeaponSystemNode, IProjectileListener
{
    #region Interface Methods
    public virtual void OnBounce(IProjectile tr, Vector3 velocity, Vector3 inDir, Vector3 outDir, float remainingDistance, ref DamagePackage package) { }
    public virtual void OnFire(IProjectile tr, Vector3 velocity, ref DamagePackage package) { }
    public virtual void OnHit(IProjectile pr, Node3D hitTransform3D, Vector3 velocity, float distance, ref DamagePackage package) { }
    public virtual void OnImpact(IProjectile pr, Node3D hitTR, Vector3 velocity, float distance, ref DamagePackage package) { }
    public virtual void OnKill(IProjectile tr, Node3D hitTransform3D, Vector3 velocity, ref DamagePackage package) { }
    public virtual void OnSpawn(IProjectile tr, Vector3 velocity, ref DamagePackage package) { }
    public virtual void OnDespawn(Node3D Transform3D, Vector3 velocity, ref DamagePackage package) { }
    public virtual void OnTravelUpdate(ref Vector3 velocity, IProjectile tr, ref DamagePackage package, float distanceTravelled) { }
    public virtual void OnRagdollCallback(IProjectile tr, ref DamagePackage package) { }
    public virtual void OnRangedAim(ref Vector3 velocity, IProjectile tr, float framedistance) { }
    #endregion
}// EOF CLASS