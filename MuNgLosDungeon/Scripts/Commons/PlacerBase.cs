using Godot;
using Munglo.DungeonGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonAddonTester.addons.MuNgLosDungeon.Scripts.Commons
{
    public class PlacerBase : IPlacer
    {
        private protected readonly ISection room;
        public PlacerBase(ISection room) { this.room = room; }
        public virtual bool Fit(ISection section)
        {
            throw new NotImplementedException();
        }

        public virtual bool Fit(ISection section, Node3D node)
        {
            throw new NotImplementedException();
        }

        public PackedScene PickRandomProp()
        {
            throw new NotImplementedException();
        }

        public virtual void Place(ISection section)
        {
            throw new NotImplementedException();
        }

        public virtual void Place(ISection section, Node3D node)
        {
            throw new NotImplementedException();
        }
    }
}
