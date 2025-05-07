using Godot;

namespace Munglo.Commons;
/// <summary>
/// The interface for weapon effect listeners
/// </summary>
public interface IProjectileListener
{
    /// <summary>
    /// Called when a bounce happens
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="speed"></param>
    /// <param name="package"></param>
    void OnBounce(IProjectile proj, Vector3 velocity, Vector3 inDir, Vector3 outDir, float remainingDistance, ref DamagePackage package);
    /// <summary>
    /// Called when projectile despawns
    /// </summary>transform
    /// <param name="package"></param>
    /// <param name="effectMultiplier"></param>
    void OnFire(IProjectile proj, Vector3 velocity, ref DamagePackage package);
    /// <summary>
    /// Called on any hit on a CanTakeDamageBehaviour.
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="hitTransform"></param>
    /// <param name="speed"></param>
    /// <param name="package"></param>
    void OnHit(IProjectile proj, Node3D hitTransform, Vector3 velocity, float distance, ref DamagePackage package);

    /// <summary>
    /// Called on any hit without a CanTakeDamageBehaviour
    /// </summary>
    /// <param name="prTR"></param>
    /// <param name="impactedTR"></param>
    /// <param name="speed"></param>
    /// <param name="package"></param>
    void OnImpact(IProjectile proj, Node3D impactedTR, Vector3 velocity, float distance, ref DamagePackage package);

    /// <summary>
    /// Called when the projectile delivers a fatal damage to a CanTakeDamageBehaviour.TakeDamage().
    /// </summary>
    /// <param name="proj"></param>
    /// <param name="hitTransform"></param>
    /// <param name="speed"></param>
    /// <param name="package"></param>
    void OnKill(IProjectile proj, Node3D hitTransform, Vector3 velocity, ref DamagePackage package);

    /// <summary>
    /// Called as projectile OnEnable() gets executed.
    /// </summary>
    /// <param name="proj"></param>
    /// <param name="speed"></param>
    /// <param name="package"></param>
    void OnSpawn(IProjectile proj, Vector3 velocity, ref DamagePackage package);

    /// <summary>
    /// Called when projectile is removed. Returning to pool counts as removed.
    /// </summary>
    /// <param name="Transform3D"></param>
    /// <param name="velocity"></param>
    /// <param name="package"></param>
    void OnDespawn(Node3D Transform3D, Vector3 velocity, ref DamagePackage package) 
    { 

    }


    /// <summary>
    /// Called for each frame projectile travels some distance. 
    /// </summary>
    /// <param name="velocity"></param>
    /// <param name="proj"></param>
    /// <param name="package"></param>
    /// <param name="framedistance"></param>
    void OnTravelUpdate(ref Vector3 velocity, IProjectile proj, ref DamagePackage package, float distanceTravelled);


    /// <summary>
    /// TODO This is only used in simulation and maybe remove?
    /// </summary>
    /// <param name="velocity"></param>
    /// <param name="proj"></param>
    /// <param name="framedistance"></param>
    void OnRangedAim(ref Vector3 velocity, IProjectile proj, float framedistance);

    /// <summary>
    /// After a unit gets replaced by ragdoll this will deliver back the corresponding transform in the doll that was hit.
    /// </summary>
    /// <param name="tr"></param>
    void OnRagdollCallback(Node3D tr, ref DamagePackage package) { }

}// EOF INTERFACE
