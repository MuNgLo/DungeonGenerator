using Godot;
using Munglo.AI.Base;
using Munglo.Commons;

namespace Munglo.AI.Actions
{
    /// <summary>
    /// This sets a lower floor of priority. Anything lower than this will basically be ignored.
    /// Or rather that is more or less the intended use of this Action. Your usecase might differ.
    /// </summary>
    [GlobalClass]
    public partial class IdleAction : ActionBaseClass, IAction
    {
        [Export] private int minIdleTime = 5;
        [Export] private int maxIdleTime = 30;
        private float endOfIdle = 0.0f;
        public override bool CheckIfPossible { get => true; } // this needs to be overriden, check the IAction interface and baseclass

        public int Evaluate(bool buildStack = false)
        {
            message = string.Empty;
            PutEvaluationOnDebugStack(buildStack); // uses default evaluation method and puts info on debug stack if buildStack is True
            return Mathf.Clamp(PriorityWithModifiers(), minPriority, maxPriority); 
        }
        public AISTATE Begin(AISTATE aiState, NavigationAgent3D navAgent)
        {
            float time = (int)(Time.GetTicksMsec() * 0.001f);
            endOfIdle = time + GD.RandRange(minIdleTime, maxIdleTime);
            if (IsSelected && debug) { Log($"{Unit.Name} started idling for ({time - endOfIdle})"); }
            return AISTATE.PERFORMINGACTION;
        }
        public AISTATE Continue(AISTATE aiState, NavigationAgent3D navAgent)
        {
            if((int)(Time.GetTicksMsec() * 0.001f) > endOfIdle) { return AISTATE.RESET; }
            return AISTATE.PERFORMINGACTION;
        }
        public void ForceStop()
        {
            endOfIdle = (int)(Time.GetTicksMsec() * 0.001f) - maxIdleTime;
            Unit.Movement.Stop();
        }
        public void Initialize(AIObject mind)
        {
            ActionInitialize(mind, Evaluate);
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
    }// EOF CLASS
}
