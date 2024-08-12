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
        [Export] public bool showArches = true;

        [ExportGroup("General")]
        [Export] public int nbOfFloors = 1;
        [Export] public int corPerFloor = 4;
        [Export] public int maxBranches = 2;
        [Export] public int chanceForWideCorridor = -1;

        //[ExportGroup("Size")]
        //[Export] public int SizeX = 30;
        //[Export] public int SizeZ = 30;
        //[Export] public int SizeY = 3;

        [ExportGroup("Rooms")]
        [Export] public SectionResource roomStart;
        [Export] public SectionResource roomDefault;
        [Export] public int maxRoomsPerPath = 4;

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
