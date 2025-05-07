using System;
using Godot;

namespace Munglo.Commons;
/// <summary>
/// Anything that should be able to be damaged or triggered by 
/// being hurt(shot or whatever) should have this behaviour on it or 
/// more commonly inherit from this class
/// </summary>
public partial class CanTakeDamage : Node3D
{
    /// <summary>
    /// Exposed event in inspector to be able to hook up triggers and such
    /// </summary>
    public EventHandler<DamagePackage> OnTookDamage;

    public virtual void Attach(Node3D tr, Node3D hitTR)
    {

    }


    /// <summary>
    /// Should deliver damage as fully intended. Returns True if it was a kill.
    /// default implementation do nothing but return false
    /// </summary>
    /// <param name="package"></param>
    /// <param name="isLocalDamage"></param>
    /// <returns></returns>
    public virtual bool TakeDamage(DamagePackage package, bool isLocalDamage = true, bool skipTookDamageEvent = false)
    {
        return false;
    }

    /// <summary>
    /// This should deliver full damage regardless of any damage modifiers. Returns True if it was a kill.
    /// default implementation do nothing but return false
    /// </summary>
    /// <param name="package"></param>
    public virtual bool TakeDirectDamage(DamagePackage package, bool isLocalDamage = true, bool skipTookDamageEvent = false)
    {
        return false;
    }


    /// <summary>
    /// Recieve redirected damage from other parts, hitboxes and such. Returns True if it was a kill.
    /// </summary>
    /// <param name="package"></param>
    /// <param name="isDirect"></param>
    /// <returns></returns>
    public virtual bool TakeRedirectedDamage(DamagePackage package, bool isDirect = false)
    {
        if (isDirect) { return TakeDirectDamage(package, false); }
        return TakeDamage(package, false);
    }
}//EOF CLASS
