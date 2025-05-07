using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Munglo.AI.Base
{
    [System.Serializable]
    public enum AIGROUPSPEC { UNSPECIFIED, PLAYERENEMY, PLAYERALLY, NEUTRAL }

    /* PENDING: Waiting for path to be picked and resolved
     * ACTIVE: Currently moving
     * FINISHED: Have arrived at the end of current movement
     */
    public enum AIMOVEMENTSTATE { INACTIVE, PENDING, ACTIVE, FINISHED }

    public enum MOVEPARAMSPEED { WALK, RUN, SPRINT }

    public enum UNITANIMSTATE { UNSET = 0, IDLE = 1, MOVING = 2, RAGDOLL = 3, JUMPING = 4, FLYING = 5 ,DEAD = 6}
}
