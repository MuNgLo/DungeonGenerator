using Godot;
namespace Munglo.DungeonGenerator
{
    [GlobalClass, Tool]
    public partial class GenerationSettingsResource : DungeonAddonResource
    {
        [ExportGroup("Show")]
        [Export] public bool showFloors = true;
        [Export] public bool showWalls = true;
        [Export] public bool showCeilings = true;
        [Export] public bool showProps = true;
        [Export] public bool showDebug = true;

        [ExportGroup("Passes")]
        [Export] public bool floorPass = true;
        [Export] public bool wallPass = true;
        [Export] public bool ceilingPass = true;
        [Export] public bool corridorPass = true;
        [Export] public bool propPass = true;
        [Export] public bool debugPass = true;

        [Export] public bool roomPass = true;
        [Export] public bool waterPass = true;

        [ExportGroup("General")]
        [Export] public int nbOfFloors = 1;


        [ExportGroup("Size")]
        [Export] public int SizeX = 30;
        [Export] public int SizeZ = 30;
        [Export] public int SizeY = 3;

        // Corridor things
        [ExportGroup("Corridors")]
        [Export] public int corPerStair = 4;
        [Export] public int corMaxTotal = 20;
        [Export] public int corMaxStraight = 5; // Each is 6m
        [Export] public int corMinStraight = 2;
        [Export] public int maxBranches = 2;


        [ExportGroup("Rooms")]
        [Export] public int maxRoomsPerPath = 4;
        //[Export] public Vector3I roomMaxSize = new Vector3I(5, 1, 5);
        //[Export] public Vector3I roomMinSize = new Vector3I(3, 1, 3);

        [ExportGroup("Water")]
        [Export] public int nbOfWaterPathsPer10 = 8; // Number of water paths to create per 10 in size of Max(X,Z)


        [ExportGroup("Seed")]
        [Export] public int seed1 = 1111;
        [Export] public int seed2 = 2222;
        [Export] public int seed3 = 3333;
        [Export] public int seed4 = 4444;

        public ulong[] Seed => new[] { (ulong)seed1, (ulong)seed2, (ulong)seed3, (ulong)seed4 };

        public GenerationSettingsResource() { }
    }// EOF CLASS
}
