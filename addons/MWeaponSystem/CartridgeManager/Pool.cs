using Godot;
using System.Collections.Generic;

namespace Munglo.WeaponsSystem.Cartridges;
class Pool
{
    private readonly int poolIndex;
    private CartridgeManager manager;
    private Node3D prefab;
    private int size;
    private List<PoolableCartridge> pool;
    private bool debug => manager.debug;
    public int Count => pool.Count;

    public Pool(int pIndex, Node3D prefabToPool, int startSize, CartridgeManager man)
    {
        poolIndex = pIndex;
        prefab = prefabToPool;
        size = startSize;
        manager = man;
        BuildPool();
    }
    internal void Clear()
    {
        ReturnAllObjectsToPool();
        foreach (PoolableCartridge cartridge in pool)
        {
            cartridge.ReturnToPool();
        }
    }

    private void BuildPool()
    {
        pool = new List<PoolableCartridge>();
        // Init all objects and add to list
        for (int i = 0; i < size; i++)
        {
            AddOne();
        }
    }

    public void ReturnAllObjectsToPool()
    {
        foreach (PoolableCartridge obj in pool)
        {
            ReturnObject(obj);
        }
    }

    /// <summary>
    /// Get an unused object from pool. 
    //  if there is none, create one more entry in the pool and return that one.
    /// </summary>
    /// <returns></returns>
    internal PoolableCartridge GetObject()
    {
        pool.RemoveAll(p => p == null);
        PoolableCartridge go = pool.Find(p => p.IsInUse == false);
        if (go == null)
        {
            AddOne();
            go = pool[pool.Count - 1];
            if (debug) { GD.Print($"{Core.WHO}Pool{prefab.Name} extending. Size {pool.Count}"); }
        }
        go.IsInUse = true;
        return go;
    }

    private void AddOne()
    {
        Node3D go = prefab.Duplicate() as Node3D;
        (go as Cartridge).SetDamagePackage((prefab as Cartridge).Damage);
        go.Name = $"{prefab.Name}_{pool.Count}";
        PoolableObjectBase poolable = go.GetNode<PoolableObjectBase>("Poolable");
        poolable.PoolIndex = poolIndex;
        pool.Add(poolable as PoolableCartridge);
    }

    internal void ReturnObject(PoolableCartridge go)
    {
        if(debug){ GD.Print($"{Core.WHO}Pool::ReturnObject() returning[{go.GetParent().Name}]");}
        Node3D n = go.GetParent<Node3D>();
        n.GetParent().RemoveChild(n);
        n.RequestReady();
        go.ResetObject();
    }
}// EOF CLASS