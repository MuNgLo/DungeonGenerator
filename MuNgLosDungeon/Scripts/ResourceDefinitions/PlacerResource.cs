using Godot;
using System.Collections.Generic;

namespace Munglo.DungeonGenerator
{
    [Tool, GlobalClass]
    public partial class PlacerResource : DungeonAddonResource, IPlacer
    {
        [Export] private PackedScene[] props;
        public bool Fit(ISection section)
        {
            throw new System.NotImplementedException();
        }

        public bool Fit(ISection section, Node3D node)
        {
            return true;
        }

        public PackedScene PickRandomProp()
        {
            return props[0];
        }

        public void Place(ISection section)
        {
            GD.Print($"PlacerResource::Place({section.SectionContainer.Name}) running[{ResourcePath}]");
        }

        public void Place(ISection section, Node3D node)
        {
            //GD.Print($"PlacerResource::Place({section.SectionContainer.Name}, {node.Name}) running[{ResourcePath}]");

            List<Vector3> positions = (section as SectionBase).PropGrid.GetFloorPositions(0);
            for (int i = 0; i < 10; i++)
            {
                int pick = GD.RandRange(0,positions.Count - 1);
                Node3D copy = node.Duplicate() as Node3D;
                section.SectionContainer.AddChild(copy, true);
                copy.Position = positions[pick];
                positions.RemoveAt(pick);
                if(positions.Count < 5) { break; }
            }
        }
    }// EOF CLASS
}
