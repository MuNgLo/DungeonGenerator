using Godot;
using Munglo.AI.Base;
using Munglo.Commons;

namespace Munglo.AI.Actions
{
    /// <summary>
    /// Randomly move across the navmesh
    /// </summary>
    [GlobalClass]
    public partial class RoamAction : ActionBaseClass, IAction
    {
        public override bool CheckIfPossible { get => true; }
        public int Evaluate(bool buildStack)
        {
            PutEvaluationOnDebugStack(buildStack);
            return Mathf.Clamp(PriorityWithModifiers(), 0, maxPriority);
        }
        public AISTATE Begin(AISTATE aiState, NavigationAgent3D navAgent)
        {
            if (debug) { Log($"{Unit.Name} started roaming"); }
            message = string.Empty;


            Unit.Movement.MoveToRandomSpot(debug);
            return AISTATE.PERFORMINGACTION;
        }
        public AISTATE Continue(AISTATE aiState, NavigationAgent3D navAgent)
        {
            switch (Unit.MovementState)
            {
                case AIMOVEMENTSTATE.PENDING:
                case AIMOVEMENTSTATE.ACTIVE:
                    Unit.Movement.Update(debug);
                    return AISTATE.PERFORMINGACTION;
                case AIMOVEMENTSTATE.FINISHED:
                    Unit.Movement.Stop();
                    return AISTATE.RESET;
                case AIMOVEMENTSTATE.INACTIVE:
                default:
                    return AISTATE.RESET;
            }
        }
        public void ForceStop()
        {
            Unit.Movement.Stop();
        }
        public override void RegisterType(string typeName, string source)
        {
            throw new System.NotImplementedException();
        }
        public void Interupt(AISTATE aiState, NavigationAgent3D navAgent)
        {
            Unit.Movement.Stop();
        }
        public bool Resume()
        {
            return false;
        }
        public void Initialize(AIObject mind)
        {
            ActionInitialize(mind, Evaluate);
        }
    }// EOF CLASS
}