using Godot;
using Godot.Collections;
using System;

namespace Munglo.DungeonGenerator
{
    [GlobalClass, Tool]
    public partial class SectionResource : DungeonAddonResource
    {
        [Export] public string sectionName = string.Empty;
        [Export] public string sectionType = string.Empty;
        [Export] public string sectionStyle = string.Empty;
        [Export] public ROOMCONNECTIONRESPONCE defaultResponses = (ROOMCONNECTIONRESPONCE)15;
        [ExportGroup("General")]
        [Export] public int sizeWidthMin { get; set; } = 3;
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
        [Export] public Array<PlacerEntryResource> placers;
        [Export] public bool centerSpiralStairs = false;
        [Export] public bool firstPieceDoor = true;
        [Export] public int backDoorChance = 30;
        [Export] public bool allFloor = false;

        internal void VerifyValues()
        {
            if (sizeWidthMax < sizeWidthMin) { sizeWidthMax = Mathf.Clamp(sizeWidthMax, sizeWidthMin, 1000); }
            sizeWidthMin= Mathf.Clamp(sizeWidthMin, 1, sizeWidthMax);

            if (sizeDepthMax < sizeDepthMin) { sizeDepthMax = Mathf.Clamp(sizeDepthMax, sizeDepthMin, 1000); }
            sizeDepthMin = Mathf.Clamp(sizeDepthMin, 1, sizeDepthMax);

            if (nbFloorsMax < nbFloorsMin) { nbFloorsMax = Mathf.Clamp(nbFloorsMax, nbFloorsMin, 1000); }
            nbFloorsMin = Mathf.Clamp(nbFloorsMin, 1, nbFloorsMax);

            backDoorChance = Mathf.Clamp(backDoorChance, 0, 100);
        }
    }// EOF CLASS
}