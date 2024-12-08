#if TOOLS
using Godot;
using Munglo.DungeonGenerator.UI;

namespace Munglo.DungeonGenerator
{
    [Tool]
    public partial class Dungeons : EditorPlugin
    {
        private VIEWERMODE mode = VIEWERMODE.DUNGEON;
        public VIEWERMODE Mode => mode;
        private readonly string screenName = "Dungeon";
        private AddonSettingsResource masterConfig;
        public AddonSettingsResource MasterConfig => masterConfig;
        private MainScreen screen;
        public MainScreen MS => screen;
        private BottomScreen bscreen;
        private SubViewportContainer subV;
        private CameraControls cam;
        private PackedScene mainPrefab = ResourceLoader.Load<PackedScene>("res://addons/MuNgLosDungeon/Scenes/MainScreen.tscn");
        private PackedScene bottomPrefab = ResourceLoader.Load<PackedScene>("res://addons/MuNgLosDungeon/Scenes/BottomScreen.tscn");
        public ProfileResource Profile = ResourceLoader.Load("res://addons/MuNgLosDungeon/Config/def_profile.tres") as ProfileResource;
        private EditorFileDialog popup;
        #region Overrides
        public override void _EnterTree()
        {
            GD.Print("Loaded MuNgLo's Dungeon Plugin");
            masterConfig = ResourceLoader.Load("res://addons/MuNgLosDungeon/Config/def_addonconfig.tres") as AddonSettingsResource;

            // Centerscreen
            screen = (MainScreen)mainPrefab.Instantiate();
            screen.addon = this;
            // Add screen instance to the editor
            EditorInterface.Singleton.GetEditorMainScreen().AddChild(screen);
            // Hide the main panel. Very much required.
            _MakeVisible(false);

            subV = screen.FindChild("SubViewportContainer") as SubViewportContainer;
            subV.MouseEntered += WhenMouseEnterMain;
            subV.MouseExited += WhenMouseExitMain;

            cam = screen.FindChild("Camera3D") as CameraControls;

            // Hook up mainscreen UI 
            // The other side
            (screen.FindChild("Profile") as TextureButton).Pressed += WhenMSProfilePressed;
            (screen.FindChild("Settings") as TextureButton).Pressed += WhenMSSettingsPressed;
            (screen.FindChild("Biome") as TextureButton).Pressed += WhenMSBiomePressed;
            (screen.FindChild("Export") as TextureButton).Pressed += WhenMSExportPressed;

            //RunDebugTestThings();

            // Bottomscreen
            bscreen = (BottomScreen)bottomPrefab.Instantiate();
            bscreen.addon = this;
            // Add bottom screen instance to the editor
            AddControlToBottomPanel(bscreen, "Dungeon");

        }

        Button testBTN;
        private void RunDebugTestThings()
        {
            testBTN = new Button() { Text = "Debug" };
            AddControlToContainer(CustomControlContainer.SpatialEditorMenu, testBTN);
            testBTN.Pressed += DebugDumpToScene;
        }

        private void DebugDumpToScene()
        {
            PackedScene sceneToSave = new PackedScene();
            //GD.Print($"ExportConfirmed() O1[{screen.CurrentDungeon.GetChildren()[0].Owner}] O2[{screen.CurrentDungeon.GetChildren()[1].Owner}]");

            Node copy = testBTN.GetParent().GetParent().GetParent().GetParent().GetParent();


            Error err = sceneToSave.Pack(copy);
            if (err != Error.Ok)
            {
                GD.PrintErr($"Dungeons::DebugDumpToScene() err[{err}]");
            }
            //sceneToSave.ResourcePath = popup.CurrentPath;
            sceneToSave.ResourcePath = "DebugDump2.tscn";
            ResourceSaver.Save(sceneToSave);
        }

        public override void _ExitTree()
        {
            if (testBTN is not null)
            {
                RemoveControlFromContainer(CustomControlContainer.SpatialEditorMenu, testBTN);
            }
            RemoveControlFromBottomPanel(bscreen);
            GD.Print("Unloaded MuNgLo's Dungeon Plugin");
            subV.MouseEntered -= WhenMouseEnterMain;
            subV.MouseExited -= WhenMouseExitMain;
            // Release mainscreen UI
            // The other Side
            (screen.FindChild("Profile") as TextureButton).Pressed -= WhenMSProfilePressed;
            (screen.FindChild("Settings") as TextureButton).Pressed -= WhenMSSettingsPressed;
            (screen.FindChild("Biome") as TextureButton).Pressed -= WhenMSBiomePressed;
            (screen.FindChild("Export") as TextureButton).Pressed -= WhenMSExportPressed;
        }
        public override bool _HasMainScreen()
        {
            return true;
        }
        public override string _GetPluginName()
        {
            return screenName;
        }
        public override Texture2D _GetPluginIcon()
        {
            Texture2D icon = ResourceLoader.Load("res://addons/MuNgLosDungeon/Icons/AddonIcon.png") as Texture2D;
            return icon;
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
            if (!screen.cursorIsInside) { return; }
            Vector3 inputvector = Vector3.Zero;
            if (Input.IsKeyPressed(Key.W)) { inputvector += Vector3.Forward; }
            if (Input.IsKeyPressed(Key.S)) { inputvector += Vector3.Back; }
            if (Input.IsKeyPressed(Key.A)) { inputvector += Vector3.Left; }
            if (Input.IsKeyPressed(Key.D)) { inputvector += Vector3.Right; }
            if (Input.IsKeyPressed(Key.E)) { inputvector += Vector3.Up; }
            if (Input.IsKeyPressed(Key.Q)) { inputvector += Vector3.Down; }
            //if (Input.IsKeyPressed(Key.Shift)) { screen.shiftIsPressed; }
            inputvector = inputvector.Normalized();
            cam.Inputvector(inputvector);
        }
        public override void _Input(InputEvent @event)
        {
            if (!MS.cursorIsInside)
            {
                if (cam.State == CameraControls.CAMERAMODE.FREELOOK)
                {
                    cam.GoLocked();
                }
                return;
            }
            if (@event is InputEventMouseMotion)
            {
                InputEventMouseMotion m = (InputEventMouseMotion)@event;
                cam.MouseMove(m.Relative);
            }
            if (@event is InputEventMouseButton)
            {
                InputEventMouseButton b = (InputEventMouseButton)@event;

                if (b.ButtonIndex == MouseButton.Left && b.IsPressed())
                {
                    SubViewportContainer cont = screen.GetNode<SubViewportContainer>("SubViewportContainer");


                    if ((cont as SelectOnClick).RayCastToMapPiece(out MapPiece mp))
                    {
                        if (Input.IsKeyPressed(Key.Shift))
                        {
                            MS.Selection.SelectPathTargetMapPiece(mp);
                        }
                        else
                        {
                            MS.Selection.SelectMapPiece(mp);
                        }

                    }
                }

                if (b.ButtonIndex == MouseButton.Right)
                {
                    if (b.Pressed) { cam.GoFreeLook(); }
                    if (b.IsReleased()) { cam.GoLocked(); }
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
        #endregion

        #region Listeners
        /// <summary>
        /// Export Dialougue
        /// </summary>
        private void WhenMSExportPressed()
        {
            (screen.FindChild("Export") as TextureButton).ReleaseFocus();

            popup = new EditorFileDialog();
            popup.AlwaysOnTop = true;
            popup.Title = "Export Dungeon";
            popup.FileMode = EditorFileDialog.FileModeEnum.SaveFile;
            popup.Access = EditorFileDialog.AccessEnum.Resources;
            popup.PopupWindow = true;
            popup.OkButtonText = "Save";
            popup.Confirmed += WhenExportConfirmed;
            //popup.ResetSize();
            popup.Size = screen.GetViewport().GetWindow().Size / 2;
            EditorInterface.Singleton.GetBaseControl().GetViewport().AddChild(popup);
            popup.MoveToCenter();
            popup.Show();
        }
        /// <summary>
        /// Handle the export
        /// </summary>
        private void WhenExportConfirmed()
        {
            popup.Confirmed -= WhenExportConfirmed;
            PackedScene sceneToSave = new PackedScene();
            foreach (Node node in screen.CurrentDungeon.GetChildren())
            {
                SetOwner(screen.CurrentDungeon, node);
            }
            GD.Print($"ExportConfirmed() O1[{screen.CurrentDungeon.GetChildren()[0].Owner}] O2[{screen.CurrentDungeon.GetChildren()[1].Owner}]");
            Error err = sceneToSave.Pack(screen.CurrentDungeon);
            if (err != Error.Ok)
            {
                GD.PrintErr($"Dungeons::WhenExportConfirmed() err[{err}]");
            }
            sceneToSave.ResourcePath = popup.CurrentPath;
            if (!popup.CurrentPath.Contains(".tscn")) { sceneToSave.ResourcePath = sceneToSave.ResourcePath + ".tscn"; }
            ResourceSaver.Save(sceneToSave);
            popup.QueueFree();
        }
        public bool VerifySectionsFolder()
        {
            if (masterConfig.ProjectResourcePath != string.Empty && DirAccess.DirExistsAbsolute(masterConfig.SectionResourcePath))
            {
                return true;
            }
            if (masterConfig.ProjectResourcePath != string.Empty && DirAccess.DirExistsAbsolute(masterConfig.ProjectResourcePath))
            {
                GD.Print("Dungeons:: Creating Sections folder in the project path");
                DirAccess.MakeDirAbsolute(masterConfig.SectionResourcePath);
                EditorInterface.Singleton.GetResourceFilesystem().Scan();
                return DirAccess.DirExistsAbsolute(masterConfig.SectionResourcePath);
            }
            return false;
        }
        private void WhenMSSettingsPressed()
        {
            EditorInterface.Singleton.InspectObject(Profile.settings);
            (screen.FindChild("Settings") as TextureButton).ReleaseFocus();
        }
        private void WhenMSBiomePressed()
        {
            EditorInterface.Singleton.InspectObject(Profile.biome);
            (screen.FindChild("Biome") as TextureButton).ReleaseFocus();
        }
        private void WhenMSProfilePressed()
        {
            EditorInterface.Singleton.InspectObject(Profile);
            (screen.FindChild("Build") as TextureButton).ReleaseFocus();
        }


        private void WhenMouseEnterMain()
        {
            screen.cursorIsInside = true;
        }
        private void WhenMouseExitMain()
        {
            screen.cursorIsInside = false;
        }

        #endregion

        /// <summary>
        /// Sets the owner of the node and all its children recursively 
        /// Skips children of scenbe instances
        /// </summary>
        /// <param name="Owner"></param>
        /// <param name="node"></param>
        private void SetOwner(Node Owner, Node node)
        {
            node.Owner = Owner;
            if (node.SceneFilePath != string.Empty) { return; }
            foreach (Node n in node.GetChildren())
            {
                SetOwner(screen.CurrentDungeon, n);
            }
        }
        public void ChangeMainScreenToDungeon()
        {
            EditorInterface.Singleton.SetMainScreenEditor(screenName);
        }
        /// <summary>
        /// Toggles mode between dungeon and section. Defaults to dungeon.
        /// </summary>
        public void ChangeMode()
        {
            ChangeMode(mode == VIEWERMODE.DUNGEON ? VIEWERMODE.SECTION : VIEWERMODE.DUNGEON);
        }
        public void ChangeMode(VIEWERMODE newMode)
        {
            if (newMode != mode)
            {
                GD.Print($"Dungeons::ChangeMode() changed to {newMode}");
                mode = newMode;
                screen.RaiseUpdateUI();
            }
        }
    }// EOF CLASS
}
#endif
