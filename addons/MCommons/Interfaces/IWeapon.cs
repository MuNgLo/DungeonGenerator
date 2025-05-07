using System;
using Godot;

namespace Munglo.Commons;
/// <summary>
/// All weapons must implement this interface to be compatible with the modules
/// </summary>
public interface IWeapon
{
    public string WeaponName { get; }

    /// <summary>
    /// Raised when an attack is made. Sends the duration of the attack.
    /// </summary>
    public EventHandler<float> OnAttack { get; }
    /// <summary>
    /// Called when chambering a projectile.
    /// </summary>
    public EventHandler OnChamber { get; }
    /// <summary>
    /// Called when reload starts.
    /// </summary>
    public EventHandler<float> OnReload { get; }

    public EventHandler OnReloadCompleted { get; }

    /// <summary>
    /// Called as weapon is mounted to player or dropped.
    /// </summary>
    public EventHandler OnStopAnimation { get; }
    /// <summary>
    /// Node3D of the weapon 
    /// </summary>
    public Node3D N3D { get; }
    /// <summary>
    /// The IRecoil.AddRecoil() is the standardized way weapons add recoil
    /// </summary>
    public IRecoil Recoil { get; set; }
    /// <summary>
    /// maximum range for this weapon
    /// </summary>
    public float Range { get; }
    /// <summary>
    /// AIObjectID of the owner. -1 when not owned
    /// </summary>
    public int OwnerAIOID { get; set; }

    public float PitchMin { get; }
    public float PitchMax { get; }
    //public int WeaponIndex { get; }

    public int CurrentCharges { get; }

    public ICartridge NextCartridge { get; }

    public AMMOTYPES AmmoType { get; }
    /// <summary>
    /// Defines the parts of the weapon
    /// </summary>
    public WeaponsSystem.Weaponparts Parts { get; }
    /// <summary>
    /// Returns True if the weapon can be used for an attack
    /// </summary>
    /// <returns></returns>
    public bool CanAttack();
    /// <summary>
    /// Returns True if the weapon can be used for an attack
    /// with the given mode index
    /// </summary>
    /// <returns></returns>
    //public bool CanAttack(int modeIndex);

    public void SetWeaponValues(WeaponsSystem.WeaponDefinition values);

    /// <summary>
    /// This makes an attack with the weapon
    /// </summary>
    public void Attack();
    //public void Attack(int modeIndex);

    public void Aim(Vector3 targetPoint, Vector3 upAxis);

    public bool IsInCoolDown { get; }
    public bool IsReloading { get; }

    public WEAPONSTATE State { get; set; }

    #region IK references
    public Node3D GripLeft { get; }
    public Node3D GripRight { get; }
    public Node3D PoleLeft { get; }
    public Node3D PoleRight { get; }
    #endregion
    public void MountWeaponToPlayer(Node3D mountPoint);
    public void AIControlWeapon(Node3D mountPoint, int aiObjectID);
    public void TurnTowardsDefault(float angle);
    public void DropWeapon();
    public void FireCartridge();
    public bool TurnTowardsPoint(Vector3 aimTargetPoint, float angle);
    /// <summary>
    /// Will start a reload of weapon
    /// </summary>
    public void Reload();
    public void ReloadCompleted();
    public void Eject();

    public void ChamberProjectile();

    public void AnimPlayWindup(string animName);
    public void AnimPlayExecusion(string animName);
    public void AnimPlayWinddown(string animName);

}// EOF INTERFACE
