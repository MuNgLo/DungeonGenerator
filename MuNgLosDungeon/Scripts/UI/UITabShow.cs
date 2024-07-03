#if TOOLS
using Godot;
using Munglo.DungeonGenerator;
[Tool]
public partial class UITabShow : GridContainer
{
    private GenerationSettingsResource Settings => (GetParent().GetParent().FindChild("SettingsSelector") as EditorResourcePicker).EditedResource is not null ? (GetParent().GetParent().FindChild("SettingsSelector") as EditorResourcePicker).EditedResource as GenerationSettingsResource : null;

    private CheckBox floors;
    private Callable whenFloorsToggled;
	private CheckBox walls;
    private Callable whenWallsToggled;
	private CheckBox ceilings;
    private Callable whenCeilingsToggled;
	private CheckBox props;
    private Callable whenPropsToggled;
	private CheckBox debug;
    private Callable whenDebugToggled;
    public override void _EnterTree()
    {
        floors = FindChild("CheckBoxFloors") as CheckBox;
        whenFloorsToggled = new Callable(this, "WhenFloorsToggled");
        floors.Connect("toggled", whenFloorsToggled);
        walls = FindChild("CheckBoxWalls") as CheckBox;
        whenWallsToggled = new Callable(this, "WhenWallsToggled");
        walls.Connect("toggled", whenWallsToggled);
        ceilings = FindChild("CheckBoxCeilings") as CheckBox;
        whenCeilingsToggled = new Callable(this, "WhenCeilingsToggled");
        ceilings.Connect("toggled", whenCeilingsToggled);
        props = FindChild("CheckBoxProps") as CheckBox;
        whenPropsToggled = new Callable(this, "WhenPropsToggled");
        props.Connect("toggled", whenPropsToggled);
        debug = FindChild("CheckBoxDebug") as CheckBox;
        whenDebugToggled = new Callable(this, "WhenDebugToggled");
        debug.Connect("toggled", whenDebugToggled);
    }
    public override void _Ready()
    {
        UpdateVisibleValues();
    }
    public override void _ExitTree()
    {
        if (floors.IsConnected("toggled", whenFloorsToggled)) { floors.Disconnect("toggled", whenFloorsToggled); }
        if (walls.IsConnected("toggled", whenWallsToggled)) { walls.Disconnect("toggled", whenWallsToggled); }
        if (ceilings.IsConnected("toggled", whenCeilingsToggled)) { ceilings.Disconnect("toggled", whenCeilingsToggled); }
        if (props.IsConnected("toggled", whenPropsToggled)) { props.Disconnect("toggled", whenPropsToggled); }
        if (debug.IsConnected("toggled", whenDebugToggled)) { debug.Disconnect("toggled", whenDebugToggled); }
    }
    private void WhenDraw()
    {
        UpdateVisibleValues();
    }
    private void WhenFloorsToggled(bool toggledOn)
    {
        Settings.showFloors = toggledOn; 
        ResourceSaver.Save(Settings);
        UpdateVisibleValues(); 
    }
    private void WhenWallsToggled(bool toggledOn)
    {
        Settings.showWalls = toggledOn; 
        ResourceSaver.Save(Settings);
        UpdateVisibleValues(); 
    }
    private void WhenCeilingsToggled(bool toggledOn)
    {
        Settings.showCeilings = toggledOn; 
        ResourceSaver.Save(Settings);
        UpdateVisibleValues(); 
    }
    private void WhenPropsToggled(bool toggledOn)
    {
        Settings.showProps = toggledOn; 
        ResourceSaver.Save(Settings);
        UpdateVisibleValues(); 
    }
    private void WhenDebugToggled(bool toggledOn)
    {
        Settings.showDebug = toggledOn;
        ResourceSaver.Save(Settings);
        UpdateVisibleValues();
    }
    public void UpdateVisibleValues()
	{
        floors.SetPressedNoSignal(Settings is not null ? Settings.showFloors : false);
		walls.SetPressedNoSignal(Settings is not null ? Settings.showWalls : false);
		ceilings.SetPressedNoSignal(Settings is not null ? Settings.showCeilings : false);
		props.SetPressedNoSignal(Settings is not null ? Settings.showProps : false);
		debug.SetPressedNoSignal(Settings is not null ? Settings.showDebug : false);
    }
}
#endif