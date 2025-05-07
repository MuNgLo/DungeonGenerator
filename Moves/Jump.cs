using System;
using Godot;
using Munglo.Movement.Nodes;

namespace Munglo.Movement.Moves;
// All jump related values and calculations here
[GlobalClass]
internal partial class Jump : MMNode
{
    [Export] private bool debug = false;
    [Export] private float jumpSpeed = 4.16f;// The speed at which the character's up axis gains when hitting jump
    [Export] private float coolDown = 0.3f;
    [Export] internal float slopeJumpInheritence = 0.85f;
    private float lastJump = -10.0f;
    /// <summary>
    /// Returns true if currently able to Jump
    /// </summary>
    /// <returns></returns>
    internal bool CanJump(Motor mm)
    {
        bool result = (Time.GetTicksMsec() * 0.001f) > lastJump + coolDown && mm.GroundNormal != Vector3.Zero;
        //if (debug && !result) { GD.Print($"Jump::JumpCheck() > {result}"); }
        return result;
    }
    /// <summary>
    /// Applies a Jump velocity vector to fallVelocity
    /// </summary>
    /// <param name="fallVelocity"></param>
    /// <param name="gravity"></param>
    /// <returns></returns>
    internal Vector3 DoJump(Motor mm, Vector3 velocity)
    {
        Vector3 jumpVelocity;
        // Update timestamp
        lastJump = Time.GetTicksMsec() * 0.001f;
        // Calculate velocity to add for the jump
        Vector3 invGravNormal = (-mm.Gravity).Normalized();
        Vector3 groundNormalized = mm.GroundNormal.Normalized();
        if (debug) { GD.Print($"Jump::DoJump() invGravNormal [{invGravNormal.IsNormalized()}] groundNormalized[{groundNormalized.IsNormalized()}] groundNormal[{mm.GroundNormal}]"); }
        if (invGravNormal != groundNormalized)
        {
            jumpVelocity = invGravNormal.Slerp(groundNormalized, slopeJumpInheritence).Normalized() * jumpSpeed;
        }
        else
        {
            jumpVelocity = invGravNormal * jumpSpeed;
        }
        // Negate any downwards momentum before applying jump
        if (velocity.Normalized().Dot(mm.Gravity.Normalized()) > 0.0f)
        {
            // project fallspeed on gravity vetor and subtract it
            velocity -= velocity.Project(mm.Gravity);
        }
        if (debug) { DrawGizmo(mm, 1.5f, velocity + jumpVelocity, Colors.Cyan); }
        // Apply jump and return new velocity
        return velocity + jumpVelocity;
    }


    private void DrawGizmo(Motor mm, float ttl, Vector3 velocity, Color col)
    {
        Commons.Gizmos.SegmentedGizmo gizmo = new Commons.Gizmos.SegmentedGizmo();
        gizmo.color = col;
        GetTree().Root.AddChild(gizmo);
        gizmo.Position = mm.GlobalPosition;
        gizmo.ttl = ttl;
        gizmo.ClearSegments();
        gizmo.AddSegment(Vector3.Zero, velocity);
    }
}// EOF CLASS