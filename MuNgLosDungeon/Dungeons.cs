#if TOOLS
using Godot;
using System;

namespace Munglo.DungeonGenerator
{
    [Tool]
    public partial class Dungeons : EditorPlugin
    {
        private readonly string screenName = "Dungeon";
        private static Control dock;

        PackedScene mainPrefab = ResourceLoader.Load<PackedScene>("res://addons/MuNgLosDungeon/Scenes/MainScreen.tscn");
        MainScreen screen;
        CameraControls cam;

        public override void _EnterTree()
        {
            // Initialization of the plugin goes here.
            GD.Print("Loaded MuNgLo's Dungeon Plugin");
            dock = (Control)(GD.Load("res://addons/MuNgLosDungeon/Scenes/DungeonControls.tscn") as PackedScene).Instantiate();
            AddControlToDock(DockSlot.RightUl, dock);

            // Centerscreen
            screen = (MainScreen)mainPrefab.Instantiate();
            EditorInterface.Singleton.GetEditorMainScreen().AddChild(screen);
            screen.addon = this;
            // Hide the main panel. Very much required.
            _MakeVisible(false);

            SubViewportContainer subV = screen.FindChild("SubViewportContainer") as SubViewportContainer;
            subV.MouseEntered += WhenMouseEnterMain;
            subV.MouseExited += WhenMouseExitMain;

            cam = screen.FindChild("Camera3D") as CameraControls;

            (dock as UIDungeonControls).SetSubViewport(screen);


            // Hook up mainscreen UI
            (screen.FindChild("Build") as TextureButton).Pressed += WhenMSBuildPressed;
            (screen.FindChild("Clear") as TextureButton).Pressed += WhenMSClearPressed;
            (screen.FindChild("Debug") as TextureButton).Pressed += WhenMSDebugPressed;

            (screen.FindChild("RNGSeed") as TextureButton).Pressed += WhenMSRNGSeedPressed;
            (screen.FindChild("Show") as MenuButton).GetPopup().IdPressed += WhenMSShowChanged;

            ProfileResource Profile = ResourceLoader.Load("res://addons/MuNgLosDungeon/Config/def_profile.tres") as ProfileResource;
            if (!Profile.useRandomSeed)
            {
                Texture2D off = ResourceLoader.Load("res://addons/MuNgLosDungeon/Icons/DiceCrossedOutIcon.png") as Texture2D;
                (screen.FindChild("RNGSeed") as TextureButton).TextureNormal = off;
            }

            PopupMenu pop = (screen.FindChild("Show") as MenuButton).GetPopup();
            pop.HideOnCheckableItemSelection = false;
            pop.HideOnItemSelection = false;
            pop.HideOnStateItemSelection = false;
            pop.SetItemChecked(0, Profile.settings.showFloors);
            pop.SetItemChecked(1, Profile.settings.showWalls);
            pop.SetItemChecked(2, Profile.settings.showCeilings);
            pop.SetItemChecked(3, Profile.settings.showProps);
            pop.SetItemChecked(4, Profile.settings.showDebug);

        }

        private void WhenMSShowChanged(long id)
        {

            PopupMenu pop = (screen.FindChild("Show") as MenuButton).GetPopup();
            int index = pop.GetItemIndex((int)id);
            GD.Print($"Dungeons::WhenMSShowChanged() index[{index}]");

            pop.SetItemChecked(index, !pop.IsItemChecked(index));

            ProfileResource Profile = ResourceLoader.Load("res://addons/MuNgLosDungeon/Config/def_profile.tres") as ProfileResource;
            switch (id)
            {
                case 0:
                    Profile.settings.showFloors = pop.IsItemChecked(index);
                    ResourceSaver.Save(Profile.settings);
                    break;
                case 1:
                    Profile.settings.showWalls = pop.IsItemChecked(index);
                    ResourceSaver.Save(Profile.settings);
                    break;
                case 2:
                    Profile.settings.showCeilings = pop.IsItemChecked(index);
                    ResourceSaver.Save(Profile.settings);
                    break;
                case 3:
                    Profile.settings.showProps = pop.IsItemChecked(index);
                    ResourceSaver.Save(Profile.settings);
                    break;
                case 4:
                    Profile.settings.showDebug = pop.IsItemChecked(index);
                    ResourceSaver.Save(Profile.settings);
                    break;
                default:
                    break;
            }

        }

        private void WhenMSRNGSeedPressed()
        {
            TextureButton btn = screen.FindChild("RNGSeed") as TextureButton;
            Texture2D on = ResourceLoader.Load("res://addons/MuNgLosDungeon/Icons/DiceIcon.png") as Texture2D;
            Texture2D off = ResourceLoader.Load("res://addons/MuNgLosDungeon/Icons/DiceCrossedOutIcon.png") as Texture2D;
            ProfileResource Profile = ResourceLoader.Load("res://addons/MuNgLosDungeon/Config/def_profile.tres") as ProfileResource;

            if(btn.TextureNormal.ResourcePath == on.ResourcePath)
            {
                btn.TextureNormal = off;
                Profile.useRandomSeed = false;
                ResourceSaver.Save(Profile);
                return;
            }
            btn.TextureNormal = on;
            Profile.useRandomSeed = true;
            ResourceSaver.Save(Profile);
        }

        private void WhenMSDebugPressed()
        {
            //GD.Print("Dungeons::WhenMSDebugPressed()");
            TextureButton btn = screen.FindChild("Debug") as TextureButton;
            ProfileResource Profile = ResourceLoader.Load("res://addons/MuNgLosDungeon/Config/def_profile.tres") as ProfileResource;
            Profile.showDebugLayer = !Profile.showDebugLayer;
            screen.SetDebugLayer(Profile.showDebugLayer);
            ResourceSaver.Save(Profile);
        }

        private void WhenMSClearPressed()
        {
            GD.Print("Dungeons::WhenMSClearPressed()");
            screen.WhenClearPressed();
        }

        private void WhenMSBuildPressed()
        {
            screen.WhenClearPressed();
            GD.Print("Dungeons::WhenMSBuildPressed()");
            ProfileResource Profile = ResourceLoader.Load("res://addons/MuNgLosDungeon/Config/def_profile.tres") as ProfileResource;
            screen.GenerateDungeon(Profile.settings, Profile.biome);
        }

        public override void _ExitTree()
        {
            SubViewportContainer subV = screen.FindChild("SubViewportContainer") as SubViewportContainer;
            subV.MouseEntered -= WhenMouseEnterMain;
            subV.MouseExited -= WhenMouseExitMain;
            if (dock != null)
            {
                dock.QueueFree();
            }
            dock = null;
            GD.Print("Unloaded MuNgLo's Dungeon Plugin");

            // Release mainscreen UI
            (screen.FindChild("Build") as TextureButton).Pressed -= WhenMSBuildPressed;
            (screen.FindChild("Clear") as TextureButton).Pressed -= WhenMSClearPressed;
            (screen.FindChild("Debug") as TextureButton).Pressed -= WhenMSDebugPressed;

            (screen.FindChild("RNGSeed") as TextureButton).Pressed -= WhenMSRNGSeedPressed;
            (screen.FindChild("Show") as MenuButton).GetPopup().IdPressed -= WhenMSShowChanged;

        }
        public override bool _HasMainScreen()
        {
            return true;
        }
        public void ChangeMainScreenToDungeon()
        {
            EditorInterface.Singleton.SetMainScreenEditor(screenName);
        }
        private void WhenMouseEnterMain()
        {
            screen.isInFocus = true;
        }
        private void WhenMouseExitMain()
        {
            screen.isInFocus = false;
        }
        public override string _GetPluginName()
        {
            return screenName;
        }

        public override Texture2D _GetPluginIcon()
        {
            Texture2D icon = ResourceLoader.Load("res://addons/MuNgLosDungeon/Icons/AddonIcon.png") as Texture2D;
            return icon;
            //return EditorInterface.Singleton.GetEditorTheme().GetIcon("Node", "EditorIcons");
        }

        public override void _MakeVisible(bool visible)
        {
            if (screen != null)
            {
                screen.Visible = visible;
            }
        }
        public override void _Process(double delta)
        {
            if (!screen.isInFocus) { return; }
            Vector3 inputvector = Vector3.Zero;
            //if (Input.IsActionPressed("Forward")) { inputvector += Vector3.Forward; }
            if (Input.IsKeyPressed(Key.W)) { inputvector += Vector3.Forward; }
            if (Input.IsKeyPressed(Key.S)) { inputvector += Vector3.Back; }
            if (Input.IsKeyPressed(Key.A)) { inputvector += Vector3.Left; }
            if (Input.IsKeyPressed(Key.D)) { inputvector += Vector3.Right; }
            if (Input.IsKeyPressed(Key.E)) { inputvector += Vector3.Up; }
            if (Input.IsKeyPressed(Key.Q)) { inputvector += Vector3.Down; }
            inputvector = inputvector.Normalized();
            cam.Inputvector(inputvector);
        }

        public override void _Input(InputEvent @event)
        {
            if (!screen.isInFocus) { return; }
            //if (state != CAMERAMODE.FREELOOK) { return; } // skip if camera is inactive
            if (@event is InputEventMouseMotion)
            {
                InputEventMouseMotion m = (InputEventMouseMotion)@event;
                cam.MouseMove(m.Relative);
                //mVel = m.Relative * mouseSensitivity * 0.1f;
            }
            if (@event is InputEventMouseButton)
            {
                InputEventMouseButton b = (InputEventMouseButton)@event;

                //GD.Print($"GOAL! InputEventMouseButton b.ButtonIndex[{b.ButtonIndex}]");
                if (b.ButtonIndex == MouseButton.Right) {
                    if(b.Pressed) { cam.GoFreeLook(); }
                    if(b.IsReleased()) { cam.GoLocked(); }
                }
                if (b.ButtonIndex == MouseButton.WheelUp)
                {
                    if (b.Pressed) { cam.WheelUp(); }
                }
                if (b.ButtonIndex == MouseButton.WheelDown)
                {
                    if (b.Pressed) { cam.WheelDown(); }
                }
            }
        }

        

    }// EOF CLASS
}
#endif
