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
        /// <summary>
        /// Use this to only react to input if cursor is over screen
        /// </summary>
        public bool cursorIsInside = false;
        public Dungeons addon;
        private Selection.Manager selection;
        private ScreenDungeonVisualizer dunVis;
        

        public Node3D CurrentDungeon => GetNode<Node3D>("SubViewportContainer/SubViewport/Dungeon/Generated");
        public Node3D Gizmos => GetNode<Node3D>("SubViewportContainer/SubViewport/Gizmos");

        internal EventHandler OnMainScreenUIUpdate;
        internal EventHandler<string> OnNotificationPushed;
    
        internal Selection.Manager Selection => selection;


        public override void _Ready()
        {
            dunVis = GetNode<ScreenDungeonVisualizer>("SubViewportContainer/SubViewport/Dungeon");
            GD.Print($"DirExistsAbs [{DirAccess.DirExistsAbsolute(addon.MasterConfig.ProjectResourcePath)}]");

            (FindChild("MasterConfig") as TextureButton).Pressed += PopupInitialSettingsDialougue;

            if(addon.MasterConfig.ProjectResourcePath == string.Empty || !DirAccess.DirExistsAbsolute(addon.MasterConfig.ProjectResourcePath))
            {
                PopupInitialSettingsDialougue();
            }
            selection = new Selection.Manager(addon, this, dunVis);
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
                    selection.SelectFirstSection();
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
            selection.ClearSelection();
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
