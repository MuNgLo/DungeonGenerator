using System.Collections.Generic;
using Godot;

namespace GizmosCSharp;

[Tool]
public static class GizmoUtils
{
    public static void DrawLine(Vector3 start, Vector3 end, float duration = 1.0f)
    {
        DrawLine(start,end,duration,Colors.Yellow);
    }
    public static void DrawLine(Vector3 start, Vector3 end, float duration, Color col)
    {
        SegmentedGizmo gizmo = new SegmentedGizmo(); gizmo.pathScale = 1.0f;
        gizmo.color = col;
        EditorInterface.Singleton.GetEditedSceneRoot().AddChild(gizmo);
        List<Vector3> path = new List<Vector3>() { start, end };
        gizmo.ClearSegments();
        gizmo.AddSegments(path);
        gizmo.UpdateGizmo(duration);
    }

      public static void DrawShape(Vector3 location, GSHAPES shape, float duration = 1.0f, float scale = 1.0f)
    {
        DrawShape(location,shape,duration, scale, Colors.Yellow);
    }
    public static void DrawShape(Vector3 location, GSHAPES shape, float duration, float scale, Color col)
    {
        SegmentedGizmo gizmo = new SegmentedGizmo(); gizmo.pathScale = 1.0f;
        gizmo.color = col;
        EditorInterface.Singleton.GetEditedSceneRoot().AddChild(gizmo);
        gizmo.GlobalPosition = location;
        gizmo.Scale = Vector3.One * scale;
        gizmo.ClearSegments();
        gizmo.AddSegments(GizmoShapes.GetShape(shape));
        gizmo.UpdateGizmo(duration);
    }
}// EOF CLASS

