#if TOOLS
using Godot;
using Munglo.DungeonGenerator;

[Tool]
public partial class UITabGeneral : GridContainer
{
    private GenerationSettingsResource Settings => (GetParent().GetParent().FindChild("SettingsSelector") as EditorResourcePicker).EditedResource is not null ? (GetParent().GetParent().FindChild("SettingsSelector") as EditorResourcePicker).EditedResource as GenerationSettingsResource : null;


    private SpinBox nbOfFloors;
    private Callable whenNBOfFloorsChanged;

    public override void _EnterTree()
    {
        nbOfFloors = FindChild("sbNBOfFloors") as SpinBox;
        whenNBOfFloorsChanged = new Callable(this, "WhenNBOfFloorsChanged");
        nbOfFloors.Connect("value_changed", whenNBOfFloorsChanged);
    }
    public override void _ExitTree()
    {
        if (nbOfFloors.IsConnected("value_changed", whenNBOfFloorsChanged)) { nbOfFloors.Disconnect("value_changed", whenNBOfFloorsChanged); }
    }
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        nbOfFloors.SetValueNoSignal(Settings is not null ? Settings.nbOfFloors : 0);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public void WhenNBOfFloorsChanged(double value)
	{
        value = Mathf.Clamp(value, 1, 50);
        Settings.nbOfFloors = (int)value;
        ResourceSaver.Save(Settings);
        nbOfFloors.SetValueNoSignal(Settings.nbOfFloors);
    }
}// EOF CLASS
#endif
