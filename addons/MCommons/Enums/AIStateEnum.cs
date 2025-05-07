namespace Munglo.Commons
{
    /*  INACTIVE: No AI updates running at all
     *  ATTACKING: When AI can and will attack, makes use of a Subset AI resolver that has internal states.
     *  PERFORMINGACTION: Doing things.
     *  RESET: Set This to reset AI so it will make a new decision. 
     */
    public enum AISTATE { INACTIVE, ATTACKING, PERFORMINGACTION, RESET }

    /* INACTIVE: Don't do anything. Will cause an AIState.Reset in an active Attack
     * GENERAL: Start Phase of an attack. If Attack fails but needs a mulligan, drop to this.
     * SELECTINGTARGET: Pick a target.
     * ATTACKSETUP: Turn to faace target, move into position, get attack ready and all that , phase.
     * ATTACKWINDUP: Channeling the attack. This is when attack should be telegrafed
     * ATTACKEXECUSION: Running through the attack sequense.
     * ATTACKWINDDOWN: Phase after execusion to make leway for animations and such.
     * ATTACKMOVE: When setup phase wants to reposition it dips into this phase.
     * SPECIALMOVE: Dodging, sneaking, stalking, jumping and such happens here
     * SPECIALACTION: Spells, Shouts, buffs whatever that isn't an actual attack
     */
    public enum ATTACKSTATE { 
        INACTIVE, 
        GENERAL, 
        SELECTINGTARGET, 
        ATTACKSETUP, 
        ATTACKWINDUP, 
        ATTACKEXECUSION, 
        ATTACKWINDDOWN, 
        ATTACKMOVE, 
        SPECIALMOVE, 
        SPECIALACTION 
    }

}