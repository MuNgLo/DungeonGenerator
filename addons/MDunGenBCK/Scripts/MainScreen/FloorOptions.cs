using Godot;
using System;
namespace Munglo.DungeonGenerator.UI
{
	[Tool]
	public partial class FloorOptions : CenterContainer
	{
        private AddonSettingsResource MasterConfig;
        private MainScreen MS;
        private ProfileResource Profile = ResourceLoader.Load("res://addons/MDunGen/Config/def_profile.tres") as ProfileResource;


        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
		{
            MasterConfig = ResourceLoader.Load("res://addons/MDunGen/Config/def_addonconfig.tres") as AddonSettingsResource;
            MS = GetParent<MainScreen>();
            // Spinbox
            (FindChild("SpinBox") as SpinBox).ValueChanged += WhenSpinBoxValueChanged;
            (FindChild("Plus") as TextureButton).Pressed += WhenPlusPressed;
            (FindChild("Minus") as TextureButton).Pressed += WhenMinusPressed;

            (FindChild("EndVisibleFloor") as Label).Text = MasterConfig.maxVisibleFloors.ToString();
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
            // Container for the floor boxes
            HBoxContainer cont = GetNode<HBoxContainer>("HBoxContainer");
            // Get the OG floor box
            TextureButton FloorBox = FindChild("FloorBox") as TextureButton;
            // Get Index of OG box. After this the rest will be inserted/removed
            int startInsertIndex = FloorBox.GetIndex() + 1;
            // Calculate endfloor
            int endfloor = MasterConfig.visibleFloorStart + MasterConfig.maxVisibleFloors - 1;
            (FindChild("EndVisibleFloor") as Label).Text = endfloor.ToString();


            int currentCount = cont.GetChildCount();
            int goalCount = endfloor - MasterConfig.visibleFloorStart + 5 + 1; // 5 extra because not all are floorboxes

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
            GD.Print($"FloorOptions::UpdateUI() startInsertIndex[{startInsertIndex}] currentCount[{currentCount}] goalCount[{goalCount}]");

        }

        private void WhenSpinBoxValueChanged(double value)
        {
            MasterConfig.visibleFloorStart = Mathf.Clamp((int)value, 0, 100);
            ResourceSaver.Save(MasterConfig);
            ShowFloorsNotif();
            UpdateUI();
            MS.ReDrawDungeon();
            (FindChild("Plus") as TextureButton).GrabFocus(); // Block the spinbox lineedit from grabbing keystrokes
        }
        private void WhenPlusPressed()
        {
            MasterConfig.maxVisibleFloors = Mathf.Clamp(MasterConfig.maxVisibleFloors + 1, 1, 10);
            ResourceSaver.Save(MasterConfig);
            ShowFloorsNotif();
            UpdateUI();
            MS.ReDrawDungeon();
        }
        private void WhenMinusPressed()
        {
            MasterConfig.maxVisibleFloors = Mathf.Clamp(MasterConfig.maxVisibleFloors - 1, 1, 10);
            ResourceSaver.Save(MasterConfig);
            ShowFloorsNotif();
            UpdateUI();
            MS.ReDrawDungeon();
        }

        private void ShowFloorsNotif(){
            MS.RaiseNotification($"Showing floor {MasterConfig.visibleFloorStart + 1}" + (MasterConfig.maxVisibleFloors > 1 ? $" through {MasterConfig.visibleFloorStart + MasterConfig.maxVisibleFloors}" : string.Empty));
        }
    }// EOF CLASS
}