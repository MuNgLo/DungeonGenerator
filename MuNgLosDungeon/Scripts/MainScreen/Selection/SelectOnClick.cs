using Godot;
namespace Munglo.DungeonGenerator.UI
{
    [Tool]
    public partial class SelectOnClick : SubViewportContainer
    {
        private MainScreen MS;
        private Camera3D cam;
        private SubViewport subV;
        private Node3D cube;


        public override void _EnterTree()
        {
            MS = GetParent<MainScreen>();
            cam = GetNode<Camera3D>("SubViewport/Camera3D");
            subV = GetNode<SubViewport>("SubViewport");
            cube = FindChild("Dungeon") as Node3D;
        }
    
        public void DoRayCastIntoSubViewport()
        {
            //float z = 20.0f;
            Vector2 position2D = subV.GetMousePosition();
            //Plane dropPlane = new Plane(new Vector3(0, 0, 1), z);
            Vector3 cursorWorldPos = cam.ProjectRayOrigin(position2D);
            Vector3 rayDir = cam.ProjectRayNormal(position2D);
            World3D world = cube.GetWorld3D();
            if (TryToHit(cursorWorldPos, rayDir, world, out Node3D hit, out Vector3 point))
            {
                (subV.FindChild("Target") as Node3D).GlobalPosition = point;
                ScreenDungeonVisualizer vis = FindChild("Dungeon") as ScreenDungeonVisualizer;
                MapPiece mapPiece = vis.GetMapPiece(Dungeon.GlobalSnapCoordinate((Vector3I)point));
                if (mapPiece is null) { return; }
                MS.Selection.SelectMapPiece(mapPiece);
                return;
            }
        }
        public bool TryToHit(Vector3 startPoint, Vector3 dir, World3D world, out Node3D hit, out Vector3 point)
        {
            Vector3 endPos = startPoint + dir * 1000.0f;
            point = Vector3.Zero;
            PhysicsDirectSpaceState3D spaceState = PhysicsServer3D.SpaceGetDirectState(world.Space);
            Godot.Collections.Array<Rid> excluding = new Godot.Collections.Array<Rid> { };
            PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(startPoint, endPos, exclude: excluding);
            Godot.Collections.Dictionary results = spaceState.IntersectRay(query);
            if (results.Keys.Count > 0)
            {
                hit = (results["collider"].AsGodotObject() as Node3D).GetParent<Node3D>();

                point = results["position"].AsVector3();

                return true;
            }
            hit = null;
            return false;
        }
    }// EOF
}