using Munglo.Commons;
using Munglo.AI.Base;
using System;
using System.Collections.Generic;
using Godot;

namespace Munglo.AI.Actions
{
    /// <summary>
    ///  Move away from thing we see of Type
    /// </summary>
    [GlobalClass]
    public partial class AvoidAction : ActionBaseClass, IAction
    {
        private List<int> potentialThreats;
        private int currentThreat;
        private float triggerDistance = 5.0f;
        private float fleeDistance = 20.0f;
        private Dictionary<string, string> types;
        public override bool CheckIfPossible { get => PossibilityCheck(); }
        public int Evaluate(bool skipStack = false)
        {
            PutEvaluationOnDebugStack(skipStack);
            currentThreat = potentialThreats[0];
            if (debug && Unit.IsSelected) {
                Log($"AvoidAction::Evaluate() {Unit.Name} FinalBasePriority({FinalBasePriority()}) Priority({Mathf.Clamp(PriorityWithModifiers() + FinalBasePriority(), 0, maxPriority)})");
            }
            return Mathf.Clamp(PriorityWithModifiers(), 0, maxPriority);
        }
        private bool PossibilityCheck()
        {
            if (potentialThreats.Count < 1)
            {
                message = "Don't see anything worth avoiding.";
                return false;
            }
            return true;
        }
        private int FinalBasePriority()
        {
            return Mathf.FloorToInt((maxPriority - minPriority) * Unit.Awareness.GetDistanceToKnownObject(currentThreat) / (fleeDistance - triggerDistance));
        }
        public AISTATE Begin(AISTATE aiState, NavigationAgent3D navAgent)
        {
            if (debug && Unit.IsSelected) { Log($"{Unit.Name} started AvoidAction"); }
            message = $"Avoiding {currentThreat}";
            Unit.Movement.MoveAwayFrom(Unit.Awareness.GetObjectLocation(currentThreat), fleeDistance, 2, 0);
            return AISTATE.PERFORMINGACTION;
        }
        public AISTATE Continue(AISTATE aiState, NavigationAgent3D navAgent)
        {
            switch (Unit.MovementState)
            {
                case AIMOVEMENTSTATE.PENDING:
                case AIMOVEMENTSTATE.ACTIVE:
                    if(currentThreat < 0) { return AISTATE.RESET; }
                    if (navAgent.TargetPosition.DistanceTo(Unit.Awareness.GetObjectLocation(currentThreat)) < triggerDistance)
                    {
                        Unit.Movement.MoveAwayFrom(Unit.Awareness.GetObjectLocation(currentThreat), fleeDistance, debug);
                        return AISTATE.PERFORMINGACTION;
                    }
                    Unit.Movement.Update(debug);
                    return AISTATE.PERFORMINGACTION;
                case AIMOVEMENTSTATE.FINISHED:
                case AIMOVEMENTSTATE.INACTIVE:
                default:
                    return AISTATE.RESET;
            }
        }
        private void OnVisibleChangedHidden(object sender, int e)
        {
            OnVisibleChanged(e, false);
        }

        private void OnVisibleChangedVisible(object sender, int e)
        {
            OnVisibleChanged(e, true);
        }
        private void OnVisibleChanged(int objectID, bool flag)
        {
            if (flag)
            {
                if (!potentialThreats.Exists(p => p == objectID))
                {
                    if (AIManager.Matrix.IsIDinMatrix(objectID))
                    {
                        AIObject tarGO = AIManager.Matrix.GetAIObject(objectID);
                        if (tarGO is not null)
                        {
                            foreach (string value in types.Values)
                            {
                                if (value == "Hostiles")
                                {
                                    if(AIManager.Factions.GetFactionRelationship(Unit.factionID, tarGO.factionID) == FACTIONRELATIONSTATE.HOSTILE)
                                    {
                                        if (debug && Unit.IsSelected) { Log($"AvoidAction::OnVisibleChanged() Personality!"); }
                                        potentialThreats.Add(objectID);
                                        Unit.Mind.ChangeIfHigherPrio(Evaluate(true));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                potentialThreats.RemoveAll(p => p == objectID);
                if (currentThreat == objectID)
                {
                    currentThreat = -1;
                }
            }
        }
        public override void RegisterType(string typeName, string source)
        {
            if (debug) { Log($"AvoidAction::RegisterType() on {Unit.Name} for Type({typeName}) from {source}"); }
            types[source] = typeName;
        }
        public void ForceStop(){}
        public void Initialize(AIObject unit)
        {
            types = new Dictionary<string, string>();
            potentialThreats = new List<int> { };
            (unit as AIUnit).Awareness.OnVisibleChangedVisible += OnVisibleChangedVisible;
            (unit as AIUnit).Awareness.OnVisibleChangedHidden += OnVisibleChangedHidden;
            (unit as AIUnit).Awareness.OnForgotten += OnForgotten;
            ActionInitialize(unit, Evaluate);
        }

        

        private void OnForgotten(Object sender, int objectID)
        {
            potentialThreats.RemoveAll(p => p == objectID); 
            if(currentThreat == objectID)
            {
                currentThreat = -1; ;
            }
        }
        public bool Resume()
        {
            return false;
        }
        public void Interupt(AISTATE aiState, NavigationAgent3D navAgent){}
    }// EOF CLASS
}
