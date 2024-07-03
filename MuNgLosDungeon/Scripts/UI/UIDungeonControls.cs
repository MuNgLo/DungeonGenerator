#if TOOLS
using Godot;
using Munglo.DungeonGenerator;
using System;

[Tool]
public partial class UIDungeonControls : GridContainer
{
    private MainScreen screen;
    internal GenerationSettingsResource Settings => (FindChild("SettingsSelector") as EditorResourcePicker).EditedResource is not null ? settingsSelector.EditedResource as GenerationSettingsResource : null;
    private AddonSettings Config;
    private EditorResourcePicker profileSelector;
    private Callable whenProfileResourceChanged;
    private ProfileResource Profile => profileSelector.EditedResource as ProfileResource;

    private EditorResourcePicker settingsSelector;
    private Callable whenSettingsResourceChanged;

    private EditorResourcePicker biomeSelector;
    private Callable whenBiomeResourceChanged;


    private LineEdit seedIn1;
    private LineEdit seedIn2;
    private LineEdit seedIn3;
    private LineEdit seedIn4;
    #region Callables
    private Callable whenSeedRNGToggle;
    private Callable whenSetupButtonPressed;
    private Callable whenBuildButtonPressed;
    private Callable whenClearButtonPressed;
    private Callable whenDebugToggleButtonPressed;
    // Seed 1
    private Callable seed1Submit;
    private Callable seed1FocusEntered;
    private Callable seed1FocusExited;
    // Seed 2
    private Callable seed2Submit;
    private Callable seed2FocusEntered;
    private Callable seed2FocusExited;
    // Seed 3
    private Callable seed3Submit;
    private Callable seed3FocusEntered;
    private Callable seed3FocusExited;
    // Seed 4
    private Callable seed4Submit;
    private Callable seed4FocusEntered;
    private Callable seed4FocusExited;
    #endregion
    public override void _EnterTree()
    {
        Config = ResourceLoader.Load("res://addons/MuNgLosDungeon/Config/def_addonconfig.tres") as AddonSettings;
        profileSelector = FindChild("ProfileSelector") as EditorResourcePicker;
        settingsSelector = FindChild("SettingsSelector") as EditorResourcePicker;
        biomeSelector = FindChild("BiomesSelector") as EditorResourcePicker;
    }
    public override void _Ready()
	{
        whenProfileResourceChanged = new Callable(this, "WhenProfileChanged"); 
        profileSelector.Connect("resource_changed", whenProfileResourceChanged);

        whenSettingsResourceChanged = new Callable(this, "WhenSettingsChanged");
        settingsSelector.Connect("resource_changed", whenSettingsResourceChanged);

        whenBiomeResourceChanged = new Callable(this, "WhenBiomeChanged");
        biomeSelector.Connect("resource_changed", whenBiomeResourceChanged);

        #region Seed Values related
        seedIn1 = FindChild("SeedIn1") as LineEdit;
        seed1Submit = new Callable(this, "Seed1Submit");
        seed1FocusEntered = new Callable(this, "Seed1FocusEntered");
        seed1FocusExited = new Callable(this, "Seed1FocusExit");
        seedIn1.Connect("text_submitted", seed1Submit); 
        seedIn1.Connect(SignalName.FocusEntered, seed1FocusEntered);
        seedIn1.Connect(SignalName.FocusExited, seed1FocusExited);

        seedIn2 = FindChild("SeedIn2") as LineEdit;
        seed2Submit = new Callable(this, "Seed2Submit");
        seed2FocusEntered = new Callable(this, "Seed2FocusEntered");
        seed2FocusExited = new Callable(this, "Seed2FocusExit");
        seedIn2.Connect("text_submitted", seed2Submit);
        seedIn2.Connect(SignalName.FocusEntered, seed2FocusEntered);
        seedIn2.Connect(SignalName.FocusExited, seed2FocusExited);

        seedIn3 = FindChild("SeedIn3") as LineEdit;
        seed3Submit = new Callable(this, "Seed3Submit");
        seed3FocusEntered = new Callable(this, "Seed3FocusEntered");
        seed3FocusExited = new Callable(this, "Seed3FocusExit");
        seedIn3.Connect("text_submitted", seed3Submit);
        seedIn3.Connect(SignalName.FocusEntered, seed3FocusEntered);
        seedIn3.Connect(SignalName.FocusExited, seed3FocusExited);

        seedIn4 = FindChild("SeedIn4") as LineEdit;
        seed4Submit = new Callable(this, "Seed4Submit");
        seed4FocusEntered = new Callable(this, "Seed4FocusEntered");
        seed4FocusExited = new Callable(this, "Seed4FocusExit");
        seedIn4.Connect("text_submitted", seed4Submit);
        seedIn4.Connect(SignalName.FocusEntered, seed4FocusEntered);
        seedIn4.Connect(SignalName.FocusExited, seed4FocusExited);
        #endregion
        CheckBox seedToggleBox = FindChild("SeedRNGToggle") as CheckBox;
        whenSeedRNGToggle = new Callable(this, "WhenSeedRNGToggled");
        seedToggleBox.Connect("toggled", whenSeedRNGToggle);
        Button buildBtn = FindChild("Build") as Button;
        whenBuildButtonPressed = new Callable(this, "WhenBuildPressed");
        buildBtn.Connect("pressed", whenBuildButtonPressed);
        Button clearBtn = FindChild("Clear") as Button;
        whenClearButtonPressed = new Callable(this, "WhenClearPressed");
        clearBtn.Connect("pressed", whenClearButtonPressed);
        Button setupBtn = FindChild("Setup") as Button;
        whenSetupButtonPressed = new Callable(this, "WhenSetupPressed");
        setupBtn.Connect("pressed", whenSetupButtonPressed);

        Button dDebugToggleBtn = FindChild("DebugToggle") as Button;
        whenDebugToggleButtonPressed = new Callable(this, "WhenDebugTogglePressed");
        dDebugToggleBtn.Connect("pressed", whenDebugToggleButtonPressed);

        ProfileResource profile = ResourceLoader.Load(Config.lastUsedProfile) as ProfileResource;
        profileSelector.EditedResource = profile;
        settingsSelector.EditedResource = profile.settings;
        biomeSelector.EditedResource = profile.biome;

        UpdateSeeds();
        GetNode<UITabShow>("TabContainer/Show").UpdateVisibleValues();
        GetNode<UITabPasses>("TabContainer/Passes").UpdateVisibleValues();
        GetNode<UITabCorridors>("TabContainer/Corridors").UpdateVisibleValues();
        GetNode<UITabRooms>("TabContainer/Rooms").UpdateVisibleValues();
    }

   

    public override void _ExitTree()
    {
        if(profileSelector.IsConnected("resource_changed", whenProfileResourceChanged)) { profileSelector.Disconnect("resource_changed", whenProfileResourceChanged); }
        if(settingsSelector.IsConnected("resource_changed", whenSettingsResourceChanged)) { settingsSelector.Disconnect("resource_changed", whenSettingsResourceChanged); }
        if(biomeSelector.IsConnected("resource_changed", whenBiomeResourceChanged)) { biomeSelector.Disconnect("resource_changed", whenBiomeResourceChanged); }
        if (seedIn1.IsConnected("text_submitted", seed1Submit)) { seedIn1.Disconnect("text_submitted", seed1Submit); }
        if(seedIn1.IsConnected(SignalName.FocusEntered, seed1FocusEntered)) { seedIn1.Disconnect(SignalName.FocusEntered, seed1FocusEntered); }
        if(seedIn1.IsConnected(SignalName.FocusExited, seed1FocusExited)) { seedIn1.Disconnect(SignalName.FocusExited, seed1FocusExited); }
        if (seedIn2.IsConnected("text_submitted", seed2Submit)) { seedIn2.Disconnect("text_submitted", seed2Submit); }
        if (seedIn2.IsConnected(SignalName.FocusEntered, seed2FocusEntered)) { seedIn2.Disconnect(SignalName.FocusEntered, seed2FocusEntered); }
        if (seedIn2.IsConnected(SignalName.FocusExited, seed2FocusExited)) { seedIn2.Disconnect(SignalName.FocusExited, seed2FocusExited); }
        if (seedIn3.IsConnected("text_submitted", seed3Submit)) { seedIn3.Disconnect("text_submitted", seed3Submit); }
        if (seedIn3.IsConnected(SignalName.FocusEntered, seed3FocusEntered)) { seedIn3.Disconnect(SignalName.FocusEntered, seed3FocusEntered); }
        if (seedIn3.IsConnected(SignalName.FocusExited, seed3FocusExited)) { seedIn3.Disconnect(SignalName.FocusExited, seed3FocusExited); }
        if (seedIn4.IsConnected("text_submitted", seed4Submit)) { seedIn4.Disconnect("text_submitted", seed4Submit); }
        if (seedIn4.IsConnected(SignalName.FocusEntered, seed4FocusEntered)) { seedIn4.Disconnect(SignalName.FocusEntered, seed4FocusEntered); }
        if (seedIn4.IsConnected(SignalName.FocusExited, seed4FocusExited)) { seedIn4.Disconnect(SignalName.FocusExited, seed4FocusExited); }
        Button buildBtn = FindChild("Build") as Button;
        if(buildBtn.IsConnected("pressed", whenBuildButtonPressed)) { buildBtn.Disconnect("pressed", whenBuildButtonPressed); }
        Button clearBtn = FindChild("Clear") as Button;
        if(clearBtn.IsConnected("pressed", whenClearButtonPressed)) { clearBtn.Disconnect("pressed", whenClearButtonPressed); }
        Button setupBtn = FindChild("Setup") as Button;
        if(setupBtn.IsConnected("pressed", whenSetupButtonPressed)) { setupBtn.Disconnect("pressed", whenSetupButtonPressed); }

        Button debugShowBtn = FindChild("DebugToggle") as Button;
        if (debugShowBtn.IsConnected("pressed", whenDebugToggleButtonPressed)) { debugShowBtn.Disconnect("pressed", whenDebugToggleButtonPressed); }

        CheckBox seedToggleBox = FindChild("SeedRNGToggle") as CheckBox;
        if (seedToggleBox.IsConnected("toggled", whenSeedRNGToggle)) { seedToggleBox.Disconnect("toggled", whenSeedRNGToggle); }
    }

    private void WhenProfileChanged(Resource newResource)
    {
        GD.Print("UIDungeonControls::UpdateProfile()");
        if (profileSelector.EditedResource == null) {
            settingsSelector.EditedResource = null;
            biomeSelector.EditedResource = null;
            WhenSettingsChanged(null);
            WhenBiomeChanged(null);
            (FindChild("SeedIn1") as LineEdit).Text = "1234";
            (FindChild("SeedIn2") as LineEdit).Text = "1234";
            (FindChild("SeedIn3") as LineEdit).Text = "1234";
            (FindChild("SeedIn4") as LineEdit).Text = "1234";
            return; 
        }

        settingsSelector.EditedResource = (profileSelector.EditedResource as ProfileResource).settings;
        biomeSelector.EditedResource = (profileSelector.EditedResource as ProfileResource).biome;


        Node3D debugContainer = EditorInterface.Singleton.GetEditedSceneRoot().FindChild("GeneratedDebug") as Node3D;
        if (debugContainer == null) { return; }
        Button dDebugToggleBtn = FindChild("DebugToggle") as Button;

        if ((profileSelector.EditedResource as ProfileResource).showDebugLayer)
        {
            debugContainer.Show();
            dDebugToggleBtn.Text = "Hide Debug Info";
        }
        else
        {
            debugContainer.Hide();
            dDebugToggleBtn.Text = "Show Debug Info";
        }
        WhenSettingsChanged(settingsSelector.EditedResource);
        WhenBiomeChanged(biomeSelector.EditedResource);
        UpdateSeeds();
        (FindChild("DebugToggle") as Button).SetPressedNoSignal(Profile.useRandomSeed);

        SaveAddon();
    }

    private void SaveAddon()
    {
        Config.lastUsedProfile = profileSelector.EditedResource == null ? "res://addons/MuNgLosDungeon/Config/def_biome.tres" : profileSelector.EditedResource.ResourcePath;
        ResourceSaver.Save(Config, Config.ResourcePath, ResourceSaver.SaverFlags.ReplaceSubresourcePaths);
    }

    private void WhenSettingsChanged(Resource newResource)
    {
        if (Profile is not null) { Profile.settings = newResource is not null ? newResource as GenerationSettingsResource : null; }

        GetNode<UITabShow>("TabContainer/Show").UpdateVisibleValues();
        GetNode<UITabPasses>("TabContainer/Passes").UpdateVisibleValues();
        GetNode<UITabCorridors>("TabContainer/Corridors").UpdateVisibleValues();
        GetNode<UITabRooms>("TabContainer/Rooms").UpdateVisibleValues();
        if (Profile is not null) { ResourceSaver.Save(Profile, Profile.ResourcePath, ResourceSaver.SaverFlags.ReplaceSubresourcePaths); }
    }
    private void WhenBiomeChanged(Resource newResource)
    {
        if (Profile is not null) { Profile.biome = newResource is not null ? newResource as BiomeDefinition : null; }

        GD.Print($"Dungeons::BiomeChanged [{newResource?.ResourcePath}] is[{newResource is BiomeDefinition}]");
        ResourceSaver.Save(Profile, Profile.ResourcePath, ResourceSaver.SaverFlags.ReplaceSubresourcePaths);

    }
    #region Seed Value Managing
    private void Seed1Submit(string text) { if (int.TryParse(text, out int value)) { (settingsSelector.EditedResource as GenerationSettingsResource).seed1 = value; } UpdateSeeds(); }
    private void Seed1FocusEntered() { seedIn1.SelectAll(); }
    private void Seed1FocusExit() { if (int.TryParse(seedIn1.Text, out int value)) { (settingsSelector.EditedResource as GenerationSettingsResource).seed1 = value; } UpdateSeeds(); }
    private void Seed2Submit(string text) { if (int.TryParse(text, out int value)) { (settingsSelector.EditedResource as GenerationSettingsResource).seed2 = value; } UpdateSeeds(); }
    private void Seed2FocusEntered() { seedIn2.SelectAll(); }
    private void Seed2FocusExit() { if (int.TryParse(seedIn2.Text, out int value)) { (settingsSelector.EditedResource as GenerationSettingsResource).seed2 = value; } UpdateSeeds(); }
    private void Seed3Submit(string text) { if (int.TryParse(text, out int value)) { (settingsSelector.EditedResource as GenerationSettingsResource).seed3 = value; } UpdateSeeds(); }
    private void Seed3FocusEntered() { seedIn3.SelectAll(); }
    private void Seed3FocusExit() { if (int.TryParse(seedIn3.Text, out int value)) { (settingsSelector.EditedResource as GenerationSettingsResource).seed3 = value; } UpdateSeeds(); }
    private void Seed4Submit(string text) { if (int.TryParse(text, out int value)) { (settingsSelector.EditedResource as GenerationSettingsResource).seed4 = value; } UpdateSeeds(); }
    private void Seed4FocusEntered() { seedIn4.SelectAll(); }
    private void Seed4FocusExit() { if (int.TryParse(seedIn4.Text, out int value)) { (settingsSelector.EditedResource as GenerationSettingsResource).seed4 = value; } UpdateSeeds(); }
    #endregion
    private void WhenSeedRNGToggled(bool toggled)
    {
        if (toggled)
        {
            GD.Print("Seed will be randomized and written to config when pressing build");
        }
        else
        {
            GD.Print("The seed given will be used when pressing build");
        }
    }
    private void WhenSetupPressed()
    {
        if(EditorInterface.Singleton.GetEditedSceneRoot() == null)
        {
            GD.PrintErr("Setup can only be run in a 3D scene. Make sure the root node is a Node3D");
            return;
        }

        Node dungeonNode = EditorInterface.Singleton.GetEditedSceneRoot().FindChild("Dungeon");
        if (dungeonNode == null)
        {
            Node3D obj = new Node3D();
            EditorInterface.Singleton.GetEditedSceneRoot().AddChild(obj);
            obj.Owner = EditorInterface.Singleton.GetEditedSceneRoot();
            obj.Name = "Dungeon";

            ulong objId = obj.GetInstanceId();
            // Replaces old C# instance with a new one. Old C# instance is disposed.
            obj.SetScript(ResourceLoader.Load<CSharpScript>("res://addons/MuNgLosDungeon/Scripts/DungeonGenerator.cs"));
            // Get the new C# instance
            obj = (DungeonGenerator)InstanceFromId(objId);
            dungeonNode = (DungeonGenerator)obj;
        }
        // Generated
        NavigationRegion3D generated = dungeonNode.FindChild("Generated") as NavigationRegion3D;
        if (generated == null)
        {
            NavigationRegion3D obj = new NavigationRegion3D();
            dungeonNode.AddChild(obj);
            obj.Owner = EditorInterface.Singleton.GetEditedSceneRoot();
            obj.Name = "Generated";
            generated = obj;
            generated.NavigationMesh = new NavigationMesh();
        }
        // Generated Debug
        Node3D generatedDebug = dungeonNode.FindChild("GeneratedDebug") as Node3D;
        if (generatedDebug == null)
        {
            Node3D obj = new Node3D();
            dungeonNode.AddChild(obj);
            obj.Owner = EditorInterface.Singleton.GetEditedSceneRoot();
            obj.Name = "GeneratedDebug";
            generatedDebug = obj;
            generatedDebug.Hide();
        }
    }
    private void WhenBuildPressed()
    {
       WhenClearPressed();

        // Maybe randomize seed
        if ((FindChild("SeedRNGToggle") as CheckBox).ButtonPressed)
        {
            Settings.seed1 = GD.RandRange(1000, 9999);
            Settings.seed2 = GD.RandRange(1000, 9999);
            Settings.seed3 = GD.RandRange(1000, 9999);
            Settings.seed4 = GD.RandRange(1000, 9999);
            UpdateSeeds();
        }

        if(Config== null)
        {
            Config = ResourceLoader.Load("res://addons/MuNgLosDungeon/Config/def_addonconfig.tres") as AddonSettings;
        }

        RoomResource startRoom = ResourceLoader.Load(Config.defaultStartRoom) as RoomResource;
        RoomResource standardRoom = ResourceLoader.Load(Config.defaultStandardRoom) as RoomResource;

        screen.GenerateDungeon(Settings, biomeSelector.EditedResource as BiomeDefinition);
    }
    private void WhenClearPressed()
    {
        screen.WhenClearPressed();
    }
    
    private void WhenDebugTogglePressed()
    {
        Button dDebugToggleBtn = FindChild("DebugToggle") as Button;

        //if (screen.WhenDebugTogglePressed())
        //{
        //    (screen.FindChild("GeneratedDebug") as Node3D).Hide();
        //    dDebugToggleBtn.Text = "Show Debug Info";
        //}
        //else
        //{
        //    (screen.FindChild("GeneratedDebug") as Node3D).Show();
        //    dDebugToggleBtn.Text = "Hide Debug Info";
        //}
    }

    
    private void UpdateSeeds()
    {
        if (Settings != null)
        {
            (FindChild("SeedIn1") as LineEdit).Text = Settings.seed1.ToString();
            (FindChild("SeedIn2") as LineEdit).Text = Settings.seed2.ToString();
            (FindChild("SeedIn3") as LineEdit).Text = Settings.seed3.ToString();
            (FindChild("SeedIn4") as LineEdit).Text = Settings.seed4.ToString();
        }
    }

    internal void SetSubViewport(MainScreen screen)
    {
        this.screen = screen;
    }
}
#endif