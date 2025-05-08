using Godot;
using Munglo.Movement.Nodes;
namespace Munglo.Movement.Moves;
[GlobalClass]
internal partial class Sliding : MMNode
{
    [Export] private bool debug = false;
    [Export] private bool canControl = false;
    [Export] private float maxDegree = 45.0f;
    [Export] private float slideControl = 0.3f;

    [ExportCategory("Debugging Settings")]
    [Export] private float lineDuration = 3.0f;
    [Export] private float lineScale = 0.3f;
    [Export] private Color adjustedWish = Colors.Yellow;
    [Export] private Color acceleration = Colors.BlueViolet;
    [Export] private Color finalVelocity = Colors.Green;
    /// <summary>
    /// In Radians
    /// </summary>
    public float MaxAngle => Mathf.DegToRad(maxDegree);
    /// <summary>
    /// True if sliding
    /// </summary>
    /// <param name="mm"></param>
    /// <param name="wishDir"></param>
    /// <param name="flatVelocity"></param>
    /// <param name="delta"></param>
    /// <returns></returns>
    internal bool Slide(Motor mm, Vector3 wishDir, float delta)
    {
        //if (debug) { GD.Print($"Sliding::Slide() ground angle [{mm.GroundAngle}] MaxAngle[{MaxAngle}]"); }
        // Detect steep surface
        if (mm.GroundAngle <= MaxAngle) { return false; }

        Vector3 flatVelocity = mm.GroundPlane.Project(mm.velocity);
        Vector3 fallVelocity = mm.velocity - flatVelocity;



        // make accel vector
        Vector3 slideAcceleration = MakeSlideAccelVector(mm.Gravity, mm.GroundPlane);
        float inclineModifier = (flatVelocity.Normalized().Dot(-slideAcceleration.Normalized()) + 1.0f) * 0.5f  + 1.0f;
        slideAcceleration *= inclineModifier * 50.0f;
        //if (debug) { Core.DrawGizmo(mm.GlobalPosition + Vector3.Up * 0.1f, lineDuration, slideAcceleration * lineScale, acceleration); }

        if (canControl)
        {
            slideAcceleration = slideAcceleration.Lerp(wishDir * slideControl, slideControl);
            //if (debug) { Core.DrawGizmo(mm.GlobalPosition + Vector3.Up * 0.1f, lineDuration, slideAcceleration, adjustedWish); }
        }



        flatVelocity += slideAcceleration * delta;
        //if (debug) { Core.DrawGizmo(mm.GlobalPosition + Vector3.Up * 0.1f, lineDuration, flatVelocity * lineScale, finalVelocity); }

        mm.velocity = flatVelocity + fallVelocity;
        return true;
    }
    /// <summary>
    /// Calculates acceleration vector. Amount relative to angle between groundNormal and gravity.
    /// </summary>
    private Vector3 MakeSlideAccelVector(Vector3 gravity, Plane groundPlane)
    {
        float gravityWeight = Mathf.Abs(groundPlane.Normal.Dot(gravity.Normalized()));
        return groundPlane.Project(gravity).Normalized() * gravityWeight;
    }




    /// <summary>
    /// Changes wishDir to downhill if the direction goes against gravity.
    /// </summary>
    private static void AdjustInputVector(Motor mm, ref Vector3 wishDir)
    {
        Vector3 adjustedwishDir = mm.GroundPlane.Project(wishDir).Normalized();
        if (adjustedwishDir.Dot(mm.Gravity) < 0.0f)
        {
            wishDir = mm.GroundPlane.Project(mm.Gravity).Normalized();
        }
    }

    /*private void DrawGizmo(Vector3 globalPoint, float ttl, Vector3 velocity, Color col)
    {
        Commons.Gizmos.SegmentedGizmo gizmo = new Commons.Gizmos.SegmentedGizmo();
        gizmo.color = col;
        GetTree().Root.AddChild(gizmo);
        gizmo.Position = globalPoint;
        gizmo.ttl = ttl;
        gizmo.ClearSegments();
        gizmo.AddSegment(Vector3.Zero, velocity);
    }*/
}// EOF CLASS