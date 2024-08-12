using Godot;
using System;
namespace Munglo.DungeonGenerator.UI
{
	[Tool]
	public partial class FloorOptions : CenterContainer
	{
        private AddonSettingsResource MasterConfig;
        private MainScreen MS;
        private ProfileResource Profile = ResourceLoader.Load("res://addons/MuNgLosDungeon/Config/def_profile.tres") as ProfileResource;


        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
		{
            MasterConfig = ResourceLoader.Load("res://addons/MuNgLosDungeon/Config/def_addonconfig.tres") as AddonSettingsResource;
            MS = GetParent<MainScreen>();
            // Spinbox
            (FindChild("SpinBox") as SpinBox).ValueChanged += WhenSpinBoxValueChanged;
            (FindChild("Plus") as TextureButton).Pressed += WhenPlusPressed;
            (FindChild("Minus") as TextureButton).Pressed += WhenMinusPressed;

            (FindChild("Label") as Label).Text = MasterConfig.maxVisibleFloors.ToString();
        }
        public override void _ExitTree()
        {
            // Floor selector
            (FindChild("SpinBox") as SpinBox).ValueChanged -= WhenSpinBoxValueChanged;
            (FindChild("Plus") as TextureButton).Pressed -= WhenPlusPressed;
            (FindChild("Minus") as TextureButton).Pressed -= WhenMinusPressed;
        }

        private void UpdateUI()
        {
            int cubes = MasterConfig.visibleFloorStart + MasterConfig.maxVisibleFloors;
            (FindChild("Label") as Label).Text = (cubes - 1).ToString();
            HBoxContainer cont = GetNode<HBoxContainer>("HBoxContainer");
            TextureButton FloorBox = FindChild("FloorBox") as TextureButton;

            int startInsertIndex = FloorBox.GetIndex() + 1;

            int currentCount = cont.GetChildCount();
            int goalCount = cubes + 5;

            if(currentCount > goalCount)
            {
                // Subtract
                for (int i = 0; i < currentCount - goalCount; i++)
                {
                    cont.GetChild(startInsertIndex + i).QueueFree();
                }
            }
            if (currentCount < goalCount)
            {
                // Add
                for (int i = 0; i < goalCount - currentCount; i++)
                {
                    TextureButton copy = FloorBox.Duplicate() as TextureButton;
                    cont.AddChild(copy);
                    cont.MoveChild(copy, startInsertIndex + i);
                }
            }
        }

        private void WhenSpinBoxValueChanged(double value)
        {
            MasterConfig.maxVisibleFloors = Mathf.Clamp(MasterConfig.maxVisibleFloors, 1, 10);
            MasterConfig.visibleFloorStart = Mathf.Clamp((int)value, 0, 100);
            GD.Print($"FloorOptions::WhenSpinBoxValueChanged({value}) as int[{(int)value}] MasterConfig.visibleFloorStart[{MasterConfig.visibleFloorStart}] MasterConfig.maxVisibleFloors[{MasterConfig.maxVisibleFloors}]");
            ResourceSaver.Save(MasterConfig);
            UpdateUI();
            MS.RaiseNotification($"Showing floor {MasterConfig.visibleFloorStart}" + (MasterConfig.maxVisibleFloors > 1 ? $" through {MasterConfig.visibleFloorStart + MasterConfig.maxVisibleFloors - 1}" : string.Empty));
            MS.ReDrawDungeon();
            (FindChild("Plus") as TextureButton).GrabFocus(); // Block the spinbox lineedit from grabbing keystrokes
        }
        private void WhenPlusPressed()
        {
            MasterConfig.maxVisibleFloors = Mathf.Clamp(MasterConfig.maxVisibleFloors + 1, 1, 10);
            ResourceSaver.Save(MasterConfig);
            UpdateUI();
            MS.RaiseNotification($"Showing floor {MasterConfig.visibleFloorStart}" + (MasterConfig.maxVisibleFloors > 1 ? $" through {MasterConfig.visibleFloorStart + MasterConfig.maxVisibleFloors - 1}" : string.Empty));
            MS.ReDrawDungeon();
        }
        private void WhenMinusPressed()
        {
            MasterConfig.maxVisibleFloors = Mathf.Clamp(MasterConfig.maxVisibleFloors - 1, 1, 10);
            ResourceSaver.Save(MasterConfig);
            UpdateUI();
            MS.RaiseNotification($"Showing floor {MasterConfig.visibleFloorStart}" + (MasterConfig.maxVisibleFloors > 1 ? $" through {MasterConfig.visibleFloorStart + MasterConfig.maxVisibleFloors - 1}" : string.Empty));
            MS.ReDrawDungeon();
        }
    }// EOF CLASS
}