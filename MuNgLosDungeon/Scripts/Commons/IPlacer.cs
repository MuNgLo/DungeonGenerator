using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Munglo.DungeonGenerator
{
    public interface IPlacer
    {
        public PackedScene PickRandomProp();
        public void Place(ISection section);
        public void Place(ISection section, Node3D node);
        public bool Fit(ISection section);
        public bool Fit(ISection section, Node3D node);
    }
}
