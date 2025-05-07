using Godot;
using System;
using System.Collections.Generic;

namespace Munglo.WeaponsSystem.Projectiles;
[GlobalClass]
public partial class ProjectileDefinition : WeaponSystemResource
{
    [Export] public string definitionName;
    [Export] public Mesh mesh;
    [Export] public Material material;
    [Export(PropertyHint.Layers3DPhysics)] public int hittable;
    [Export] public float precastRadius = 0.2f;
    /// <summary>
    /// In Seconds
    /// </summary>
    [Export] public float TTL = 5.0f;
    [Export] public int minPoolsize;
    [Export] public int[] baseListeners;
}// EOF CLASS