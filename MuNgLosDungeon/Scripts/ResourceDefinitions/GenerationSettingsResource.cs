using Godot;
namespace Munglo.DungeonGenerator
{
    [GlobalClass, Tool]
    public partial class GenerationSettingsResource : DungeonAddonResource
    {
        [ExportGroup("General")]
        [Export] public int nbOfFloors = 1;
        [Export] public FloorResource floorDef;
        
        [ExportGroup("Seed")]
        [Export] public int seed1 = 1111;
        [Export] public int seed2 = 2222;
        [Export] public int seed3 = 3333;
        [Export] public int seed4 = 4444;

        public ulong[] Seed => new[] { (ulong)seed1, (ulong)seed2, (ulong)seed3, (ulong)seed4 };

        public GenerationSettingsResource() { }
    }// EOF CLASS
}
