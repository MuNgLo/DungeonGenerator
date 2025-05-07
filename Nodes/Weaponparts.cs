using Munglo.Commons;
using Godot;
using System;

namespace Munglo.WeaponsSystem;
/// <summary>
/// Defines the grips and muzzle Position/Rotation relative to the MountPOinnt Transform3D
/// </summary>
[GlobalClass]
public partial class Weaponparts : WeaponSystemNode
{
    [Export] public bool debug = false;
    [Export] private Node3D cartridgePoint;
    [Export] private Node3D muzzlePoint;
    [Export] private Node3D gripLeft;
    [Export] private Node3D gripRight;
    [Export] private Node3D ikPoleLeft;
    [Export] private Node3D ikPoleRight;

    private Node3D cartridge;

    public Node3D CartridgePoint { get => cartridgePoint; }
    public Node3D MuzzlePoint { get => muzzlePoint; }
    public ICartridge Cartridge { get => cartridge as ICartridge; }
    public Node3D GripLeft { get => gripLeft; }
    public Node3D GripRight { get => gripRight; }
    public Node3D IKPoleLeft { get => ikPoleLeft; }
    public Node3D IKPoleRight { get => ikPoleRight; }

    /// <summary>
    /// Brings the cartridge into tree as a child to cartridgepoint and zeros position/rotation
    /// Sets up the cartridge reference
    /// </summary>
    /// <param name="cart"></param>
    /// <param name="show"></param>
    public void MountCartridge(ICartridge cart, bool show)
    {
        cartridge = cart as Node3D;
        if (cart != null)
        {
            cartridgePoint.AddChild(cartridge);
            cartridge.Owner = cartridgePoint;
            cartridge.Position = Vector3.Zero;
            cartridge.Rotation = Vector3.Zero;
            if(debug){GD.Print($"{Core.WHO}Weaponparts::MountCartridge() Cart Path[{cartridge.GetPath()}] show[{show}]");}
        }
    }
    /// <summary>
    /// Returns cartridge to pool and drops the refrence
    /// </summary>
    public void UnMountCartridge()
    {
        if (cartridge != null)
        {
            (cartridge as  ICartridge).ReturnToPool();
            cartridge = null;
        }
    }
}// EOF CLASS