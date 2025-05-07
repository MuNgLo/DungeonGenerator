using Godot;

namespace Munglo.Movement.Nodes
{
    /// <summary>
    /// Holds all ground movement related values and calculates movement on ground.
    /// Note that friction is still in physics
    /// </summary>
    [GlobalClass]
    public partial class GroundMovement : MMNode
    {
        [Export] private bool debug = true;
        [Export] public float moveSpeed = 8.0f;                // Ground move speed
        [Export] private float runAcceleration = 10.5f;         // Ground accel
       
        /// <summary>
        /// Takes current velocity, the direction the player wants to go and calculates a velocity change
        /// </summary>
        /// <param name="playerVelocity"></param>
        /// <param name="wishdir"></param>
        /// <returns></returns>
        internal Vector3 GroundMove(Vector3 playerVelocity, Vector3 wishdir, float delta)
        {
            float wishspeed = wishdir.Length();
            wishspeed *= moveSpeed;
            if (debug) { GD.Print($"GroundMovement::GroundMove() wishdir[{wishdir}] wishspeed[{wishspeed}]"); }
            return Accelerate(playerVelocity, wishdir, wishspeed, runAcceleration, delta);
        }

        private Vector3 Accelerate(Vector3 playerVelocity, Vector3 wishdir, float wishspeed, float accel, float delta)
        {
            float addspeed;
            float accelspeed;
            float currentspeed;

            currentspeed = playerVelocity.Dot(wishdir);
            addspeed = wishspeed - currentspeed;

            if (debug) { GD.Print($"GroundMovement::Accelerate() currentspeed[{currentspeed}] addspeed[{addspeed}]"); }

            if (addspeed <= 0)
                return playerVelocity;
            accelspeed = accel * delta * wishspeed;
            if (accelspeed > addspeed)
                accelspeed = addspeed;

            //playerVelocity.x += accelspeed * wishdir.x;
            //playerVelocity.z += accelspeed * wishdir.z;
            return playerVelocity + accelspeed * wishdir;
        }
    }// End of class
}
