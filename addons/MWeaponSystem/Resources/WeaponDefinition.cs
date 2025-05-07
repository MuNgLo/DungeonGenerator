using Godot;
using Munglo.Commons;

namespace Munglo.WeaponsSystem;
/// <summary>
/// Organizing Resources in the tree
/// All resources in weaponsystem should be derived from this class
/// </summary>
[GlobalClass]
public partial class WeaponDefinition : WeaponSystemResource
{
    [Export] public string name = "UnNamedWeapon";
    [Export] public PackedScene baseWeaponPrefab;
    [Export] public Mesh model;
    /// <summary>
    /// This weapon data should be an actual created project resource that exist fopr that weapon base type
    /// </summary>
    [Export] public WeaponData data;
    [ExportCategory("Player view limitations")]
    [Export] public float pitchMin = 7.5f;
    [Export] public float pitchMax = 175.0f;
}// EOF CLASS