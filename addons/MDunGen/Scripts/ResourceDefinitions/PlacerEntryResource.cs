using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Munglo.DungeonGenerator
{
    [Tool,GlobalClass]
    public partial class PlacerEntryResource : DungeonAddonResource
    {
        [Export] public bool active = true;
        [Export] public int count = 1;
        [Export] public PlacerResource placer;


        public string Name {  get { return placer is not null ? placer.ResourceName : "UnNamed"; } }


    }// EOF CLASS
}
