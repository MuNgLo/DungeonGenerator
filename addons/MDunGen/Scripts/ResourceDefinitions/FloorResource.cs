using Godot;
using System;
using System.Linq;
using System.Resources;

namespace Munglo.DungeonGenerator;

[GlobalClass, Tool]
public partial class FloorResource : DungeonAddonResource
{
    [Export] public BuildRuleResource[] rules;
}// EOF CLASS