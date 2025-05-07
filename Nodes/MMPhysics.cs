
using Godot;

namespace Munglo.Movement.Nodes
{
    /// <summary>
    /// This is the base physics for the movment. this handles cast to se if we can rotate player, move him or rotate.
    /// </summary>
    [GlobalClass]
    public partial class MMPhysics : MMNode
    {
        [Export] private bool debug = true;
        [Export] public float rotationSpeed = 120.0f; // How fast we rotate towards gravity when doing it manually
        [Export] public float rotationSpeedModifier = 0.3f; // modifier to rotationSpeed when we rotate without manual input
        [Export]private float runDeacceleration = 10.0f;       // Deacceleration that occurs when running on the ground
        [Export]private float friction = 6; //Ground friction
        [Export]public float groundDistance = 0.04f;
        [Export]public float pushDownDistance = 0.08f;
        [Export]public float stepHeight = 0.2f;

        [Export]public Plane gravityPlane;


        [Export]private float lastGravityChange = 0.0f;
        [Export]private Vector3 currentGravity;
        public Vector3 Gravity { get { return currentGravity; } set { SetGravity(value); } }

        /// <summary>
        /// Call this after initiazion to make additional preperations like setting an initial gravity
        /// </summary>
        internal void Setup()
        {
            Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity_vector").AsVector3() * (float)ProjectSettings.GetSetting("physics/3d/default_gravity").AsDouble();
        }
        /// <summary>
        /// This makes sure we keep track of gravity vectors
        /// </summary>
        /// <param name="newGravity"></param>
        private void SetGravity(Vector3 newGravity)
        {
            if(newGravity == currentGravity) { return; }
            if (debug) { GD.Print($"MMPhysics::SetGravity() newGravity[{newGravity}]"); }
            currentGravity = newGravity;
            lastGravityChange = Time.GetTicksMsec() * 0.001f;
            gravityPlane = new(currentGravity.Normalized());
        }
        /// <summary>
        /// Applies friction to the player. Usually only when grounded
        /// </summary>
        /// <param name="modifier"></param>
        /// <param name="playerVelocity"></param>
        /// <returns></returns>
        internal Vector3 ApplyFriction(float modifier, Vector3 playerVelocity, float delta)
        {
            float speed = playerVelocity.Length();
            float drop = 0.0f;
            float control = speed < runDeacceleration ? runDeacceleration : speed;
            drop = control * friction * delta * modifier;
            float newspeed = speed - drop;
            if (newspeed < 0)
                newspeed = 0;
            if (speed > 0)
                newspeed /= speed;
            return playerVelocity * newspeed;
        }
    }// End of class
}
