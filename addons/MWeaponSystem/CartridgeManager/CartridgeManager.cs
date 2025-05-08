using Munglo.Commons;
using System.Collections.Generic;
using Godot;
using System;

namespace Munglo.WeaponsSystem.Cartridges;

/// <summary>
/// This builds object pools as defined by host.
/// Host init the pools as host starts
/// Client inits pools as directed by host on client validation step in lobby
/// </summary>
[GlobalClass]
public partial class CartridgeManager : WeaponSystemNode3D
{
    public static CartridgeManager Instance;
    [Export] public bool debug = false;
    [Export] private PackedScene baseCartridgePrefab;
    [Export] public int initialSize = 20;
    [Export] private CartridgeDefinition[] cartridgeDefinitions;

    private Dictionary<int, Pool> pools;
    public override void _EnterTree()
    {
        Instance = this;
        pools = new Dictionary<int, Pool>();
    }
    public override void _Ready()
    {
        //Events.Lobby.OnHostSetupReady += WhenHostSetupReady;
    }

    private void WhenHostSetupReady(object sender, EventArgs e)
    {
        //if (debug) { GD.Print($"{Core.WHO}CartridgeManager::WhenHostSetupReady()"); }
        BuildPools();
    }

    private void BuildPools()
    {
        if (debug) { GD.Print($"CartridgeManager::BuildPools()"); }
        foreach (CartridgeDefinition cartDef in cartridgeDefinitions)
        {
            if (cartDef.ResourceName == "UnNamedCartridgeVariant") { GD.Print($"Can't build pool from {cartDef.ResourceName}"); continue; }
            cartDef.poolKey = pools.Keys.Count;
            // Apply config values to prefab
            Cartridge cr = baseCartridgePrefab.Instantiate() as Cartridge;
            cr.SetCartridgeValues(cartDef.poolKey, cartDef);
            pools[pools.Keys.Count] = new Pool(pools.Keys.Count, cr, initialSize, this);
        }
        if (debug)
        {
            int count = 0;
            foreach (Pool pool in pools.Values)
            {
                count += pool.Count;
            }
            GD.Print($"CartridgeManager: Initilized {pools.Keys.Count} pools with a total of {count} objects.");
        }
    }

    private void ClearPools()
    {
        if (debug) { GD.Print($"CartridgeManager::ClearPools()"); }
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
    public static CartridgeDefinition GetDefaultCartridgeDefinition(AMMOTYPES ammotype)
    {
        if (CartExists(ammotype, out int index))
        {
            return Instance.cartridgeDefinitions[index];
        }
        return Instance.cartridgeDefinitions[0];
    }

    private static bool CartExists(AMMOTYPES ammotype, out int i)
    {
        for (i = 0; i < Instance.cartridgeDefinitions.Length; i++)
        {
            if (Instance.cartridgeDefinitions[i].ammoType == ammotype) { return true; }
        }
        return false;
    }

    public static CartridgeDefinition GetDefaultCartridgeDefinition(int index)
    {
        if (index >= 0 && Instance.cartridgeDefinitions.Length >= index)
        {
            return Instance.cartridgeDefinitions[index];
        }
        return Instance.cartridgeDefinitions[0];
    }


    public static bool CartridgeExists(int index)
    {
        if (index >= 0 && Instance.cartridgeDefinitions.Length >= index)
        {
            return true;
        }
        return false;
    }
    public static ICartridge GetCartridge(AMMOTYPES ammotype)
    {
        return GetCartridge(GetDefaultCartridgeDefinition(ammotype).poolKey);
    }
    public static ICartridge GetCartridge(int poolKey)
    {
        if (!Instance.pools.ContainsKey(poolKey))
        {
            GD.Print($"CartridgeManager asked for index {poolKey} which don't exist.");
            return null;
        }

        PoolableCartridge go = Instance.pools[poolKey].GetObject();
        if (go == null)
        {
            GD.Print($"CartridgeManager:: pool({poolKey}) ran out.");
            return null;
        }
        return go.GetParent() as ICartridge;
    }

    static public void ReturnObject(PoolableCartridge go)
    {
        if (Instance.debug) { GD.Print($"CartridgeManager::ReturnObject({go.GetParent().Name})"); }
        Instance.pools[go.PoolIndex].ReturnObject(go);
    
    }

    //private void SceneManager_sceneUnloaded(Scene arg0)
    //{
    //    ClearPools();
    //}
    //
    //private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    //{
    //    BuildPools();
    //}
}// EOF CLASS