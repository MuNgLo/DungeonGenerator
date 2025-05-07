using System;
using Godot;

namespace Munglo.Movement.Base
{
    [Serializable]
    public class GroundTest
    {
        public bool grounded;
        public Vector3 groundPoint;
        public Vector3 groundNormal;
        public float groundAngle;
    }
  
    [Serializable]
    public struct PlayerMovementEventArguments
    {
        public bool InputActive;
        public bool CanJump;
        public bool MovementActive;
        /// <summary>
        /// Set this true if you want rigidbody simulated.
        /// </summary>
        public bool Simulate;
    }
    [Serializable]
    public struct PlayerMovementResetArguments
    {
        public Vector3 Location;
        public Quaternion Rotation;
        public Vector3 Gravity;
    }
    
}// EOF Namespace
