using System;
using System.Collections.Generic;
using Munglo.Commons;
using Godot;
using Munglo.AI.Base;
using System.Linq;

namespace Munglo.AI.Actions
{
    [GlobalClass]
    public partial class CollectThings : ActionBaseClass, IAction
    {
        [Export] private float maxDistance = 20.0f; // At this distance and further, all things lose appeal.
        [Export] private int maxDistancePenalty = 50;
        [Export] private float reach = .5f; // How close we hvae to be to pick an things up
        [Export] private int targetThingID = -1;
        [Export] private Vector3 targetLocation = Vector3.Zero;


        private List<int> visibleThings;
        private List<int> notVisibleThings;
        private Dictionary<string, string> typesOfThings;
        public override bool CheckIfPossible { get => KnowAboutThings(); }

        public void Initialize(AIObject unit)
        {
            //GD.Print(typeof(Apple).Name);
            //GD.Print(typeof(Apple).FullName);
            ActionInitialize(unit, Evaluate);
            visibleThings = new List<int>();
            notVisibleThings = new List<int>();
            typesOfThings = new Dictionary<string, string>();
            Unit.Awareness.OnVisibleChangedHidden += OnVisibleChangedHidden;
            Unit.Awareness.OnVisibleChangedVisible += OnVisibleChangedVisible;
            Unit.Awareness.OnForgotten += OnForgotten;
        }
        public override void RegisterType(string typeName, string source)
        {
            if (debug) { Log($"CollectThings::RegisterType() on {Unit.Name} for Type({typeName}) from {source}"); }
            typesOfThings[typeName] = source;
            if (!typesOfThings.Keys.Contains(typeName))
            {
                Log($"CollectThings::RegisterType() Failed");
            }
        }
        public int Evaluate(bool buildStack)
        {
            message = $"DistancePenalty {DistancePenalty()}";
            PutEvaluationOnDebugStack(buildStack);
            return Mathf.Clamp(PriorityWithModifiers() - DistancePenalty(), 0, maxPriority);
        }
        private int DistancePenalty(float distance, float distance2)
        {
            return DistancePenalty(Mathf.Min(distance, distance2));
        }
        private int DistancePenalty(float distance)
        {
            return Mathf.FloorToInt((distance / maxDistance) * maxDistancePenalty);
        }
        private int DistancePenalty()
        {
            if (KnowAboutThings())
            {
                Unit.Awareness.GetDistanceToClosest(visibleThings, out float distance);
                Unit.Awareness.GetDistanceToClosest(notVisibleThings, out float distance2);
                return DistancePenalty(distance, distance2);
            }
            return 0;
        }
        public AISTATE Begin(AISTATE aiState, NavigationAgent3D navAgent)
        {
            if (!KnowAboutThings()) { return AISTATE.RESET; }
            // First execution of action runs this once
            if (Unit.Awareness.GetClosest(visibleThings, out ViewObject thvisableTHinging))
            {
                targetThingID = thvisableTHinging.aIObjectID;
                targetLocation = thvisableTHinging.Position;
            }
            else if (Unit.Awareness.GetClosest(notVisibleThings, out ViewObject hiddenTHing))
            {
                targetThingID = hiddenTHing.aIObjectID;
                targetLocation = hiddenTHing.Position;
            }
            //if (debug) { Log($"CollectThings::Begin() {Unit.Body.Name} started collecting a thing[{targetThingID}]"); }
            return AISTATE.PERFORMINGACTION;
        }

        public AISTATE Continue(AISTATE aiState, NavigationAgent3D navAgent)
        {
            if (!KnowAboutThings()) { return AISTATE.RESET; }
            if (Unit.Awareness.GetClosest(visibleThings, out ViewObject thing))
            {
                if (targetThingID != thing.aIObjectID)
                {
                    Unit.Movement.Stop();
                    return AISTATE.RESET;
                }
            }
            // Check that target ID is valid. If not reset.
            if (targetThingID < 0)
            {
                Unit.Movement.Stop();
                return AISTATE.RESET;
            }
            // Continue action
            switch (Unit.MovementState)
            {
                case AIMOVEMENTSTATE.FINISHED:
                    if (Unit.GlobalPosition.DistanceTo(targetLocation) <= reach)
                    {
                        PickupThing();
                        Unit.Movement.Stop();
                        return AISTATE.RESET;
                    }
                    Unit.Movement.Stop();
                    return AISTATE.RESET;
                case AIMOVEMENTSTATE.PENDING:
                case AIMOVEMENTSTATE.ACTIVE:
                    if (Unit.Awareness.GetObjectLocation(targetThingID).DistanceTo(targetLocation) > reach)
                    {
                        //if (debug) { Log($"CollectThings::Continue() Target thing[{targetThingID}] has moved. Drift({Unit.Awareness.GetObjectLocation(targetThingID).DistanceTo(targetLocation)}) reach({reach} destination({navAgent.TargetPosition}))"); }
                        Unit.Movement.Move(Unit.Awareness.GetObjectLocation(targetThingID), 1, 0, false);
                        Unit.Awareness.ForgetAbout(targetThingID);
                        targetThingID = -1;
                        return AISTATE.RESET;
                    }
                    if ( Unit.GlobalPosition.DistanceTo(targetLocation) <= reach)
                    {
                        PickupThing();
                        Unit.Movement.Stop();
                        return AISTATE.RESET;
                    }
                    break;
                case AIMOVEMENTSTATE.INACTIVE:
                default:
                    Unit.Movement.Move(Unit.Awareness.GetObjectLocation(targetThingID), 1, 0, false);
                    break;
            }
            return AISTATE.PERFORMINGACTION;
        }


        private void PickupThing()
        {
            AIObject thing = AIManager.Matrix.GetAIObject(targetThingID);
            if (thing is not null)
            {
                if (debug) { Log($"CollectThings::PickupThing({targetThingID}) Pick the Thing! {thing.Name}"); }
                thing.DeRegister(false);
                Unit.Inventory.AddItem(thing);
            }
            Unit.Awareness.ForgetAbout(targetThingID);
            targetThingID = -1;
        }

        public void ForceStop()
        {
            Unit.Movement.Stop();
            targetThingID = -1;
            targetLocation = Vector3.Zero;
        }

        public void Interupt(AISTATE aiState, NavigationAgent3D navAgent)
        {
            Unit.Movement.Stop();
            targetThingID = -1;
            targetLocation = Vector3.Zero;
        }

        private bool KnowAboutThings()
        {
            if (visibleThings.Count + notVisibleThings.Count > 0)
            {
                message = string.Empty;
                return true;
            }
            message = $"Dont know about any collectibles. ({typesOfThings.Count}) { String.Join(',', typesOfThings.Keys) }.";
            return false;
        }

        public bool Resume()
        {
            return false;
        }

        #region Keep Track of things spotted
        private void OnForgotten(object sender, int aIObjectID)
        {
            if(targetThingID == aIObjectID)
            {
                if (debug) { 
                    GD.Print($"CollectThings::OnForgotten() {aIObjectID} wven when tracking it"); targetThingID = -1;
                }
            }
            notVisibleThings.Remove(aIObjectID);
            visibleThings.Remove(aIObjectID);
        }
        private void OnVisibleChangedVisible(object sender, int aIObjectID)
        {
            ViewObject vObject = Unit.Awareness.GetKnownObject(aIObjectID);
            foreach (string key in typesOfThings.Keys)
            {
                if (AIManager.Matrix.GetAIObject(vObject.aIObjectID).GetType().Name == key)
                {
                    MoveAddToVisible(aIObjectID);
                }
            }
        }
        private void OnVisibleChangedHidden(object sender, int aIObjectID)
        {

            AIObject vObject = AIManager.Matrix.GetAIObject(aIObjectID);
            if (vObject != null)
            {
                foreach (string key in typesOfThings.Keys)
                {
                    if (vObject.GetType().Name == key)
                    {
                        MoveAddToNotVisible(aIObjectID);
                    }
                }
            }
        }
        private void MoveAddToVisible(int aIObjectID)
        {
            //if (debug) { GD.Print($"CollectThings::MoveAddToVisible() {aIObjectID}"); }

            if (!visibleThings.Exists(p => p == aIObjectID)) { visibleThings.Add(aIObjectID); }
            notVisibleThings.Remove(aIObjectID);
            if (Unit.Awareness.GetClosest(visibleThings, out ViewObject closeThing))
            {
                if (targetThingID != closeThing.aIObjectID)
                {
                    if (Unit.Awareness.GetDistanceToClosest(visibleThings, out float distance))
                    {
                        Unit.Mind.ChangeIfHigherPrio(Priority);
                    }
                }
            }
        }
        private void MoveAddToNotVisible(int aIObjectID)
        {
            //if (debug) { GD.Print($"CollectThings::MoveAddToNotVisible() {aIObjectID}"); }
            if (!notVisibleThings.Exists(p => p == aIObjectID)) { notVisibleThings.Add(aIObjectID); }
            visibleThings.Remove(aIObjectID);
        }
        #endregion
    }// EOF CLASS
}