using Godot;
using System;
using Munglo.DungeonGenerator.Gizmos;
using System.Linq;
using Munglo.DungeonGenerator.Pathfinding;
using System.Collections.Generic;

namespace Munglo.DungeonGenerator.UI.Selection;

internal class Manager
{
    internal Dungeons addon;

    private MainScreen MS;
    private ScreenDungeonVisualizer dunVis;
    private ActivePath activePath;

    private PackedScene gizmoDiamond;
    private PackedScene gizmoConnParent;
    private PackedScene gizmoConnChild;
    private PackedScene gizmoSegmented;

    private MapPiece selectedMapPiece;
    private MapPiece selectedMapPiece2nd;
    private ISection selectedSection;
  

    private SectionResource selectedSectionResource;
    internal MapPiece SelectedMapPiece => selectedMapPiece;
    internal ISection SelectedSection => selectedSection;
    internal SectionResource SelectedSectionResource => selectedSectionResource;
    internal EventHandler OnSelectionChanged;
    internal PackedScene GizmoSegmented => gizmoSegmented;

    internal Manager(Dungeons dun, MainScreen ms, ScreenDungeonVisualizer dv)
    {
        activePath = new ActivePath(this);
        addon = dun;
        MS = ms;
        dunVis = dv;
        gizmoDiamond = ResourceLoader.Load("res://addons/MuNgLosDungeon/Scenes/Gizmos/diamond.tscn") as PackedScene;
        gizmoConnParent = ResourceLoader.Load("res://addons/MuNgLosDungeon/Scenes/Gizmos/connparent.tscn") as PackedScene;
        gizmoConnChild = ResourceLoader.Load("res://addons/MuNgLosDungeon/Scenes/Gizmos/connchild.tscn") as PackedScene;
        gizmoSegmented = ResourceLoader.Load("res://addons/MuNgLosDungeon/Scenes/Gizmos/segmented.tscn") as PackedScene;
        MS.OnMainScreenUIUpdate += UpdateGizmos;
    }

    private void UpdateGizmos(object sender, EventArgs e)
    {
        // Clear All Gizmos
        foreach (Node3D child in MS.Gizmos.GetChildren())
        {
            child.QueueFree();
        }
        // Insert Diamond on mappiece
        if (selectedMapPiece is not null)
        {
            Node3D d = gizmoDiamond.Instantiate() as Node3D;
            MS.Gizmos.AddChild(d);
            d.Position = Dungeon.GlobalPosition(selectedMapPiece);
        }
        // Insert Diamond on path target mappiece
        if (selectedMapPiece2nd is not null)
        {
            Node3D d = gizmoDiamond.Instantiate() as Node3D;
            MS.Gizmos.AddChild(d);
            d.Position = Dungeon.GlobalPosition(selectedMapPiece2nd);
            activePath.UpdatePathGizmos(selectedMapPiece, selectedMapPiece2nd);
        }
        // Insert section gizmos
        if (selectedSection is not null)
        {
            // Add bridge flag gizmos
            foreach (MapPiece piece in selectedSection.Pieces)
            {
                if (piece.isBridge)
                {
                    SegmentedGizmo d = gizmoSegmented.Instantiate() as SegmentedGizmo;

                    d.pathScale = 0.5f;
                    d.offset = Vector3.Up * 0.5f;
                    d.ClearSegments();
                    d.AddSegments(GizmoShapes.Bridge);


                    MS.Gizmos.AddChild(d);
                    d.Position = Dungeon.GlobalPosition(piece.Coord);
                    d.GlobalRotationDegrees = Dungeon.ResolveRotation(piece.Orientation);
                }
            }

            // Put arrows on connection tiles
            foreach (MapCoordinate conKey in selectedSection.Connections)
            {
                if (!dunVis.Map.Connections.ContainsKey(conKey))
                {
                    GD.PushError($"Selection > Manager::UpdateGizmos() connection key was missing in MapData!");
                    return;
                }
                SectionConnection connection = dunVis.Map.Connections[conKey];
                //GD.Print($"Selection.Manager::UpdateGizmos() Adding arrow gizmo on connection[{connection.Coord}] connection.Dir[{connection.Dir}]");
                // Parent
                if (selectedSection.SectionIndex == connection.ParentSection)
                {
                    Node3D d = gizmoConnParent.Instantiate() as Node3D;
                    MS.Gizmos.AddChild(d);
                    d.Position = Dungeon.GlobalPosition(connection.Coord);
                    d.GlobalRotationDegrees = Dungeon.ResolveRotation(Dungeon.Flip(connection.Dir));
                }
                // Child
                if (selectedSection.SectionIndex == connection.ChildSection)
                {
                    Node3D d = gizmoConnParent.Instantiate() as Node3D;
                    d = gizmoConnChild.Instantiate() as Node3D;
                    MS.Gizmos.AddChild(d);
                    d.Position = Dungeon.GlobalPosition(connection.Coord + connection.Dir);
                    d.GlobalRotationDegrees = Dungeon.ResolveRotation(connection.Dir);
                }

            }



            if (selectedSection.SectionStyle == "Connector")
            {
                AddMapPieceGizmoOnCoord(selectedSection.Pieces.First().Coord);
                AddMapPieceGizmoOnCoord(selectedSection.Pieces.Last().Coord);
                return;
            }

            AddSectionGizmo(selectedSection);


        }
    }
    private void AddMapPieceGizmoOnCoord(MapCoordinate coord)
    {
        Node3D segmented = gizmoSegmented.Instantiate() as Node3D;
        MS.Gizmos.AddChild(segmented);
        segmented.Position = Dungeon.GlobalPosition(coord);
        Vector3[] mapPieceBox = BuildMapPieceBox();
        (segmented as SegmentedGizmo).ClearSegments();
        (segmented as SegmentedGizmo).AddSegments(mapPieceBox);
    }
    internal void AddSectionGizmo(int sectionID)
    {
        AddSectionGizmo(dunVis.Map.Sections[sectionID]);
    }
    internal void AddSectionGizmo(ISection section)
    {
        Node3D segmented = gizmoSegmented.Instantiate() as Node3D;
        MS.Gizmos.AddChild(segmented);
        segmented.Position = Dungeon.GlobalPosition(section.MinCoord);
        Vector3[] mapPieceBox = BuildMapPieceBox(section.MaxCoord - section.MinCoord);
        (segmented as SegmentedGizmo).ClearSegments();
        (segmented as SegmentedGizmo).AddSegments(mapPieceBox);
    }
    private Vector3[] BuildMapPieceBox()
    {
        return BuildMapPieceBox(new MapCoordinate(0, 0, 0));
    }

    private Vector3[] BuildMapPieceBox(MapCoordinate size)
    {
        float width = size.x * 6.0f;
        float depth = size.z * 6.0f;
        float height = size.y * 6.0f;

        Vector3[] box = new Vector3[]{
                // TOP
                new Vector3(3.0f + width,5.0f + height,3.0f + depth),
                new Vector3(-3.0f,5.0f + height,3.0f + depth),
                new Vector3(-3.0f,5.0f + height,-3.0f),
                new Vector3(3.0f + width,5.0f + height,-3.0f),
                new Vector3(3.0f + width,5.0f + height,3.0f + depth),

                new Vector3(0.0f,float.PositiveInfinity,0.0f),
                // Bottom
                new Vector3(-3.0f,-1.0f,3.0f + depth),
                new Vector3(-3.0f,-1.0f,-3.0f),
                new Vector3(3.0f + width,-1.0f,-3.0f),
                new Vector3(3.0f + width,-1.0f,3.0f + depth),
                new Vector3(-3.0f,-1.0f,3.0f + depth),


                new Vector3(0.0f,float.PositiveInfinity,0.0f),

                new Vector3(-3.0f,5.0f,3.0f + depth),
                new Vector3(-3.0f,-1.0f + height,3.0f + depth),

                new Vector3(0.0f,float.PositiveInfinity,0.0f),

                new Vector3(-3.0f,-1.0f,-3.0f),
                new Vector3(-3.0f,5.0f + height,-3.0f),

                new Vector3(0.0f,float.PositiveInfinity,0.0f),

                new Vector3(3.0f + width,-1.0f,-3.0f),
                new Vector3(3.0f + width,5.0f + height,-3.0f),

                new Vector3(0.0f,float.PositiveInfinity,0.0f),

                new Vector3(3.0f + width,-1.0f,3.0f + depth),
                new Vector3(3.0f + width,5.0f + height,3.0f + depth)
                };
        return box;
    }

    internal void ClearSelection()
    {
        selectedMapPiece = null;
        selectedSection = null;
        if (addon.Mode != VIEWERMODE.SECTION) { selectedSectionResource = null; }
        UpdateGizmos(this, null);
    }
    internal void SelectFirstSection(bool runUpdates = true)
    {
        GD.Print($"Selection.Manager::SelectFirstSection()");
        ScreenDungeonVisualizer vis = MS.FindChild("Dungeon") as ScreenDungeonVisualizer;
        if (vis.Map.Sections.Count < 1) { return; }
        SelectSection(0);
    }
    internal void SelectPathTargetMapPiece(MapPiece mapPiece, bool runUpdates = true)
    {
        GD.Print($"Selection.Manager::SelectPathTargetMapPiece()");
        if (mapPiece == null || selectedMapPiece is null) { return; }
        selectedMapPiece2nd = mapPiece;
        //SelectSection(mapPiece.SectionIndex, false);
        if (runUpdates)
        {
            RaiseSelectionChanged();
            MS.RaiseUpdateUI();
        }
    }
    internal void SelectMapPiece(MapPiece mapPiece, bool runUpdates = true)
    {
        if (mapPiece == null) { return; }
        selectedMapPiece = mapPiece;
        selectedMapPiece2nd = null;
        SelectSection(mapPiece.SectionIndex, false);
        if (runUpdates)
        {
            RaiseSelectionChanged();
            MS.RaiseUpdateUI();
        }


    }
    internal void SelectSection(int sectionIndex, bool runUpdates = true)
    {
        if (sectionIndex < 0) { return; }
        selectedSection = dunVis.Map.Sections[sectionIndex];

        if (runUpdates)
        {
            RaiseSelectionChanged();
            MS.RaiseUpdateUI();
        }
    }
    internal void SelectSectionResource(Resource res, bool runUpdates = true)
    {
        if (res is not SectionResource) { return; }
        GD.Print($"Selection.Manager::SelectSectionResource() SectionResource is [{res.ResourcePath}]");

        if (selectedSectionResource is not null)
        {
            selectedSectionResource.Changed -= RaiseSelectionChanged;
        }
        selectedSectionResource = res as SectionResource;
        selectedSectionResource.Changed += RaiseSelectionChanged;

        if (runUpdates)
        {
            RaiseSelectionChanged();
            MS.RaiseUpdateUI();
        }
    }
    internal void RaiseSelectionChanged()
    {
        //GD.Print($"Selection.Manager::RaiseSelectionChanged()  EVENT!!");

        EventHandler evt = OnSelectionChanged;
        evt?.Invoke(this, EventArgs.Empty);


    }
}// EOF CLASS
