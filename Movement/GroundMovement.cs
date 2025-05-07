using Godot;
using Munglo.AI.Base;

namespace Munglo.AI.Movement
{
    public static class GroundMovement
    {
        public static AIMOVEMENTSTATE Move(NavigationAgent3D agent, Vector3 targetlocation, bool debug = false)
        {
            if (debug) { GD.Print($"GroundMovement::ExecuteGroundMove()"); }

            agent.TargetPosition = targetlocation;
            return AIMOVEMENTSTATE.ACTIVE;
        }

        public static AIMOVEMENTSTATE RandomMove(Node3D unit, NavigationAgent3D agent, float maxDistance, bool debug = false)
        {
            // check agentus tings
            if (agent.ProcessMode == Node.ProcessModeEnum.Disabled)
            {
                GD.Print($"GroundMovement::RandomMove()  Agent not enabled");
                return AIMOVEMENTSTATE.INACTIVE;
            }

            float distance = (float)GD.RandRange(0.0f, maxDistance * 0.7f) - maxDistance * 0.30f;
            DoMoveTo(
                unit,
                agent,
                maxDistance,
                Navigation.RandomCirclePointInRange(unit.GlobalPosition + -unit.Transform.Basis.Z * distance, maxDistance * 0.5f),
                debug);
            return AIMOVEMENTSTATE.ACTIVE;
        }

        private static void DoMoveTo(Node3D unit, NavigationAgent3D agent, float maxDistance, Vector3 worldPoint, bool debug = false)
        {
            worldPoint = NavigationServer3D.MapGetClosestPoint(agent.GetNavigationMap(), worldPoint);
            agent.TargetPosition = worldPoint;
        }
    }// EOF CLASS
}
