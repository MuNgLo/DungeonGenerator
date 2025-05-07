using Godot;
using System.Collections.Generic;
using Munglo.Commons;
namespace Munglo.WeaponsSystem.Projectiles;
class ProjectilePool
{
    private readonly int poolIndex;
    private ProjectileManager manager;
    private Node3D prefab;
    private int size;
    private List<PoolableProjectile> pool;
    private bool debug => manager.debug;
    public int Count => pool.Count;

    public ProjectilePool(int pIndex, Node3D prefabToPool, int startSize, ProjectileManager man)
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
        foreach (PoolableProjectile proj in pool)
        {
            proj.ReturnToPool();
        }
    }

    private void BuildPool()
    {
        pool = new List<PoolableProjectile>();
        // Init all objects and add to list
        for (int i = 0; i < size; i++)
        {
            AddOne();
        }
    }

    public void ReturnAllObjectsToPool()
    {
        foreach (PoolableProjectile obj in pool)
        {
            ReturnObject(obj);
        }
    }

    /// <summary>
    /// Get an unused object from pool.
    //  if there is non, create one more entry in the pool and return that one.
    /// </summary>
    /// <returns></returns>
    internal IProjectile GetObject()
    {
        pool.RemoveAll(p => p == null);
        PoolableProjectile go = pool.Find(p => p.IsInUse == false);
        if (go == null)
        {
            AddOne();
            go = pool[pool.Count - 1];
            if (debug) { GD.Print($"{Core.WHO}ProjectilePool::GetObject() {prefab.Name} extending. Size {pool.Count}"); }
        }
        go.IsInUse = true;
        go.GetParent<Node3D>().Scale = Vector3.One;
        return go.GetParent() as IProjectile;
    }

    private void AddOne()
    {
        Node3D go = prefab.Duplicate() as Node3D;
        (go as IProjectile).SetDamagePackage((prefab as IProjectile).Damage);
        go.Name = $"{prefab.Name} ({pool.Count})";
        PoolableObjectBase poolable = go.GetNode<PoolableObjectBase>("PoolableProjectile");
        poolable.PoolIndex = poolIndex;
        pool.Add(poolable as PoolableProjectile);
    }

    internal void ReturnObject(PoolableProjectile go)
    {
        //GD.Print($"{Core.WHO}ProjectilePool::ReturnObject() returning[{go.GetParent().Name}]");
        Node3D n = go.GetParent<Node3D>();
        n.GetParent().RemoveChild(n);
        n.RequestReady();
        go.ResetObject();
    }

}// EOF CLASS