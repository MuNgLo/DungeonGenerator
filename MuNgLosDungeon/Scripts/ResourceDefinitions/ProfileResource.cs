using Godot;
using System.Security.Cryptography.X509Certificates;

namespace Munglo.DungeonGenerator
{
    [GlobalClass, Tool]
    public partial class ProfileResource : DungeonAddonResource
    {
        [Export] public bool showDebugLayer = false;
        [Export] public GenerationSettingsResource settings;
        [Export] public BiomeResource biome;
        [Export] public bool useRandomSeed = true;
    }
}
