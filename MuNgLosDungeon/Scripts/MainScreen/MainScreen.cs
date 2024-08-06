using Godot;
using Godot.Collections;
using System;
using System.Reflection;

namespace Munglo.DungeonGenerator.UI
{
    /// <summary>
    /// The mainscreen window center editor to generate/view dungeon data
    /// </summary>
    [Tool]
    internal partial class MainScreen : Control
    {
        public Dungeons addon;
        /// <summary>
        /// Use this to only react to input if cursor is over screen
        /// </summary>
        public bool cursorIsInside = false;
        private RichTextLabel notiff;
        private double notiffTTL;
        private ScreenDungeonVisualizer dunVis;
        public Node3D CurrentDungeon => GetNode<Node3D>("SubViewportContainer/SubViewport/Dungeon/Generated");

        public EventHandler OnMainScreenUIUpdate;


        #region selection related
        private MapPiece selectedMapPiece;
        private ISection selectedSection;
        private SectionResource selectedSectionResource;
        public MapPiece SelectedMapPiece => selectedMapPiece;
        public ISection SelectedSection => selectedSection;
        public SectionResource SelectedSectionResource => selectedSectionResource;
        public EventHandler OnSelectionChanged;
        public void ClearSelection()
        {
            selectedMapPiece = null;
            selectedSection = null;
            if (addon.Mode != VIEWERMODE.SECTION) { selectedSectionResource = null; }
        }
        public void SelectFirstSection(bool runUpdates = true)
        {
            GD.Print($"MainScreen::SelectFirstSection()");
            ScreenDungeonVisualizer vis = FindChild("Dungeon") as ScreenDungeonVisualizer;
            if (vis.Map.Sections.Count < 1) { return; }
            SelectSection(0);
        }
        public void SelectMapPiece(MapPiece mapPiece, bool runUpdates = true)
        {
            if (mapPiece == null) { return; }
            selectedMapPiece= mapPiece;
            SelectSection(mapPiece.SectionIndex, false);
            if (runUpdates)
            {
                RaiseSelectionChanged();
                RaiseUpdateUI();
            }
        }
        public void SelectSection(int sectionIndex, bool runUpdates = true)
        {
            if (sectionIndex < 0) { return; }
            selectedSection = dunVis.Map.Sections[sectionIndex];

            if (runUpdates)
            {
                RaiseSelectionChanged();
                RaiseUpdateUI();
            }
        }
        public void SelectSectionResource(Resource res, bool runUpdates = true)
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
                RaiseUpdateUI();
            }
        }
        public void RaiseSelectionChanged()
        {
            GD.Print($"MainScreen::RaiseSelectionChanged()  EVENT!!");

            EventHandler evt = OnSelectionChanged;
            evt?.Invoke(this, EventArgs.Empty);

            if (SelectedMapPiece is not null)
            {
                GD.Print($"SELECTED => MapPiece[{SelectedMapPiece.Coord}] section[{SelectedMapPiece.SectionIndex}] floor[{SelectedMapPiece.hasFloor}] bridge[{SelectedMapPiece.isBridge}] stair[{SelectedMapPiece.hasStairs}]");

            }
            if (selectedSection is not null)
            {
                GD.Print($"Section[{SelectedSection.SectionIndex}] has [{SelectedSection.ConnectionCount}] connections. Section Min/Max [{SelectedSection.MinCoord} / {SelectedSection.MaxCoord}]");
            }
        }
        #endregion



        public override void _Ready()
        {
            notiff = FindChild("Notification") as RichTextLabel;
            dunVis = GetNode<ScreenDungeonVisualizer>("SubViewportContainer/SubViewport/Dungeon");
        }


        public override void _Process(double delta)
        {
            // Notification timer
            if(notiffTTL> 0) { notiffTTL -= delta; if (notiffTTL < 0) { notiff.Text = string.Empty; } }
        }

        public void RaiseUpdateUI()
        {
            GD.Print($"MainScreen::RaiseUpdateUI()  EVENT!!");
            EventHandler evt = OnMainScreenUIUpdate;
            evt?.Invoke(this, EventArgs.Empty);
        }

 

        /// <summary>
        /// Show a message on the bottom of the viewer. Only one can be shown so it overwrites the existing one.
        /// </summary>
        /// <param name="message"></param>
        public async void ScreenNotify(string message)
        {
            notiff.Text= "[center]" + message + "[/center]";
            notiffTTL = 2.0;
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
        /// <summary>
        /// Generates and display a dungeon in the viewer
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="biome"></param>
        public void GenerateDungeon(GenerationSettingsResource settings, BiomeResource biome)
        {
            dunVis.BuildDungeon(settings, biome);
        }

        public void GenerateSection(SectionResource sectionDef, GenerationSettingsResource settings, BiomeResource biome)
        {
            OptionButton btn = this.GetNode<OptionButton>("ModeSelector");
            EditorResourcePicker picker = this.GetNode<EditorResourcePicker>("PlacerResouceSelector");

            Array<PlacerEntryResource> placers = sectionDef.placers;
            if(picker.EditedResource is not null)
            {
                placers = new Array<PlacerEntryResource>() { picker.EditedResource as PlacerEntryResource };
            }

            GD.Print($"MainScreen::GenerateSection() defIsNull[{sectionDef is null}] placersisNull[{placers is null}]");

            dunVis.BuildSection(btn.GetItemText(btn.Selected), sectionDef, placers, settings, biome, ReDrawDungeon);
        }

        public void ReDrawDungeon()
        {
            switch (addon.Mode)
            {
                case VIEWERMODE.SECTION:
                    dunVis.ReDrawSection();
                    SelectFirstSection();
                    break;
                case VIEWERMODE.DUNGEON:
                default:
                    dunVis.ReDrawMap();
                    break;
            }
        }
        /// <summary>
        /// Clears existing dungoen that is being viewed
        /// </summary>
        internal async void WhenClearPressed()
        {
            ScreenNotify("CLEARING...");
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            dunVis.ClearLevel();
            dunVis.ClearLevelDebug();
            ClearSelection();
            ScreenNotify("CLEARED");
        }
        /// <summary>
        /// Sets the state of the debug information
        /// </summary>
        /// <param name="state"></param>
        internal void SetDebugLayer(bool state)
        {
            dunVis.SetDebugLayer(state);
        }

    
    }// EOF CLASS
}
