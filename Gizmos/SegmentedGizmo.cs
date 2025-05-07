using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Munglo.Commons.Gizmos;
/// <summary>
/// Uses an array of vector3 to draw line segments.
/// Has predefined shapes, offset,scale and colour that can be changed
/// Set shape custom and assign your own vector3 array for your own shapes.
/// </summary>
[GlobalClass, Tool]
public partial class SegmentedGizmo : MeshInstance3D
{
    #region PRIVATE FIELDS
    public float pathScale = 1.0f;
    public double ttl = 3.0f;
    public Color color = Colors.Red;
    private bool isLocal = true;
    public Vector3 offset = Vector3.Zero;
    private List<Vector3> segmentPoints = new List<Vector3>();
    private ImmediateMesh iMesh => this.Mesh as ImmediateMesh;
    private StandardMaterial3D mat = new StandardMaterial3D();
    #endregion
    // Called when the node enters the scene tree for the first time.
    // Note that this means that scene might need to be reloaded for it to work right in editor
    public override void _Ready()
    {
        //segmentPoints.AddRange(GizmoShapes.Diamond);
        VisibilityChanged += WhenVisibiltyChanged;
        UpdateGizmo();
    }
    public override void _PhysicsProcess(double delta)
    {
        ttl -= delta;
        if(ttl < 0.0){ QueueFree(); }
    }
    // Runs a full update of the gizmo
    public void UpdateGizmo()
    {
        if (mat is null) { mat = MaterialOverride as StandardMaterial3D; }
        CastShadow = ShadowCastingSetting.Off;
        mat.DisableReceiveShadows = true;
        mat.AlbedoColor = color;
        mat.EmissionEnabled = true;
        mat.Emission = color;
        mat.EmissionEnergyMultiplier = 0.3f;
        int segCount = Mathf.FloorToInt(segmentPoints.Count - 1);
        // Make new iMesh so even when copying nodes in scene it will have a unique mesh
        Mesh = new ImmediateMesh();
        // Clean up old mesh
        iMesh.ClearSurfaces();

        // Skip if no path
        if (segCount < 1) { return; }
        // Begin draw
        iMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);
        // Build verts
        Vector3 frameOffset = offset - (isLocal ? Vector3.Zero : GlobalPosition);
        for (int i = 0; i < segCount; i++)
        {
            if (segmentPoints[i].Y > 10000.0f) { continue; }
            if (segmentPoints[i + 1].Y > 10000.0f) { continue; }
            // Prepare attributes for add_vertex.
            iMesh.SurfaceSetNormal(Vector3.Back);
            iMesh.SurfaceSetUV(Vector2.Down);
            // Call last for each vertex, adds the above attributes.
            iMesh.SurfaceAddVertex(segmentPoints[i] * pathScale + frameOffset);
            iMesh.SurfaceAddVertex(segmentPoints[i + 1] * pathScale + frameOffset);
        }
        // End drawing
        iMesh.SurfaceEnd();
        mat.Emission = color;
        MaterialOverride = mat;
    }

    public void ClearSegments()
    {
        //GD.Print("SegmentedGizmo::ClearSegments()");
        segmentPoints = new List<Vector3>();
    }
     public void AddSegments(Vector3[] segments, float scale = 1.0f)
    {
        //GD.Print("SegmentedGizmo::AddSegments()");
        for (int i = 0; i < segments.Length - 1; i++)
        {
            AddSegment(segments[i] * scale, segments[i + 1] * scale);
        }
    }
    public void AddSegment(Vector3 from, Vector3 to)
    {
        //GD.Print("SegmentedGizmo::AddSegment()");

        if (segmentPoints.Count < 1)
        {
            segmentPoints.Add(from);
            segmentPoints.Add(to);
            UpdateGizmo();
            return;
        }
        if (segmentPoints.Last() == from)
        {
            segmentPoints.Add(to);
            UpdateGizmo();
            return;
        }
        segmentPoints.Add(new Vector3(0.0f, float.PositiveInfinity, 0.0f));
        segmentPoints.Add(from);
        segmentPoints.Add(to);
        UpdateGizmo();
    }


    // When gizmo is hidden it is completely turned off
    private void WhenVisibiltyChanged()
    {
        if (Visible) { ProcessMode = ProcessModeEnum.Inherit; return; }
        ProcessMode = ProcessModeEnum.Disabled;
    }


}// EOF CLASS