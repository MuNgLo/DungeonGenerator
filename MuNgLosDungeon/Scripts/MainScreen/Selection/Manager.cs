using Godot;
using System;
using Munglo.DungeonGenerator.Gizmos;
using System.Linq;
using Munglo.DungeonGenerator.Pathfinding;
using System.Collections.Generic;

namespace Munglo.DungeonGenerator.UI.Selection;

internal class Manager
{
    internal EventHandler OnSelectionChanged;

    private Dungeons addon;
    private MainScreen MS;
    private ScreenDungeonVisualizer dunVis;
    private PackedScene gizmoDiamond;
    private PackedScene gizmoConnParent;
    private PackedScene gizmoConnChild;
    private PackedScene gizmoSegmented;
    private MapPiece selectedMapPiece;
    private MapPiece selectedMapPiece2nd;
    private ISection selectedSection;
    /// <summary>
    /// Should only have 3 refs. one to NULL in the event raiser, one in SelectConnection() and one for the SelectedConnection property
    /// </summary>
    private SectionConnection selectedConnection;
    private SectionResource selectedSectionResource;


    internal MapPiece SelectedMapPiece => selectedMapPiece;
    internal ISection SelectedSection => selectedSection;
    internal SectionConnection SelectedConnection => selectedConnection;
    internal SectionResource SelectedSectionResource => selectedSectionResource;
    internal PackedScene GizmoSegmented => gizmoSegmented;

    internal Manager(Dungeons dun, MainScreen ms, ScreenDungeonVisualizer dv)
    {
        addon = dun;
        MS = ms;
        dunVis = dv;
        gizmoDiamond = ResourceLoader.Load("res://addons/MuNgLosDungeon/Scenes/Gizmos/diamond.tscn") as PackedScene;
        gizmoConnParent = ResourceLoader.Load("res://addons/MuNgLosDungeon/Scenes/Gizmos/connparent.tscn") as PackedScene;
        gizmoConnChild = ResourceLoader.Load("res://addons/MuNgLosDungeon/Scenes/Gizmos/connchild.tscn") as PackedScene;
        gizmoSegmented = ResourceLoader.Load("res://addons/MuNgLosDungeon/Scenes/Gizmos/segmented.tscn") as PackedScene;
        MS.OnMainScreenUIUpdate += UpdateGizmos;
    }
    private void RaiseSelectionChanged()
    {
        // If selected mappiece is part of a connection. Also debug that
        if (selectedMapPiece is not null && addon.MS.Map.GetConnection(SelectedMapPiece.SectionIndex, SelectedMapPiece.Coord, out SectionConnection conn))
        {
            if (conn.connectionID != -1) { SelectConnection(conn); }
        }
        else
        {
            selectedConnection = null;
        }
        EventHandler evt = OnSelectionChanged;
        evt?.Invoke(this, EventArgs.Empty);
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
        }
        // Insert connection gizmos
        if (selectedConnection is not null)
        {
            // Add debug paths to the connection
            for (int i = 0; i < selectedConnection.ConnectedLocations.Count; i++)
            {
                ConnectedLocation loc = selectedConnection.ConnectedLocations[i];
                if (loc.section == selectedConnection.sectionID)
                {
                    DrawPath(selectedConnection.coord, loc.coord, Colors.DodgerBlue, selectedConnection.sectionID);
                }
            }
        }
        // Build a path query between selected map pieces
        if (selectedMapPiece is not null && selectedMapPiece2nd is not null)
        {
            PathQuery query = new PathQuery(addon.MS.Map, selectedMapPiece, selectedMapPiece2nd);
            if (!query.IsValid)
            {
                Godot.GD.PushError($"Manager::UpdateGizmos() query was invalid!");
                return;
            }
            Pathing.FindPath(query, AddGizmoForAnswer);
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
            AddGizmoForSectionConnections(selectedSection);
            if (selectedSection.SectionStyle == "Connector")
            {
                AddGizmoMapPieceBox(selectedSection.Pieces.First().Coord);
                AddGizmoMapPieceBox(selectedSection.Pieces.Last().Coord);
                return;
            }
            AddGizmoSection(selectedSection);
        }
    }
    #region Selection related
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
        //GD.Print($"Selection.Manager::SelectPathTargetMapPiece()");
        if (mapPiece == null || selectedMapPiece is null) { return; }
        selectedMapPiece2nd = mapPiece;
        if (runUpdates)
        {
            RaiseSelectionChanged();
            MS.RaiseUpdateUI();
        }
    }
    private double randmonSelectionTimestamp = 0.0;
    internal void SelectRandomPiecesForPathing(float timeRandomSelectionIsBlocked)
    {
        if(randmonSelectionTimestamp > Time.GetTicksMsec()) {return;}
        randmonSelectionTimestamp = Time.GetTicksMsec() + timeRandomSelectionIsBlocked * 1000.0;
        MapPiece mp1 = MS.Map.GetRandomPieceEditor();
        MapPiece mp2 = MS.Map.GetRandomPieceEditor();
        SelectMapPiece(mp1,false);
        SelectPathTargetMapPiece(mp2);
    }
    internal void SelectMapPiece(MapPiece mapPiece, bool runUpdates = true)
    {
        if (mapPiece == null) { return; }
        selectedMapPiece = mapPiece;
        selectedMapPiece2nd = null; // NOTE Do we haff to?
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
    private void SelectConnection(SectionConnection conn)
    {
        selectedConnection = conn.connectionID != -1 ? conn : null;


    }
    #endregion
    #region Draw methods to add a group of Gizmos
    private void DrawPath(MapCoordinate start, MapCoordinate end, Color col, int sectionIndex)
    {
        ISection section = MS.Map.Sections[sectionIndex];
        if (!section.ContainsPiece(end))
        {
            return;
        }
        if (!section.ContainsPiece(end))
        {
            return;
        }
        MapPiece mpStart = addon.MS.Map.GetExistingPiece(start);
        MapPiece mpEnd = addon.MS.Map.GetExistingPiece(end);
        PathQuery query = new PathQuery(addon.MS.Map, mpStart, mpEnd);
        query.OverrideSections(sectionIndex, sectionIndex);
        if (!query.IsValid)
        {
            Godot.GD.PushError($"Selection.Manager::DrawPath() query was invalid!");
            return;
        }
        Pathing.FindPath(query, (PathAnswer answer) => { AddGizmoForAnswer(answer); });
    }
    private void DrawPathBetweenConnections(int connID1, int connID2, Color col)
    {
        if (connID1 == -1 || connID2 == -1) { return; }
        if (addon.MS.Map.Connections.ContainsKey(connID1) && addon.MS.Map.Connections.ContainsKey(connID2))
        {
            SectionConnection conn1 = addon.MS.Map.Connections[connID1];
            SectionConnection conn2 = addon.MS.Map.Connections[connID2];
            if (conn1.sectionID != conn2.sectionID) { return; }
            DrawPath(conn1.coord, conn2.coord, col, conn1.sectionID);
        }
    }
    #endregion
    #region Add single Gizmo
    internal void
    AddGizmoForAnswer(PathAnswer answer)
    {
        AddGizmoForPath(answer.path, Colors.DarkGreen);
        AddGizmoForSectionPath(answer.startConnectionn, answer.endConnection, answer.connectionPath, Colors.LimeGreen);
    }
    private void AddGizmoForPath(PathAnswer answer, Color col) { AddGizmoForPath(answer.path, col); }
    private void AddGizmoForPath(List<PathLocation> path, Color col)
    {
        if (path.Count > 0)
        {
            SegmentedGizmo pathy = GizmoSegmented.Instantiate() as SegmentedGizmo;
            pathy.pathScale = 1.0f;
            pathy.offset = Vector3.Up * 0.5f;
            pathy.color = col;
            pathy.ClearSegments();
            pathy.AddSegments(path);
            addon.MS.Gizmos.AddChild(pathy);
            pathy.Position = Vector3.Zero;
        }
    }
    private void AddGizmoForSectionPath(SectionConnection startConnection, SectionConnection endConnection, List<int> connectionPath, Color col)
    {
        if (connectionPath.Count > 0)
        {
            for (int i = 0; i < connectionPath.Count - 1; i++)
            {
                int connID1 = connectionPath[i];
                int connID2 = connectionPath[i+1];
                SectionConnection conn1 = connID1 == -1 ? startConnection : addon.MS.Map.Connections[connID1];
                SectionConnection conn2 = connID2 == -2 ? endConnection : addon.MS.Map.Connections[connID2];
                DrawPath(conn1.coord, conn2.coord, col, conn1.sectionID);
            }
        }
    }
    private void AddGizmoForSectionConnections(ISection section)
    {
        // Put arrows on connection tiles
        foreach (int conKey in section.Connections)
        {
            if (!dunVis.Map.Connections.ContainsKey(conKey))
            {
                GD.PushError($"Selection > Manager::UpdateGizmos() connection key was missing in MapData!");
                return;
            }
            SectionConnection connection = dunVis.Map.Connections[conKey];
            // Parent side
            Node3D d = gizmoConnParent.Instantiate() as Node3D;
            MS.Gizmos.AddChild(d);
            d.Position = Dungeon.GlobalPosition(connection.coord);
            d.GlobalRotationDegrees = Dungeon.ResolveRotation(Dungeon.Flip(connection.Dir));
        }
    }
    private void AddGizmoMapPieceBox(MapCoordinate coord)
    {
        Node3D segmented = gizmoSegmented.Instantiate() as Node3D;
        MS.Gizmos.AddChild(segmented);
        segmented.Position = Dungeon.GlobalPosition(coord);
        Vector3[] mapPieceBox = BuildMapPieceBox();
        (segmented as SegmentedGizmo).ClearSegments();
        (segmented as SegmentedGizmo).AddSegments(mapPieceBox);
    }
    internal void AddGizmoSection(int sectionID)
    {
        AddGizmoSection(dunVis.Map.Sections[sectionID]);
    }
    internal void AddGizmoSection(ISection section)
    {
        if (section.SectionStyle == "Connector")
        {
            AddGizmoMapPieceBox(section.Pieces.First().Coord);
            AddGizmoMapPieceBox(section.Pieces.Last().Coord);
            return;
        }
        Node3D segmented = gizmoSegmented.Instantiate() as Node3D;
        MS.Gizmos.AddChild(segmented);
        segmented.Position = Dungeon.GlobalPosition(section.MinCoord);
        Vector3[] mapPieceBox = BuildMapPieceBox(section.MaxCoord - section.MinCoord);
        (segmented as SegmentedGizmo).ClearSegments();
        (segmented as SegmentedGizmo).AddSegments(mapPieceBox);
    }
    #endregion
    #region Builder of gizmos
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

    #endregion
}// EOF CLASS
