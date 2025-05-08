using Godot;
using Godot.Collections;
using System;
using System.Reflection;


namespace Munglo.DungeonGenerator.UI;
/// <summary>
/// The bottom window center editor to view and change dungeon configuration
/// </summary>
[Tool]
public partial class BottomScreen : Control
{
    public Dungeons addon;
    [Export] private Label sectionInfo;
    [Export] private Label mapPieceInfo;
    [Export] private Label connectionInfo;
    public override void _EnterTree()
    {
        addon.MS.Selection.OnSelectionChanged += WhenselectionChanged;
        connectionInfo.Text = string.Empty;
    }

    private void WhenselectionChanged(object sender, EventArgs e)
    {
        MapPiece mp = addon.MS.Selection.SelectedMapPiece;
        ISection ss = addon.MS.Selection.SelectedSection;
        SectionConnection sc = addon.MS.Selection.SelectedConnection;
        if (mp is not null)
        {
            mapPieceInfo.Text = $"Mappiece Info:\n MapPiece[{mp.Coord}] section[{mp.SectionIndex}] floor[{mp.hasFloor}] bridge[{mp.isBridge}] stair[{mp.hasStairs}]";
        }
        if (ss is not null)
        {
            sectionInfo.Text = $"Section Info:\n Section[{ss.SectionIndex}] has [{ss.ConnectionCount}] connections. Section Min/Max [{ss.MinCoord} / {ss.MaxCoord}]";
        }
        if (sc is not null)
        {
            connectionInfo.Text = $"Connection Info:\n{sc}";
        }
        else
        {
            connectionInfo.Text = string.Empty;
        }
    }
}// EOF CLASS
