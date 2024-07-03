using Godot;
namespace Munglo.DungeonGenerator
{
    [GlobalClass, Tool]
    public partial class ProfileResource : DungeonAddonResource
    {
        [Export] public bool showDebugLayer = false;
        [Export] public GenerationSettingsResource settings;
        [Export] public BiomeDefinition biome;
        [Export] public bool useRandomSeed = true;

    }
}
