using Godot;
using Munglo.DungeonGenerator.UI;
using System;
namespace Munglo.DungeonGenerator.UI
{
    [Tool]
    public partial class Notifications : RichTextLabel
    {
        private double notiffTTL;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            GetParent<MainScreen>().OnNotificationPushed += ScreenNotify;
        }

        public override void _Process(double delta)
        {
            // Notification timer
            if (notiffTTL > 0) { notiffTTL -= delta; if (notiffTTL < 0) { Text = string.Empty; } }
        }
        /// <summary>
        /// Show a message on the bottom of the viewer. Only one can be shown so it overwrites the existing one.
        /// </summary>
        /// <param name="message"></param>
        public void ScreenNotify(object obj, string message)
        {
            Text = "[center]" + message + "[/center]";
            notiffTTL = 1.5f;
            //await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
    }// EOF CLASS
}