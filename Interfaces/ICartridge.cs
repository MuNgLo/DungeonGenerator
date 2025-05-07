using Godot;
using Munglo.WeaponsSystem.Cartridges;

namespace Munglo.Commons;
public interface ICartridge
{
    public DamagePackage Damage { get; }
    public int ProjectileIndex { get; set; }
    public void Eject();
    public void Eject(Vector3 direction, float force);
    public bool Spent { get; }
    /// <summary>
    /// When this gets called the cartridge needs to do a full reset and be ready for reuse
    /// </summary>
    public void ReturnToPool();
    public void SetCartridgeValues(int cartIndex, CartridgeDefinition cDef);
    public void Fire(Node3D muzzle, float speed, float damageMultiplier, IWeapon weapon, int ownerID);
    public void SetDamagePackage(CartridgeDefinition package);
    public void Hide();
    public void Show();
    public void UnSpend();
}//EOF INTERFACE