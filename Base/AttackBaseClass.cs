using Munglo.Commons;
using Godot;
using System;

namespace Munglo.AI.Base
{
    /// <summary>
    /// All Attacks need to inherit from this and implement the IAttack AND IAction Interfaces.
    /// </summary>
    [GlobalClass]
    public partial class AttackBaseClass : ActionBaseClass
    {
        //set this true to be able to attack/reload while moving
        [Export] private bool canMove = false;
        //The range of the attack. Remember even melee attacks have some range. Also take into account the radius of the unit's own collider
        [Export] private float range = 10.0f;
        //The telegraph time window before the attack in ms
        [Export] private ulong timeWindup = 2000;
        //The duration of the attack in ms
        [Export] private ulong timeExecusion = 2000;
        //Time window in ms after attack before anything else can happen
        [Export] private ulong timeWinddown = 2000;

        #region private fields
        private ulong timestamp;
        private TargetParameters target;
        #endregion

        #region Properties
        public bool CanMove { get => canMove; }
        public virtual bool CheckIfInPosition { get => false; }
        public float Range { get => range; }

        public TargetParameters Target { get => target; set => target = value; }
        #endregion

        public AISTATE Continue(AISTATE aiState, NavigationAgent3D navAgent)
        {
            if (Unit.CurrentAction is null) { Unit.FightState = ATTACKSTATE.INACTIVE; return AISTATE.RESET; }
            switch (Unit.FightState)
            {
                case ATTACKSTATE.INACTIVE:
                    return AISTATE.RESET;
                case ATTACKSTATE.GENERAL:
                    Unit.FightState = AttackGeneral();
                    break;
                case ATTACKSTATE.SELECTINGTARGET:
                    Unit.FightState = AttackSelectingTarget();
                    break;
                case ATTACKSTATE.ATTACKSETUP:
                    Unit.FightState = AttackSetup();
                    break;
                case ATTACKSTATE.ATTACKWINDUP:
                    Unit.FightState = AttackWindup();
                    break;
                case ATTACKSTATE.ATTACKEXECUSION:
                    Unit.FightState = AttackExecusion();
                    break;
                case ATTACKSTATE.ATTACKWINDDOWN:
                    Unit.FightState = AttackWindDown();
                    break;
                case ATTACKSTATE.ATTACKMOVE:
                    Unit.FightState = AttackMove(navAgent);
                    break;
                case ATTACKSTATE.SPECIALMOVE:
                    break;
                case ATTACKSTATE.SPECIALACTION:
                    break;
                default:
                    break;
            }
            return AISTATE.ATTACKING;
        }
        public void AttackInitialize(AIObject unit, EvaluateDelegate eDel)
        {
            ActionInitialize(unit, eDel);
            Unit.Awareness.OnVisibleChangedVisible += (object sender, int arg) => { OnVisibleChanged(arg, true); };
            Unit.Awareness.OnVisibleChangedHidden += (object sender, int arg) => { OnVisibleChanged(arg, false); }; 
            Unit.Awareness.OnForgotten += OnForgotten;
            target = new TargetParameters();
        }


        public virtual void OnVisibleChanged(int id, bool visible)
        {
            if(target != null && target.IsValid && target.AIObjectID == id)
            {
                target.isInView = visible;
            }

            if (!Unit.Awareness.IsTarget(id)) { return; }

            if (visible)
            {
                // Visible
                Unit.Mind.ChangeIfHigherPrio(PriorityNoStack);
                return;
            }
        }
        #region Phase Methods
        public virtual ATTACKSTATE AttackGeneral()
        {
            return ATTACKSTATE.SELECTINGTARGET;
        }
        public virtual ATTACKSTATE AttackSelectingTarget()
        {
            target = Unit.Awareness.GetClosestTarget(true);
            if (!target.IsValid)
            {
                return ATTACKSTATE.INACTIVE;
            }
            return ATTACKSTATE.ATTACKSETUP;
        }
        public virtual ATTACKSTATE AttackSetup()
        {
            //target = Unit.Awareness.GetAsTarget(target.AIObjectID);

            Unit.Movement.TurnTowardsPoint(target.location);

            if (target.distance > Range)
            {
                return ATTACKSTATE.ATTACKMOVE;
            }
            timestamp = Time.GetTicksMsec();
            return ATTACKSTATE.ATTACKWINDUP;
        }
        public virtual ATTACKSTATE AttackWindup()
        {
            if(Time.GetTicksMsec() < timestamp + timeWindup) { return ATTACKSTATE.ATTACKWINDUP; }
            //Unit.Weapon.AimAtPoint(target.location + Vector3.Up * 0.5f);
            Unit.Weapon.Aim(target.location + Vector3.Up * 0.5f, Unit.Body.Transform.Basis.Y);
            //Unit.Weapon.FirePrimary(Unit.aiObjectID);
            Unit.Weapon.Attack();
            timestamp = Time.GetTicksMsec();
            return ATTACKSTATE.ATTACKEXECUSION;
        }
        public virtual ATTACKSTATE AttackExecusion()
        {
            if (Time.GetTicksMsec() < timestamp + timeExecusion) { return ATTACKSTATE.ATTACKEXECUSION; }
            timestamp = Time.GetTicksMsec();
            return ATTACKSTATE.ATTACKWINDDOWN;
        }
        public virtual ATTACKSTATE AttackWindDown()
        {
            if (Time.GetTicksMsec() < timestamp + timeWinddown) { return ATTACKSTATE.ATTACKWINDDOWN; }
            return ATTACKSTATE.INACTIVE;
        }
        public virtual ATTACKSTATE AttackMove(NavigationAgent3D navAgent)
        {
            target = Unit.Awareness.GetAsTarget(target.AIObjectID);

            switch (Unit.MovementState)
            {
                case AIMOVEMENTSTATE.PENDING:
                    break;
                case AIMOVEMENTSTATE.INACTIVE:
                    if (target.distance <= Range)
                    {
                        return ATTACKSTATE.ATTACKSETUP;
                    }
                    Unit.Movement.Move(target.location, IsSelected && debug);
                    break;
                case AIMOVEMENTSTATE.ACTIVE:
                    // Track target location and update path as needed
                    if (navAgent.TargetPosition.DistanceTo(Target.location) > 0.3f)
                    {
                        Unit.Movement.Move(Target.location, IsSelected && debug);
                        return ATTACKSTATE.ATTACKMOVE;
                    }
                    if (Target.distance <= Range)
                    {
                        if (!CanMove) { Unit.Movement.Stop(); }
                        return ATTACKSTATE.ATTACKSETUP;
                    }
                    Unit.Movement.Update(IsSelected && debug);
                    break;
                case AIMOVEMENTSTATE.FINISHED:
                default:
                    if (target.isInView && target.distance <= Range)
                    {
                        return ATTACKSTATE.ATTACKSETUP;
                    }
                    target = new TargetParameters();
                    return ATTACKSTATE.GENERAL;
            }
            return Unit.FightState;
        }
        #endregion

        public virtual void ForceStop()
        {
            Unit.Movement.Stop();
            Unit.FightState = ATTACKSTATE.INACTIVE;
            Target = new TargetParameters();
        }
        public virtual void Interupt(AISTATE aiState, NavigationAgent3D navAgent)
        {
            throw new System.NotImplementedException();
        }
        public virtual bool Resume()
        {
            throw new System.NotImplementedException();
        }
        
     
        public virtual void OnForgotten(object sender, int aIObjectID)
        {
            if(Target != null && target.AIObjectID == aIObjectID)
            {
                ForceStop();
            }
        }
        public virtual bool IsAttackPossible()
        {
            if (!Unit.Awareness.GetClosestTarget(true).IsValid)
            {
                message = "There is no visible targets.";
                return false;
            }
            return true;
        }
        public virtual bool IsInPosition()
        {
            if(Target == null) { return false; }
            return !(Target.distance > Range);
        }
    }// EOF CLASS
}
