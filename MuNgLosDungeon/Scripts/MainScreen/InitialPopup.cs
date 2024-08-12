using Godot;
using System;
namespace Munglo.DungeonGenerator.UI
{
    [Tool]
	public partial class InitialPopup : Control
	{
        public Dungeons addon;
        [Export] private Button changeBtn;
        [Export] private Button closeBtn;
        [Export] private LineEdit pathLine;
        private EditorFileDialog popup;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
		{
			changeBtn.Pressed += WhenChangePressed;
            closeBtn.Pressed += QueueFree;
            pathLine.Text = addon.MasterConfig.ProjectResourcePath;

        }
        private void WhenChangePressed()
        {
            popup = new EditorFileDialog();
            popup.AlwaysOnTop = false;
            popup.Title = "Set Project Resouce Path";
            popup.FileMode = EditorFileDialog.FileModeEnum.OpenDir;
            popup.Access = EditorFileDialog.AccessEnum.Resources;
            popup.PopupWindow = false;
            popup.OkButtonText = "Select Current Folder";

            popup.ResetSize();
            changeBtn.ReleaseFocus();
            EditorInterface.Singleton.GetBaseControl().GetViewport().AddChild(popup);
            popup.MoveToCenter();
            popup.Confirmed += WhenConfirmed;
            popup.Show();
        }
        private void WhenConfirmed()
        {
            popup.Confirmed -= WhenConfirmed;
            addon.MasterConfig.ProjectResourcePath = popup.CurrentPath;
            ResourceSaver.Save(addon.MasterConfig);
            pathLine.Text = addon.MasterConfig.ProjectResourcePath;
            popup.QueueFree();
        }
    }// EOF CLASS
}