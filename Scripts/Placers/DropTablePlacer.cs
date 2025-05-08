using Godot;
using System.Collections.Generic;

namespace Munglo.DungeonGenerator.Placers
{
    [Tool, GlobalClass]
    internal partial class DropTablePlacer : PlacerResource, IPlacer
    {
        [Export] public DropTableResource table;

        public override bool PickRandomProp(out PackedScene asset, out int count)
        {
            asset = null;
            count = 1;
            if (table is null) { return false; }
            if (table.GetRandomAsset(out asset, out count)) { return true; }
            return false;
        }
   
        public override void Place(ISection section)
        {
            //GD.Print($"DropTablePlacer::Place({section.SectionContainer.Name}) running[{ResourcePath}]");
            List<Vector3> positions = (section as SectionBase).PropGrid.GetFloorPositions(0);
            if (positions.Count == 0) { return; }
            if (table is null) { return; }
            foreach (DropTableEntryResource entry in table.forcedEntries)
            {
                if (section.RNG.Next(100) < entry.weight)
                {
                    for (int c = 0; c < entry.assetCount; c++)
                    {
                        if ((section as SectionBase).PropGrid.GetRandomFloorPosition(0, section.RNG, out Vector3 pos))
                        {
                            Node3D copy = entry.asset.Instantiate() as Node3D;
                            section.SectionContainer.AddChild(copy, true);
                            copy.Position = pos;
                            copy.RotationDegrees = new Vector3(0.0f, section.RNG.Next(359), 0.0f);
                        }
                    }

                }
            }

        }
        public void DoForcedRolls(ISection section)
        {
            GD.Print($"DropTablePlacer::DoForcedRolls({section.SectionContainer.Name}) running[{ResourcePath}]");
        }
    }// EOF CLASS
}
