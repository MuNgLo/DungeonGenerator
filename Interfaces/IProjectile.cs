using Godot;
using Munglo.WeaponsSystem.Projectiles;
namespace Munglo.Commons;
public interface IProjectile
{
    [Export(PropertyHint.Layers3DPhysics)]
    public int WillHitLayers { get; }

    public int[] Listeners { get; }
    public int[] ExtraListeners { get; }
    public Node3D N3D { get; }
    public Vector3 GloabalForward { get; }
    public Vector3 GloabalRotation { get; set; }
    public Vector3 Velocity { get; }

    public bool SkipMovementOneFrame { get; set; }
    public DamagePackage Damage { get; }

    public float WeaponMultiplier { get; }
    public float Radius { get; }
    public float LastDelta { get; }
    public bool IsRunning { get; }


    public void Fire(float speed, DamagePackage dPackage, bool debug);
    public void Stop();
    public void Despawn();
    public void AddLayer(int index);
    public void Move(Vector3 from, Vector3 direction, float distance, float delta);
    /// <summary>
    /// User raycast instead of spherecast for precise collision
    /// </summary>
    /// <param name="from"></param>
    /// <param name="direction"></param>
    /// <param name="distance"></param>
    public void MovePrecision(Vector3 from, Vector3 direction, float distance, float delta);
    public void RemoveLayer(int index);
    public void ChangeVelocity(Vector3 newVelocity);
    public void RagdollCallback(Node3D hit);
    /// <summary>
    /// When projectile bounces this gets called allowing listeners to react
    /// </summary>
    /// <param name="hit"></param>
    /// <param name="inDir"></param>
    /// <param name="outDir"></param>
    /// <param name="remainingDistance"></param>
    public void RaiseBounce(Node3D hit, Vector3 inDir, Vector3 outDir, float remainingDistance);

    public void SetDamagePackage(DamagePackage package);
    void ApplyDefinition(ProjectileDefinition projDef);
}// EOF ITNERFACE





