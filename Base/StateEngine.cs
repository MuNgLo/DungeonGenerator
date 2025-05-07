using System;
using Munglo.Commons;

namespace Munglo.AI.Base
{
    [Serializable]
    public class StateEngine
    {
        private AISTATE state = AISTATE.INACTIVE;
        private ATTACKSTATE fstate = ATTACKSTATE.INACTIVE;
        private AIMOVEMENTSTATE movementState = AIMOVEMENTSTATE.INACTIVE;
        internal AISTATE State { get => state; set => state = value; }
        internal ATTACKSTATE FState { get => fstate; set => ChangeAttackState(value); }
        public AIMOVEMENTSTATE MovementState { get => movementState; set => movementState = value; }
        public EventHandler<ATTACKSTATE> OnAttackStateChangedEvent;
         
        private void ChangeAttackState(ATTACKSTATE newState)
        {
            if(fstate == newState) return;
            fstate= newState;
            EventHandler<ATTACKSTATE> raiseEvent = OnAttackStateChangedEvent;
            if (raiseEvent != null)
            {
                raiseEvent(this, fstate);
            }
        }
    }//EOF CLASS
}
