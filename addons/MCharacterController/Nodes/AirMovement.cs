using Godot;
using System;

namespace Munglo.Movement.Nodes
{
    /// <summary>
    /// Holds all air movement related values and calculates movement in air.
    /// </summary>
    [GlobalClass]
    public partial class AirMovement : MMNode
    {
        [Export] private bool debug = true;
        [Export] private float moveSpeed = 8.0f;                // Air move speed
        [Export] private float airAcceleration = 11.2f;          // Air accel
        [Export] private float airBrakeAcceleration = 5.0f;         // Deacceleration experienced when oposite 
        [Export] private float airControl = 0.14f;               // How precise air control is
        [Export] private float strafeOnlyAirControl = 0.14f;      // How precise air control is when only using strafe
        [Export] private float strafeOnlyAirAcceleration = 10.0f;  // Air accel when only sideStrafe
        [Export] private float strafeOnlyMaxSpeed = 1.5f;          // What the max speed to generate when side strafing
        internal Vector3 AirMove(Vector3 playerVelocity, Vector3 wishdir, Vector3 inVec, float delta)
        {
            float control = airControl;
            float wishspeed = wishdir.Length();
            wishspeed *= moveSpeed;
            if (debug) { GD.Print($"AirMovement::AirMove() wishdir[{wishdir}] wishspeed[{wishspeed}]"); }
            float accel;
            if (playerVelocity.Dot(wishdir) < 0 || wishdir == Vector3.Zero)
                accel = airBrakeAcceleration;
            else
                accel = airAcceleration;
            // If the player is ONLY strafing left or right
            
            if (inVec.Y == 0 && inVec.X != 0)
            {
                if (wishspeed > strafeOnlyMaxSpeed)
                    wishspeed = strafeOnlyMaxSpeed;
                accel = strafeOnlyAirAcceleration;
                control = strafeOnlyAirControl;
            }
            Accelerate(ref playerVelocity, wishdir, wishspeed, accel, delta);
            AirControl(ref playerVelocity, wishdir, wishspeed, control);
            return playerVelocity;
        }
        private void Accelerate(ref Vector3 playerVelocity, Vector3 wishdir, float wishspeed, float accel, float delta)
        {
            if (debug) { GD.Print($"AirMovement::Accelerate()"); }
            float addspeed;
            float accelspeed;
            float currentspeed = 0.0f;

            if (playerVelocity != Vector3.Zero)
            {
                currentspeed = playerVelocity.Normalized().Dot(wishdir);
            }
            addspeed = wishspeed - currentspeed;
            if (addspeed <= 0)
                return;
            accelspeed = accel * delta * wishspeed;
            if (accelspeed > addspeed)
                accelspeed = addspeed;

            if(playerVelocity == Vector3.Zero){
                playerVelocity += accelspeed * wishdir;
                return;
            }
            playerVelocity += accelspeed * wishdir.Project(playerVelocity);
        }

        private void AirControl(ref Vector3 playerVelocity, Vector3 wishdir, float wishspeed, float control)
        {
            if (debug) { GD.Print($"AirMovement::AirControl()"); }
            float speed = playerVelocity.Length();

            //float dot = (Vector3.Dot(playerVelocity.normalized, wishdir) + 1.0f) * 0.5f;
            float dot = playerVelocity.Normalized().Dot(wishdir);

            Vector3 newDir = playerVelocity + wishdir * Math.Abs(dot) * control;
            playerVelocity = newDir.Normalized() * speed;
        }
    }// end of class
}
