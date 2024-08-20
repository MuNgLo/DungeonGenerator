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
    public partial class MainScreen : Control
    {
        public Dungeons addon;
        /// <summary>
        /// Use this to only react to input if cursor is over screen
        /// </summary>
        public bool cursorIsInside = false;
        
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
        public EventHandler<string> OnNotificationPushed;
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
            dunVis = GetNode<ScreenDungeonVisualizer>("SubViewportContainer/SubViewport/Dungeon");
            GD.Print($"DirExistsAbs [{DirAccess.DirExistsAbsolute(addon.MasterConfig.ProjectResourcePath)}]");

            (FindChild("MasterConfig") as TextureButton).Pressed += PopupInitialSettingsDialougue;

            if(addon.MasterConfig.ProjectResourcePath == string.Empty || !DirAccess.DirExistsAbsolute(addon.MasterConfig.ProjectResourcePath))
            {
                PopupInitialSettingsDialougue();
            }
            SetDebugLayer(addon.Profile.showDebugLayer);
            RaiseUpdateUI();
        }
        
        private void PopupInitialSettingsDialougue()
        {
            PackedScene pScn = ResourceLoader.Load("res://addons/MuNgLosDungeon/Scenes/InitialPopup.tscn") as PackedScene;
            InitialPopup pop = pScn.Instantiate<InitialPopup>();
            pop.screen = this;
            AddChild(pop);
        }

        public void RaiseUpdateUI()
        {
            GD.Print($"MainScreen::RaiseUpdateUI()  EVENT!!");
            EventHandler evt = OnMainScreenUIUpdate;
            evt?.Invoke(this, EventArgs.Empty);
        }
        public void RaiseNotification(string message)
        {
            EventHandler<string> evt = OnNotificationPushed;
            evt?.Invoke(this, message);
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

        public void GenerateSection(string sectionTypeName, SectionResource sectionDef, GenerationSettingsResource settings, BiomeResource biome)
        {
            RaiseNotification($"Building Section {sectionDef.sectionName}");
            Array<PlacerEntryResource> placers = sectionDef.placers;
            GD.Print($"MainScreen::GenerateSection() defIsNull[{sectionDef is null}] placersisNull[{placers is null}]");
            dunVis.BuildSection(sectionTypeName, sectionDef, placers, settings, biome, ReDrawDungeon);
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
            RaiseNotification("CLEARING...");
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            dunVis.ClearLevel();
            dunVis.ClearLevelDebug();
            ClearSelection();
            RaiseNotification("CLEARED");
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
