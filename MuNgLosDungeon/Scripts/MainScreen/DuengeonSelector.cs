using Godot;
using Munglo.DungeonGenerator;
using System;

[Tool]
public partial class DuengeonSelector : SubViewportContainer
{
    MainScreen MS;
    Camera3D cam;
    SubViewport subV;
    Node3D cube;
    Node3D cursor;
    public override void _EnterTree()
	{
        MS = GetParent<MainScreen>();
        cam = GetNode<Camera3D>("SubViewport/Camera3D");
        subV = GetNode<SubViewport>("SubViewport");
        cube = FindChild("Dungeon") as Node3D;
        cursor = subV.FindChild("Cursor") as Node3D;
        cursor.Hide();
    }
    public void DoRayCastIntoSubViewport()
    {
        //float z = 20.0f;
        Vector2 position2D = subV.GetMousePosition();
        //Plane dropPlane = new Plane(new Vector3(0, 0, 1), z);
        Vector3 cursorWorldPos = cam.ProjectRayOrigin(position2D);
        Vector3 rayDir = cam.ProjectRayNormal(position2D);
        World3D world = cube.GetWorld3D();
        if (TryToHit(cursorWorldPos, rayDir, world, out Node3D hit))
        {
            GD.Print($"TestClickInSubView::DoRayCastIntoSubViewport() Hit!![{hit.Name}]");

            cursor.GlobalPosition = Dungeon.GlobalSnapPosition(hit.GlobalPosition);
            cursor.Show();


            ScreenDungeonVisualizer vis = FindChild("Dungeon") as ScreenDungeonVisualizer;
            MapPiece piece = vis.GetMapPiece(Dungeon.GlobalSnapCoordinate((Vector3I)hit.GlobalPosition));
            if(piece != null )
            {
                GD.Print($"MapPiece[{piece.Coord}] section[{piece.SectionIndex}] floor[{piece.hasFloor}] bridge[{piece.isBridge}] stair[{piece.hasStairs}]" +
                    $"Section has [{piece.Section.ConnectionCount}] connections.");
            }
            return;
        }
        cursor.Hide();
    }
    public bool TryToHit(Vector3 startPoint, Vector3 dir, World3D world, out Node3D hit)
    {
        Vector3 endPos = startPoint + dir * 1000.0f;
        PhysicsDirectSpaceState3D spaceState = PhysicsServer3D.SpaceGetDirectState(world.Space);
        Godot.Collections.Array<Rid> excluding = new Godot.Collections.Array<Rid> { };
        PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(startPoint, endPos, exclude: excluding);
        Godot.Collections.Dictionary results = spaceState.IntersectRay(query);
        
        GD.Print($"TestClickInSubView::TryToHit() results Count[{results.Keys.Count}] IslandCount[{PhysicsServer3D.GetProcessInfo(PhysicsServer3D.ProcessInfo.IslandCount)}]");
        if (results.Keys.Count > 0)
        {
            hit = (results["collider"].AsGodotObject() as Node3D).GetParent<Node3D>();
            return true;
        }
        hit = null;
        return false;
    }
}