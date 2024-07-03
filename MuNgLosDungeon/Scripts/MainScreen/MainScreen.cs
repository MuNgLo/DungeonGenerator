using Godot;
using System;

namespace Munglo.DungeonGenerator
{
    [Tool]
    internal partial class MainScreen : Control
    {
        public Dungeons addon;
        public bool isInFocus = false;
        private RichTextLabel notiff;
        private double notiffTTL;
        private ScreenDungeonVisulaizer dunVis;
        public Node3D CurrentDungeon => GetNode<Node3D>("SubViewportContainer/SubViewport/Dungeon/Generated");
        public override void _Ready()
        {
            notiff = FindChild("Notification") as RichTextLabel;
            //Node3D dun = FindChild("Dungeon") as Node3D;

            dunVis = GetNode<ScreenDungeonVisulaizer>("SubViewportContainer/SubViewport/Dungeon");

        }

        public override void _Process(double delta)
        {
            if(notiffTTL> 0) { notiffTTL -= delta; if (notiffTTL < 0) { notiff.Text = string.Empty; } }
        }
     
        public void ScreenNotify(string message)
        {
            notiff.Text= "[center]" + message + "[/center]";
            notiffTTL = 2.0;
        }
        public void GenerateDungeon(GenerationSettingsResource settings, BiomeDefinition biome)
        {
            addon.ChangeMainScreenToDungeon();
            dunVis.BuildDungeon(settings, biome);
        }

        internal void WhenClearPressed()
        {
            dunVis.ClearLevel();
            dunVis.ClearLevelDebug();
            //mapGen.ClearNavMesh();
        }

        internal void SetDebugLayer(bool state)
        {
            dunVis.SetDebugLayer(state);
        }
    }// EOF CLASS
}
