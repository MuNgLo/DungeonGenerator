using Godot;
using System;
using System.Collections.Generic;

namespace Munglo.DungeonGenerator
{
    [Tool, GlobalClass]
    public partial class PlacerResource : DungeonAddonResource, IPlacer
    {
        /*
        [Export] public int chance = 0;
        [Export] public int min = 1;
        [Export] public int max = 10;

        public int Chance { get => chance; set { chance = value; ResourceSaver.Save(this); } }
        public int Min { get => min; set { min = value; ResourceSaver.Save(this); } }
        public int Max { get => max; set { max = value; ResourceSaver.Save(this); } }
        */
        public virtual bool Fit(ISection section)
        {
            throw new System.NotImplementedException();
        }
        public virtual bool Fit(ISection section, Node3D node)
        {
            throw new System.NotImplementedException();
        }
        public virtual bool PickRandomProp(out PackedScene asset, out int count)
        {
            throw new System.NotImplementedException();
        }
        public virtual void Place(ISection section)
        {
            throw new System.NotImplementedException();
        }

        public virtual void Place(ISection section, Node3D node)
        {
            throw new System.NotImplementedException();
        }
    }// EOF CLASS
}
