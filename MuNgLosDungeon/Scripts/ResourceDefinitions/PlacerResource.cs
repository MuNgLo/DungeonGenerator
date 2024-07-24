using Godot;
using System.Collections.Generic;

namespace Munglo.DungeonGenerator
{
    [Tool, GlobalClass]
    public partial class PlacerResource : DungeonAddonResource, IPlacer
    {
        [Export] public DropTableResource table;



        public bool Fit(ISection section)
        {
            throw new System.NotImplementedException();
        }

        public bool Fit(ISection section, Node3D node)
        {
            return true;
        }

        public bool PickRandomProp(out PackedScene asset, out int count)
        {
            asset = null;
            count = 1;
            if(table is null) { return false; }
            if( table.GetRandomAsset(out asset, out count)) { return true; }
            return false;
        }

        public void Place(ISection section)
        {
            GD.Print($"PlacerResource::Place({section.SectionContainer.Name}) running[{ResourcePath}]");
        }

        public void Place(ISection section, Node3D node)
        {
            //GD.Print($"PlacerResource::Place({section.SectionContainer.Name}, {node.Name}) running[{ResourcePath}]");

            List<Vector3> positions = (section as SectionBase).PropGrid.GetFloorPositions(0);

            if(positions.Count == 0) { return; }

            for (int i = 0; i < 1; i++)
            {
                int pick = section.RNG.Next(positions.Count - 1);
                Node3D copy = node.Duplicate() as Node3D;
                section.SectionContainer.AddChild(copy, true);
                copy.Position = positions[pick];
                copy.RotationDegrees = new Vector3(0.0f, section.RNG.Next(359), 0.0f);
                positions.RemoveAt(pick);
                if(positions.Count < 5) { break; }
            }
        }
        public void DoForcedRolls(ISection section)
        {
            if(table is null) { return; }
            foreach (DropTableEntryResource entry in table.forcedEntries)
            {
                if(section.RNG.Next(100) < entry.weight)
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

    }// EOF CLASS
}
