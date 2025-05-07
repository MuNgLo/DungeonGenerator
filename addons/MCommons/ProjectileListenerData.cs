namespace Munglo.Commons;
public class ProjectileListenerData : IProjectileListenerData
{
    public int count = 0;
    public int remaining = 0;
    public virtual int Count => count;
    public virtual int Remaining => remaining;
}// EOF CLASS