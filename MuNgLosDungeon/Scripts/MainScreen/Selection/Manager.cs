using Godot;
using Godot.Collections;
using System;
using System.Reflection;

namespace Munglo.DungeonGenerator.UI.Selection
{
    internal class Manager
    {
        internal Dungeons addon;

        private MainScreen MS;
        private ScreenDungeonVisualizer dunVis;

        private PackedScene gizmoDiamond;
        private PackedScene gizmoSectionBox;

        private MapPiece selectedMapPiece;
        private ISection selectedSection;
        private SectionResource selectedSectionResource;
        internal MapPiece SelectedMapPiece => selectedMapPiece;
        internal ISection SelectedSection => selectedSection;
        internal SectionResource SelectedSectionResource => selectedSectionResource;
        internal EventHandler OnSelectionChanged;

        internal Manager(Dungeons dun, MainScreen ms, ScreenDungeonVisualizer dv)
        {
            addon = dun;
            MS = ms;
            dunVis = dv;
            gizmoDiamond = ResourceLoader.Load("res://addons/MuNgLosDungeon/Scenes/Gizmos/diamond.tscn") as PackedScene;
            gizmoSectionBox = ResourceLoader.Load("res://addons/MuNgLosDungeon/Scenes/Gizmos/sectionbox.tscn") as PackedScene;
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
            // Insert sectionbox
            if(selectedSection is not null){
                Node3D min = gizmoSectionBox.Instantiate() as Node3D;
                Node3D max = gizmoSectionBox.Instantiate() as Node3D;
                MS.Gizmos.AddChild(min);
                MS.Gizmos.AddChild(max);
                min.Position = Dungeon.GlobalPosition(selectedSection.MinCoord);
                max.Position = Dungeon.GlobalPosition(selectedSection.MaxCoord);
            }
        }

        internal void ClearSelection()
        {
            selectedMapPiece = null;
            selectedSection = null;
            if (addon.Mode != VIEWERMODE.SECTION) { selectedSectionResource = null; }
        }
        internal void SelectFirstSection(bool runUpdates = true)
        {
            GD.Print($"MainScreen::SelectFirstSection()");
            ScreenDungeonVisualizer vis = MS.FindChild("Dungeon") as ScreenDungeonVisualizer;
            if (vis.Map.Sections.Count < 1) { return; }
            SelectSection(0);
        }
        internal void SelectMapPiece(MapPiece mapPiece, bool runUpdates = true)
        {
            if (mapPiece == null) { return; }
            selectedMapPiece = mapPiece;
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
            GD.Print($"MainScreen::SelectSectionResource() SectionResource is [{res.ResourcePath}]");

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
            GD.Print($"MainScreen::RaiseSelectionChanged()  EVENT!!");

            EventHandler evt = OnSelectionChanged;
            evt?.Invoke(this, EventArgs.Empty);


        }
    }// EOF CLASS
}