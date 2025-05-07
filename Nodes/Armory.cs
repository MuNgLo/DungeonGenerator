using Godot;
using Munglo.Commons;

namespace Munglo.WeaponsSystem;
[GlobalClass]
public partial class Armory : WeaponSystemNode
{
    [Export] private bool debug = false;
    [Export] private WeaponDefinition[] weapons;

    /// <summary>
    /// Get weapon ready to use by index.
    /// If index not found it will return NULL
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public IWeapon GetWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length)
        {
            GD.PrintErr("Armory::GetWeapon(int {index}) Index out of range. Returning null");
            return null;
        }
        WeaponDefinition wd = weapons[index];
        Node3D wep = wd.baseWeaponPrefab.Instantiate() as Node3D;
        wep.Hide();
        IWeapon iw = wep as IWeapon;
        iw.SetWeaponValues(wd);
        if (debug) { GD.Print($"Armory::GetWeapon({index}) Found and made [{iw.WeaponName}]"); }
        return iw;
    }
}// EOF CLASSS