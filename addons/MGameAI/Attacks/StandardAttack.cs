using Munglo.Commons;
using Munglo.AI.Base;
using Godot;
using System;

namespace Munglo.AI.Attacks
{
    [GlobalClass]
    public partial class StandardAttack : AttackBaseClass, IAttack, IAction
    {
        public override bool CheckIfPossible => IsAttackPossible();
        public override bool CheckIfInPosition { get => IsInPosition(); }

        [Export] private string animWindup = string.Empty;
        [Export] private string animExecusion = string.Empty;
        [Export] private string animWinddown = string.Empty;

        public int Evaluate(bool buildStack = false)
        {
            message = string.Empty;
            PutEvaluationOnDebugStack(buildStack); // uses default evaluation method and puts info on debug stack if buildStack is True
            return Mathf.Clamp(PriorityWithModifiers(), minPriority, maxPriority);
        }
        public AISTATE Begin(AISTATE aiState, NavigationAgent3D navAgent)
        {
            Unit.FightState = ATTACKSTATE.GENERAL;
            return AISTATE.ATTACKING;
        }
        public void Initialize(AIObject unit)
        {
            AttackInitialize(unit, Evaluate);
            (unit as AIUnit).States.OnAttackStateChangedEvent += WhenAttackStateChanged;
        }

        private void WhenAttackStateChanged(object sender, ATTACKSTATE e)
        {
            if (Unit.CurrentAction is null || Unit.CurrentAction.GetInstanceId() != GetInstanceId()) { return; }
            switch (e)
            {
                case ATTACKSTATE.ATTACKWINDUP:
                    Unit.Weapon.AnimPlayWindup(animWindup);
                    break;
                case ATTACKSTATE.ATTACKEXECUSION:
                    Unit.Weapon.AnimPlayExecusion(animExecusion);
                    break;
                case ATTACKSTATE.ATTACKWINDDOWN:
                    Unit.Weapon.AnimPlayWinddown(animWinddown);
                    break;
                case ATTACKSTATE.INACTIVE:
                case ATTACKSTATE.GENERAL:
                case ATTACKSTATE.SELECTINGTARGET:
                case ATTACKSTATE.ATTACKSETUP:
                case ATTACKSTATE.ATTACKMOVE:
                case ATTACKSTATE.SPECIALMOVE:
                case ATTACKSTATE.SPECIALACTION:
                default:
                    Unit.Weapon.AnimPlayWinddown("Default");
                    break;
            }
        }
    }// EOF CLASS
}
