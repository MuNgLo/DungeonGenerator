#if TOOLS
using Godot;
using Munglo.DungeonGenerator;
[Tool]
public partial class UITabRooms : GridContainer
{
    private GenerationSettingsResource Settings => (GetParent().GetParent().FindChild("SettingsSelector") as EditorResourcePicker).EditedResource is not null ? (GetParent().GetParent().FindChild("SettingsSelector") as EditorResourcePicker).EditedResource as GenerationSettingsResource : null;

    private SpinBox maxPerPath;
    private Callable MaxPerPathChanged;
    private SpinBox MaxSizeX;
    private Callable MaxSizeXChanged;
    private SpinBox MaxSizeY;
    private Callable MaxSizeYChanged;
    private SpinBox MaxSizeZ;
    private Callable MaxSizeZChanged;
    private SpinBox MinSizeX;
    private Callable MinSizeXChanged;
    private SpinBox MinSizeY;
    private Callable MinSizeYChanged;
    private SpinBox MinSizeZ;
    private Callable MinSizeZChanged;
    public override void _Ready()
    {
        maxPerPath = FindChild("sbMaxPerPath") as SpinBox;
        MaxPerPathChanged = new Callable(this, "ChangeMaxPerPath");
        maxPerPath.Connect("value_changed", MaxPerPathChanged);
        MaxSizeX = FindChild("sbMaxSizeX") as SpinBox;
        MaxSizeXChanged = new Callable(this, "ChangeMaxSizeX");
        MaxSizeX.Connect("value_changed", MaxSizeXChanged);
        MaxSizeY = FindChild("sbMaxSizeY") as SpinBox;
        MaxSizeYChanged = new Callable(this, "ChangeMaxSizeY");
        MaxSizeY.Connect("value_changed", MaxSizeYChanged);
        MaxSizeZ = FindChild("sbMaxSizeZ") as SpinBox;
        MaxSizeZChanged = new Callable(this, "ChangeMaxSizeZ");
        MaxSizeZ.Connect("value_changed", MaxSizeZChanged);
        MinSizeX = FindChild("sbMinSizeX") as SpinBox;
        MinSizeXChanged = new Callable(this, "ChangeMinSizeX");
        MinSizeX.Connect("value_changed", MinSizeXChanged);
        MinSizeY = FindChild("sbMinSizeY") as SpinBox;
        MinSizeYChanged = new Callable(this, "ChangeMinSizeY");
        MinSizeY.Connect("value_changed", MinSizeYChanged);
        MinSizeZ = FindChild("sbMinSizeZ") as SpinBox;
        MinSizeZChanged = new Callable(this, "ChangeMinSizeZ");
        MinSizeZ.Connect("value_changed", MinSizeZChanged);
        UpdateVisibleValues();
    }
    public override void _ExitTree()
    {
        if (maxPerPath.IsConnected("value_changed", MaxPerPathChanged)) { maxPerPath.Disconnect("value_changed", MaxPerPathChanged); }
        if (MaxSizeX.IsConnected("value_changed", MaxSizeXChanged)) { MaxSizeX.Disconnect("value_changed", MaxSizeXChanged); }
        if (MaxSizeY.IsConnected("value_changed", MaxSizeYChanged)) { MaxSizeY.Disconnect("value_changed", MaxSizeYChanged); }
        if (MaxSizeZ.IsConnected("value_changed", MaxSizeZChanged)) { MaxSizeZ.Disconnect("value_changed", MaxSizeZChanged); }
        if (MinSizeX.IsConnected("value_changed", MinSizeXChanged)) { MinSizeX.Disconnect("value_changed", MinSizeXChanged); }
        if (MinSizeY.IsConnected("value_changed", MinSizeYChanged)) { MinSizeY.Disconnect("value_changed", MinSizeYChanged); }
        if (MinSizeZ.IsConnected("value_changed", MinSizeZChanged)) { MinSizeZ.Disconnect("value_changed", MinSizeZChanged); }
    }
    private void WhenDraw()
    {
        UpdateVisibleValues();
    }
    private void ChangeMaxPerPath(double value)
    {
        Settings.maxRoomsPerPath = (int)value; 
        ResourceSaver.Save(Settings);
        UpdateVisibleValues();
    }
    private void ChangeMaxSizeX(string text)
    {
        //if (int.TryParse(text, out int value)) { UIDungeonControls.Settings.roomMaxSize.X = value; }
        ResourceSaver.Save(Settings);
        UpdateVisibleValues();
    }
    private void ChangeMaxSizeY(string text)
    {
        //if (int.TryParse(text, out int value)) { UIDungeonControls.Settings.roomMaxSize.Y = value; }
        ResourceSaver.Save(Settings);
        UpdateVisibleValues();
    }
    private void ChangeMaxSizeZ(string text)
    {
        //if (int.TryParse(text, out int value)) { UIDungeonControls.Settings.roomMaxSize.Z = value; }
        ResourceSaver.Save(Settings);
        UpdateVisibleValues();
    }
    private void ChangeMinSizeX(string text)
    {
        //if (int.TryParse(text, out int value)) { UIDungeonControls.Settings.roomMinSize.X = value; }
        ResourceSaver.Save(Settings);
        UpdateVisibleValues();
    }
    private void ChangeMinSizeY(string text)
    {
        //if (int.TryParse(text, out int value)) { UIDungeonControls.Settings.roomMinSize.Y = value; }
        ResourceSaver.Save(Settings);
        UpdateVisibleValues();
    }
    private void ChangeMinSizeZ(string text)
    {
        //if (int.TryParse(text, out int value)) { UIDungeonControls.Settings.roomMinSize.Z = value; }
        ResourceSaver.Save(Settings);
        UpdateVisibleValues();
    }
    public void UpdateVisibleValues()
	{
        maxPerPath.SetValueNoSignal(Settings is not null ? Settings.maxRoomsPerPath : 0);
        //MaxSizeX.SetValueNoSignal(UIDungeonControls.Settings is not null ? UIDungeonControls.Settings.roomMaxSize.X : 0);
        //MaxSizeY.SetValueNoSignal(UIDungeonControls.Settings is not null ? UIDungeonControls.Settings.roomMaxSize.Y : 0);
        //MaxSizeZ.SetValueNoSignal(UIDungeonControls.Settings is not null ? UIDungeonControls.Settings.roomMaxSize.Z : 0);
        //MinSizeX.SetValueNoSignal(UIDungeonControls.Settings is not null ? UIDungeonControls.Settings.roomMinSize.X : 0);
        //MinSizeY.SetValueNoSignal(UIDungeonControls.Settings is not null ? UIDungeonControls.Settings.roomMinSize.Y : 0);
        //MinSizeZ.SetValueNoSignal(UIDungeonControls.Settings is not null ? UIDungeonControls.Settings.roomMinSize.Z : 0);
    }
    
}
#endif