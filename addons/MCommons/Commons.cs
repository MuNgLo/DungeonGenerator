using Godot;

namespace Munglo.Commons
{
    [System.Serializable]
    public enum AITYPE { GENERIC, MIND, GUARDAREA, CAPTUREAREA }
    [System.Serializable]
    public enum UISTATE { UNSET, HIDDEN, VISIBLE }
    //[System.Serializable]
    //public enum PROJECTILEEVENT { SPAWN, FIRE, INIT, TRAVELUPDATE, IMPACT, DESPAWN}
    // Used to keep track of the state a weapon is in
    // UNSET :: Weapon unusable. not setup yet
    // UNLOADED :: Weapon usable but empty
    // FIRING :: Currently firing
    // RELOADING :: reloading mag (normally we just use cooldown. this is to track when to run reload animation)
    // COOLDOWN :: After firing we do this before ready to fire again or reload
    // LOADED :: This is when we are ready to fire
    // NOAMMO :: Weapon is both unloaded and no ammo to load it with
    public enum WEAPONSTATE { UNSET, UNLOADED, FIRING, RELOADING, COOLDOWN, LOADED, NOAMMO }


    [System.Serializable]
    public struct DamageInitialSetup
    {
        public DSOURCE source;
        public int aiObjectId;
        public float weaponMultiplier;
        public string dealer;
    }
}