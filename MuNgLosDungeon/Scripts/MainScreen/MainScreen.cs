using Godot;
using System;

namespace Munglo.DungeonGenerator
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

        public void GenerateSection(RoomResource sectionDef, GenerationSettingsResource settings, BiomeResource biome)
        {
            OptionButton btn = this.GetNode<OptionButton>("ModeSelector");
            EditorResourcePicker picker = this.GetNode<EditorResourcePicker>("PlacerResouceSelector");

            PlacerResource[] placers = null;
            if(picker.EditedResource is not null)
            {
                placers = new PlacerResource[1] {picker.EditedResource as PlacerResource};
            }

            dunVis.BuildSection(btn.GetItemText(btn.Selected), sectionDef, placers, settings, biome, ReDrawDungeon);
        }
        public void ReDrawDungeon()
        {
            switch (addon.Mode)
            {
                case VIEWERMODE.SECTION:
                    dunVis.ReDrawSection();
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
            //mapGen.ClearNavMesh();
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
