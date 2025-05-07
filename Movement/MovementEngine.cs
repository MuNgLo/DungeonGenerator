using Godot;
using Munglo.Commons;
using Munglo.AI.Base;
using Godot.Collections;
using System;

namespace Munglo.AI.Movement
{
    /// <summary>
    /// This class is part of baseaction class and is how actions do movment
    /// </summary>
    [System.Serializable]
    public class MovementEngine
    {
        private AIUnit unit;
        private NavigationAgent3D agent;
        private IMovement mSettings;
        public float TurnRate { get => 40.0f; }
        private Vector3 frameVelocity;
        public MovementEngine(AIUnit Unit, NavigationAgent3D navAgent, IMovement moveSettings)
        {
            unit = Unit;
            agent = navAgent;
            mSettings = moveSettings;
            agent.NavigationFinished += () => { unit.MovementState = AIMOVEMENTSTATE.FINISHED; };
            agent.VelocityComputed += OnVelocityComputed;
        }
        /// <summary>
        /// This moves the Unit ANy and all movement goes through here and as iut completes it just goes back to 
        /// state case MOVEMENTSTATE.FINISHED
        /// If next update dont have new movement it should fall into case MOVEMENTSTATE.INACTIVE
        /// </summary>
        /// <param name="delta"></param>
        internal void PhysTickUpdate(double delta)
        {
            if (unit.MovementState == AIMOVEMENTSTATE.ACTIVE)
            {
                Vector3 dir = unit.Body.GlobalPosition.DirectionTo(agent.GetNextPathPosition());
                FaceDirection(dir);
                frameVelocity = dir * mSettings.GroundSpeed;
                // Resolve how to move and implement the move
                if (agent.AvoidanceEnabled)
                {
                    agent.SetVelocityForced(frameVelocity);
                }
                else
                {
                    OnVelocityComputed(frameVelocity);
                }
                unit.Body.GlobalPosition = unit.Body.GlobalPosition + frameVelocity * (float)delta;
            }// EOF ACTIVE 
        }
        /// <summary>
        /// When an action is in control of a movment. It's Continue() will call this Update
        /// If the action wants to stop before NavAgent arrives. Just call Stop()
        /// </summary>
        /// <param name="debug"></param>
        public void Update(bool debug)
        {
            switch (unit.MovementState)
            {
                case AIMOVEMENTSTATE.ACTIVE:
                    break;
                case AIMOVEMENTSTATE.FINISHED:
                    //Arrive(true);
                    break;
                case AIMOVEMENTSTATE.INACTIVE:
                case AIMOVEMENTSTATE.PENDING:
                default:
                    //Debug.LogWarning("AISystem update loop for state moving failed to do anything which means this AI is stuck in a loop.");
                    break;
            }
        }
        /// <summary>
        /// Rotates Unit towards point in worldspace projected to unit level. clamped my deltad maxdegree
        /// </summary>
        /// <param name="point"></param>
        /// <param name="maxTurnDegree"></param>
        /// <returns></returns>
        public bool TurnTowardsPoint(Vector3 point, float maxTurnDegreeDelta= 360.0f) // TODO VERIFY THIS CODE AS WORKING!
        {
            Vector3 dir = unit.Body.GlobalPosition.DirectionTo(point);
            return FaceDirection(dir, maxTurnDegreeDelta);
        }
        /// <summary>
        /// Stops navagent and sets movementstate to inactive
        /// Make sure an action that has a movement running calls this when it wants to stop the movment
        /// Results in Movment state INACTIVE
        /// </summary>
        public void Stop()
        {
            unit.MovementState = AIMOVEMENTSTATE.INACTIVE;
        }
    

        public bool FaceDirection(Vector3 direction, float maxTurnDegreeDelta=360.0f) // TODO VERIFY THIS CODE AS WORKING!
        {
            direction.Y = 0.0f;
            float angle = direction.AngleTo(-unit.Body.GlobalTransform.Basis.Z);
            angle = Mathf.Min(angle, maxTurnDegreeDelta);
            float dot = unit.Body.GlobalTransform.Basis.X.Dot(direction);
            if(dot > 0.0f) { angle= -angle; }
            unit.Body.Rotate(unit.Body.GlobalTransform.Basis.Y, angle);
            return true;
        }

       

        public void Move(Vector3 targetWorldLocation, bool debug = false)
        {
            Move(targetWorldLocation, 0,0, debug);
        }
        public void Move(Vector3 targetWorldLocation, int gear = 0, int master = 0, bool debug = false)
        {
            if (debug) { GD.Print($"Movement::Move()"); }
            unit.MovementState = GroundMovement.Move(agent, targetWorldLocation, debug);
        }
        public void MoveToRandomSpot(bool debug = false)
        {
            MoveToRandomSpot(0, 0, debug);
        }
        public void MoveToRandomSpot(int gear=0, int master=0, bool debug = false)
        {
            //if (debug) { Debug.Log($"Movement::MoveToRandomSpot()"); }
            unit.MovementState = GroundMovement.RandomMove(unit.Body, agent, mSettings.MaxRandomMoveDistance, debug);
        }
        public void MoveAwayFrom(Vector3 targetWorldLocation, float targetDistance, bool debug = false)
        {
            MoveAwayFrom(targetWorldLocation, targetDistance, 0, 0, debug);
        }
        public void MoveAwayFrom(Vector3 targetWorldLocation, float targetDistance, int gear = 0, int master = 0, bool debug = false)
        {
            Vector3 direction = (unit.GlobalPosition - targetWorldLocation).Normalized();
            Vector3 spot = unit.GlobalPosition + direction * targetDistance;
            spot = Navigation.RandomCirclePointInRange(spot, targetDistance * 0.25f);
            Move(spot, gear, master, debug);
        }
        private void ExecuteAirMove(bool directional, bool debug = false)
        {
            if (debug) { GD.Print($"ExecuteAirMove TODO!"); }
        }
        private void Arrive(bool debug = false)
        {
            if (debug) { 
                GD.Print($"Movement::Arrive()"); 
            }
            Stop();
        }
        /// <summary>
        /// Sets movement state to Inactive and turns navagent off
        /// </summary>
        internal void Reset(NavigationAgent3D navAgent)
        {
            unit.MovementState = AIMOVEMENTSTATE.INACTIVE;
            //navAgent.enabled = true;
        }

        internal void PoolReset(NavigationAgent3D navAgent)
        {
            //if (navAgent.enabled && navAgent.isOnNavMesh) { navAgent.isStopped = true; }
            //navAgent.enabled = false;
            unit.MovementState = AIMOVEMENTSTATE.INACTIVE;
        }

       

        private void OnVelocityComputed(Vector3 safeVelocity)
        {
            frameVelocity = safeVelocity;
        }

    }// EOF CLASS
}
