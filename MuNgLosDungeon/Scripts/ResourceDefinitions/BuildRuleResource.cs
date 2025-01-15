using Godot;
using System;
using System.Linq;
using System.Resources;

namespace Munglo.DungeonGenerator;

[GlobalClass, Tool]
public partial class BuildRuleResource : DungeonAddonResource
{
    
    // Rule category
    [Export] public CATEGORYRULE catergory = CATEGORYRULE.NONE;
    // Where to start this rule
    [Export] public STARTLOCATIONRULE location = STARTLOCATIONRULE.NONE;
    // Direction
    [Export] public MAPDIRECTION direction = MAPDIRECTION.ANY;
    // Section to insert
    [Export] public SectionResource section;
    
    // How many times we repeat this
    [Export] public int amount = 1;


}// EOF CLASS