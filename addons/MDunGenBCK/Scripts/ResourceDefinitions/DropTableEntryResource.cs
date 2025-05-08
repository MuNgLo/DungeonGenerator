using Godot;

namespace Munglo.DungeonGenerator
{
    [Tool, GlobalClass]
    public partial class DropTableEntryResource : DungeonAddonResource
    {
        /// <summary>
        /// When forced it means we always roll the chance of it happening using the weight value as percentage.
        /// It is also excluded from random pulls from the table
        /// </summary>
        [Export] public bool forcedRoll = true;
        [Export] public int Chance { get; set; } = 20;
        /// <summary>
        /// When forced the weight is the percentage change it will appear
        /// </summary>
        [Export] public int weight = 20;
        [Export] public PackedScene asset;
        [Export] public int assetCount = 1;
    }// EOF CLASS
}
