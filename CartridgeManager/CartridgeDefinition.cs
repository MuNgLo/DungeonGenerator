using Godot;
using Munglo.Commons;

namespace Munglo.WeaponsSystem.Cartridges;
[GlobalClass]
public partial class CartridgeDefinition : Resource
{
    [Export] public AMMOTYPES ammoType = AMMOTYPES.UNSET;
    [Export] public int amount = 1; // How many projectiles on cartirge Fire()
    [Export] public float damage;
    [Export] public DTYPE damageType;
    [Export] public DDIRECTIONAL directionStyle;
    [Export] public float physicalForce;
    [Export] public Mesh mesh;
    [ExportCategory("References by index to other info")]
    [Export] public int projectileIndex = -1;
    [Export] public int poolKey = -1;

}// EOF CLASS