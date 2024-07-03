#if TOOLS
using Godot;
using Munglo.DungeonGenerator;
[Tool]
public partial class UITabPasses : GridContainer
{
    private GenerationSettingsResource Settings => (GetParent().GetParent().FindChild("SettingsSelector") as EditorResourcePicker).EditedResource is not null ? (GetParent().GetParent().FindChild("SettingsSelector") as EditorResourcePicker).EditedResource as GenerationSettingsResource : null;

    private CheckBox floors;
    private Callable floorsToggled;
	private CheckBox walls;
    private Callable wallsToggled;
	private CheckBox ceilings;
    private Callable ceilingsToggled;
	private CheckBox props;
    private Callable propsToggled;
    private CheckBox rooms;
    private Callable roomsToggled;
    private CheckBox debug;
    private Callable debugToggled;
    public override void _Ready()
    {
        floors = FindChild("CheckBoxFloors") as CheckBox;
        floorsToggled = new Callable(this, "FloorsToggled");
        floors.Connect("toggled", floorsToggled);
        walls = FindChild("CheckBoxWalls") as CheckBox;
        wallsToggled = new Callable(this, "WallsToggled");
        walls.Connect("toggled", wallsToggled);
        ceilings = FindChild("CheckBoxCeilings") as CheckBox;
        ceilingsToggled = new Callable(this, "CeilingsToggled");
        ceilings.Connect("toggled", ceilingsToggled);
        props = FindChild("CheckBoxProps") as CheckBox;
        propsToggled = new Callable(this, "PropsToggled");
        props.Connect("toggled", propsToggled);
        rooms = FindChild("CheckBoxRooms") as CheckBox;
        roomsToggled = new Callable(this, "RoomsToggled");
        rooms.Connect("toggled", roomsToggled);
        debug = FindChild("cbDebug") as CheckBox;
        debugToggled = new Callable(this, "DebugToggled");
        debug.Connect("toggled", debugToggled);
        UpdateVisibleValues();
    }
    public override void _ExitTree()
    {
        if (floors.IsConnected("toggled", floorsToggled)) { floors.Disconnect("toggled", floorsToggled); }
        if (walls.IsConnected("toggled", wallsToggled)) { walls.Disconnect("toggled", wallsToggled); }
        if (ceilings.IsConnected("toggled", ceilingsToggled)) { ceilings.Disconnect("toggled", ceilingsToggled); }
        if (props.IsConnected("toggled", propsToggled)) { props.Disconnect("toggled", propsToggled); }
        if (rooms.IsConnected("toggled", roomsToggled)) { rooms.Disconnect("toggled", roomsToggled); }
        if (debug.IsConnected("toggled", debugToggled)) { debug.Disconnect("toggled", debugToggled); }
    }
    private void WhenDraw()
    {
        UpdateVisibleValues();
    }
    private void FloorsToggled(bool toggledOn)
    {
        Settings.floorPass = toggledOn;
        ResourceSaver.Save(Settings);
        UpdateVisibleValues();
    }
    private void WallsToggled(bool toggledOn)
    {
        Settings.wallPass = toggledOn;
        ResourceSaver.Save(Settings);
        UpdateVisibleValues();
    }
    private void CeilingsToggled(bool toggledOn)
    {
        Settings.ceilingPass = toggledOn;
        ResourceSaver.Save(Settings);
        UpdateVisibleValues();
    }
    private void PropsToggled(bool toggledOn)
    {
        Settings.propPass = toggledOn;
        ResourceSaver.Save(Settings);
        UpdateVisibleValues();
    }
    private void RoomsToggled(bool toggledOn)
    {
        Settings.roomPass = toggledOn;
        ResourceSaver.Save(Settings);
        UpdateVisibleValues();
    }
    private void DebugToggled(bool toggledOn)
    {
        Settings.debugPass = toggledOn;
        ResourceSaver.Save(Settings);
        UpdateVisibleValues();
    }
    public void UpdateVisibleValues()
	{
        floors.SetPressedNoSignal(Settings is not null ? Settings.floorPass : false);
		walls.SetPressedNoSignal(Settings is not null ? Settings.wallPass : false);
		ceilings.SetPressedNoSignal(Settings is not null ? Settings.ceilingPass : false);
		props.SetPressedNoSignal(Settings is not null ? Settings.propPass : false);
		rooms.SetPressedNoSignal(Settings is not null ? Settings.roomPass : false);
    }
}
#endif