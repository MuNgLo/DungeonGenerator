using Godot;

namespace Munglo.DungeonGenerator
{
    [Tool,GlobalClass]
    internal partial class PathResource : SectionResource
    {
        [ExportGroup("Corridor")]
        [Export] public int corMaxTotal = 60;
        [Export] public int corMaxStraight = 5;
        [Export] public int corMinStraight = 2;

    }
}
