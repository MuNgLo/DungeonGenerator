using Godot;
namespace Munglo.DungeonGenerator
{
    [GlobalClass, Tool]
    public partial class RoomResource : Resource
    {
        [Export] public string roomName = string.Empty;
        [Export] public string roomStyle = string.Empty;
        [Export] public ROOMCONNECTIONRESPONCE defaultResponses = (ROOMCONNECTIONRESPONCE)15;
        [ExportGroup("General")]
        [Export] public int sizeWidthMin = 3;
        [Export] public int sizeWidthMax = 5;
        [Export] public int sizeDepthMin = 3;
        [Export] public int sizeDepthMax = 5;

        [Export] public int nbFloorsMin = 1;
        [Export] public int nbFloorsMax = 2;

        [ExportGroup("Debug")]
        [Export] public bool debug = false;
        [Export] public int nbDoorsPerFloorMin = 0;
        [Export] public int nbDoorsPerFloorMax = 0;

        [ExportGroup("WiP")]
        [Export] public bool centerSpiralStairs = false;
        [Export] public bool firstPieceDoor = true;
        [Export] public int backDoorChance = 30;
        [Export] public bool allFloor = false;

    }// EOF CLASS
}