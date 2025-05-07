using Godot;
using Munglo.AI;
using System;
using System.Threading.Tasks.Dataflow;

namespace Munglo.Commons
{
    /// <summary>
    /// Anything that needs to be tracked as an object of interest for AI needs to inherit from this class
    /// Use class AIObjectDebugBehaviour to control it in a dev enviroment
    /// </summary>
    public partial class AIObject : Node
    {
        private protected Node3D body;
        public Vector3 GlobalPosition => body.GlobalPosition;
        public Node3D Body { get => body; }
        //[Tooltip("This is a unique ID to track this through the matrix. Assigned by AIManager on registration")]
        [Export] public int aiObjectID = -1;
        //[Tooltip("This is a unique faction ID")]
        [Export] public int factionID = -1;
        //[SerializeField, Tooltip("Make knowledge of this object permanent")]
        [Export] private protected bool kPermananent = false;
        //[Header("AI Object", order = 10)]
        [Export] public Aabb bounds;
        //[SerializeField, Tooltip("Declare what this object is"), HideInInspector]
        
        
        private AITYPE aiType = AITYPE.GENERIC;
        private float timeSinceLastUpdate = 1000.0f;
        public float TimeSinceLastUpdate { get => timeSinceLastUpdate; set => timeSinceLastUpdate = value; }
        public bool isPermanantKnowledge { get => kPermananent; }
        public AITYPE AIType { get => aiType; }

        internal void DeRegister(bool alsoDestroy)
        {
            AIManager.Instance.DeRegisterAIObject(aiObjectID, alsoDestroy);
        }

    }// eof CLASS
}