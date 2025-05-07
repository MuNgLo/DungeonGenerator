using Godot;
using Munglo.GameEvents;
using System.Collections.Generic;
using Munglo.Commons;
using System;

namespace Munglo.WeaponsSystem.Projectiles;
[GlobalClass]
public partial class ProjectileManager : WeaponSystemNode3D
{
    private static ProjectileManager Instance;
    [Export] public bool debug = false;
    [Export] private PackedScene baseProjectilePrefab;
    [Export(PropertyHint.ResourceType, "ProjectileDefinition")] private Resource[] projectileDefinitions;
    [Export] private PLBase[] listeners;

    private Dictionary<int, ProjectilePool> pools;
    public static Resource[] ProjectileDefinitions { get => Instance.projectileDefinitions; }

    public override void _EnterTree()
    {
        Instance = this;
        pools = new Dictionary<int, ProjectilePool>();
    }
    public override void _Ready()
    {
        Events.Lobby.OnHostSetupReady += WhenHostSetupReady;
    }
    private void WhenHostSetupReady(object sender, EventArgs e)
    {
        if (debug) { GD.Print($"{Core.WHO}ProjectileManager::WhenHostSetupReady()"); }
        BuildPools();
    }
    private void BuildPools()
    {
        if (debug) { GD.Print($"{Core.WHO}ProjectileManager::BuildPools()"); }
        foreach (ProjectileDefinition projDef in projectileDefinitions)
        {
            if (projDef.ResourceName == "UnNamedProjectileVariant") { GD.Print($"Can't build pool from {projDef.ResourceName}"); continue; }


            Node3D prefab = baseProjectilePrefab.Instantiate() as Node3D;
            PoolableProjectile pp = prefab.GetNode<PoolableProjectile>("PoolableProjectile");
            pp.PoolIndex = pools.Keys.Count;
            (prefab as IProjectile).ApplyDefinition(projDef);
            pools[pools.Keys.Count] = new ProjectilePool(pools.Keys.Count, prefab, projDef.minPoolsize, this);
        }
        if (debug)
        {
            int count = 0;
            foreach (ProjectilePool pool in pools.Values)
            {
                count += pool.Count;
            }
            GD.Print($"{Core.WHO}ProjectileManager: Initialized {pools.Keys.Count} pools with a total of {count} objects from {projectileDefinitions.Length} projectileDefinitions.");
        }
    }


 

    /// <summary>
    /// Return an instance of the projectile that corresponds to the index
    /// If it fails it returns null
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public static IProjectile GetProjectile(int index)
    {
        if (Instance.pools.Keys.Count > index)
        {
            // the correct return
            if (Instance.debug) { GD.Print($"ProjectileManager::GetProjectile({index}) nbOfPools[{Instance.pools.Keys.Count}] PoolSize[{Instance.pools[index].Count}]"); }
            return Instance.pools[index].GetObject();
        }
        GD.PushError($"ProjectileManager::GetProjectile({index}) That index is to high");
        return null;
    }
    public static ProjectileDefinition GetProjectileDefinition(int index)
    {
        if (ProjectileDefinitions.Length > index)
        {
            return ProjectileDefinitions[index] as ProjectileDefinition;
        }
        return null;
    }
    public static PLBase GetListener(int index)
    {
        if (Instance.listeners.Length > index)
        {
            return Instance.listeners[index];
        }
        return null;
    }
    #region pool things

    public static void ReturnObject(PoolableProjectile go)
    {
        if (Instance.debug) { GD.Print($"ProjectileManager::ReturnObject({go.GetParent().Name})"); }
        Instance.pools[go.PoolIndex].ReturnObject(go);
    }
    private void ClearPools()
    {
        if (debug) { GD.Print($"ProjectileManager::ClearPools()"); }
        foreach (int key in pools.Keys)
        {
            if (pools[key] != null)
            {
                pools[key].Clear();
            }
        }
        foreach (Node child in GetChildren())
        {
            child.QueueFree();
        }
    }
    #endregion

   internal static string GetProjectileDescription(int v)
    {
        return string.Empty;
    }

}// EOF CLASS