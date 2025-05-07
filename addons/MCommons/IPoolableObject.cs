namespace Munglo.Commons;

public interface IPoolableObject
{
    public int PoolIndex { get; set; }
    public bool IsInUse { get; set; }
    /// <summary>
    /// ResetObject should be called from the Pool when the poolable object is returned to it
    /// </summary>
    public void ResetObject();
    public void ReturnToPool();
    public void SetupReferences();
}// EOF INTERFACE