using Godot;
using System;

namespace Munglo.Commons
{
    /// <summary>
    /// Anything that should be able to be damaged or triggered by 
    /// being hurt(shot or whatever) should have this behaviour on it or 
    /// more commonly inherit from this class
    /// </summary>
    public interface ICanTakeDamage
    {
        /// <summary>
        /// Exposed event in inspector to be able to hook up triggers and such
        /// </summary>
        //public EventHandler<DamagePackage> OnTookDamage;
        
        /// <summary>
        /// Delivers damage as fully intended. Returns True if it was a kill.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="isLocalDamage"></param>
        /// <returns></returns>
        public abstract bool TakeDamage(DamagePackage package, bool isLocalDamage = true, bool skipTookDamageEvent = false);
        
        /// <summary>
        /// This should deliver full damage regardless of any damage modifiers. Returns True if it was a kill.
        /// </summary>
        /// <param name="package"></param>
        public abstract bool TakeDirectDamage(DamagePackage package, bool isLocalDamage = true, bool skipTookDamageEvent = false);
        
        
        /// <summary>
        /// Recieve redirected damage from other parts, hitboxes and such. Returns True if it was a kill.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="isDirect"></param>
        /// <returns></returns>
        public bool TakeRedirectedDamage(DamagePackage package, bool isDirect = false)
        {
            if (isDirect) { return TakeDirectDamage(package, false); }
            return TakeDamage(package, false);
        }
    }//EOF CLASS
}