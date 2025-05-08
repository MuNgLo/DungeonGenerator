using Godot;
using Munglo.DungeonGenerator.Pathfinding;
using System;
namespace Munglo.DungeonGenerator.UI;
[Tool]
public partial class PathInfo : Control
{
	[Export] private BottomScreen BS;
	[Export] private RichTextLabel pathDebugInfo;
	public override void _EnterTree()
	{
		BS.addon.MS.OnPathDataPushed += WhenPathDataPushed;
	}

    private void WhenPathDataPushed(object sender, PathData e)
    {
        pathDebugInfo.Text = e.ToString();
    }


}// EOF CLASS
