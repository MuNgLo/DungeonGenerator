using System;
using Godot;
using Munglo.Commons;

namespace Munglo.WeaponsSystem;

/// <summary>
/// Create instances of this in Project for each base weapon type the project has
/// </summary>
[GlobalClass]
public partial class WeaponData : Resource
{
    [Export] public WEAPONTYPE baseType = WEAPONTYPE.UNSET;
    [Export] public string crosshair = "";
    [Export] public AMMOTYPES ammoType = AMMOTYPES.UNSET;
    [Export] public float baseRange = 1.0f;
    [Export] public float baseSpeed = 80.0f;
    [Export] public float reloadTime = 1.0f;
    [Export] public float baseAttackRate = 1.0f;
    [Export] public int attackCost = 1;
    [Export] public int chargesMax = 1;
    public WeaponData()
    {

    }

   
    /*public WeaponData(WeaponData oldData)
{
   baseType = oldData.baseType;
   crosshair = oldData.crosshair;
   ammoType = oldData.ammoType;
   baseRange = oldData.baseRange;
   baseSpeed = oldData.baseSpeed;
   reloadTime = oldData.reloadTime;
   baseAttackRate = oldData.baseAttackRate;
   damageSource = oldData.damageSource;
   damageMultiplier = oldData.damageMultiplier;
   attackCost = oldData.attackCost;
   chargesCurrent = oldData.chargesCurrent;
   chargesMax = oldData.chargesMax;
   pitchMin = oldData.pitchMin;
   pitchMax = oldData.pitchMax;
}*/
}// EOF CLASS