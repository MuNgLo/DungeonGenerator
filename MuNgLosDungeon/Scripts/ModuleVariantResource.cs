using Godot;

namespace Munglo.DungeonGenerator
{
    [GlobalClass, Tool]
    internal partial class ModuleVariantResource : DungeonAddonResource
    {
        [Export] public string variantName;
        [Export] public int[] parts;
    }
}
