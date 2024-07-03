#if TOOLS
using Godot;
using Munglo.DungeonGenerator;
[Tool]
public partial class UITabCorridors : GridContainer
{
    private GenerationSettingsResource Settings => (GetParent().GetParent().FindChild("SettingsSelector") as EditorResourcePicker).EditedResource is not null ? (GetParent().GetParent().FindChild("SettingsSelector") as EditorResourcePicker).EditedResource as GenerationSettingsResource : null;

    private SpinBox perStair;
    private Callable PerStairChanged;
    private SpinBox maxLength;
    private Callable MaxLengthChanged;
    private SpinBox maxStraight;
    private Callable MaxStraightChanged;
    private SpinBox minStraight;
    private Callable MinStraightChanged;
    private SpinBox maxBranches;
    private Callable MaxBranchesChanged;
    public override void _Ready()
    {
        perStair = FindChild("sbPerStair") as SpinBox;
        PerStairChanged = new Callable(this, "ChangePerStair");
        perStair.Connect("value_changed", PerStairChanged);
        maxLength = FindChild("sbMaxLength") as SpinBox;
        MaxLengthChanged = new Callable(this, "ChangeMaxLength");
        maxLength.Connect("value_changed", MaxLengthChanged);
        maxStraight = FindChild("sbMaxStraight") as SpinBox;
        MaxStraightChanged = new Callable(this, "ChangeMaxStraight");
        maxStraight.Connect("value_changed", MaxStraightChanged);
        minStraight = FindChild("sbMinStraight") as SpinBox;
        MinStraightChanged = new Callable(this, "ChangeMinStraight");
        minStraight.Connect("value_changed", MinStraightChanged);
        maxBranches = FindChild("sbMaxBranches") as SpinBox;
        MaxBranchesChanged = new Callable(this, "ChangeMaxBranches");
        maxBranches.Connect("value_changed", MaxBranchesChanged);
        UpdateVisibleValues();
    }
    public override void _ExitTree()
    {
        if (perStair.IsConnected("value_changed", PerStairChanged)) { perStair.Disconnect("value_changed", PerStairChanged); }
        if (maxLength.IsConnected("value_changed", MaxLengthChanged)) { maxLength.Disconnect("value_changed", MaxLengthChanged); }
        if (maxStraight.IsConnected("value_changed", MaxStraightChanged)) { maxStraight.Disconnect("value_changed", MaxStraightChanged); }
        if (minStraight.IsConnected("value_changed", MinStraightChanged)) { minStraight.Disconnect("value_changed", MinStraightChanged); }
        if (maxBranches.IsConnected("value_changed", MaxBranchesChanged)) { maxBranches.Disconnect("value_changed", MaxBranchesChanged); }
    }
    private void WhenDraw()
    {
        UpdateVisibleValues();
    }
    private void ChangePerStair(double value)
    {
        Settings.corPerStair = (int)value;
        ResourceSaver.Save(Settings);
        UpdateVisibleValues();
    }
    private void ChangeMaxLength(double value)
    {
        Settings.corMaxTotal = (int)value;
        ResourceSaver.Save(Settings);
        UpdateVisibleValues();
    }
    private void ChangeMaxStraight(double value)
    {
        Settings.corMaxStraight = Mathf.Max((int)value, 3);
        ResourceSaver.Save(Settings);
        UpdateVisibleValues();
    }
    private void ChangeMinStraight(double value)
    {
        Settings.corMinStraight = Mathf.Max((int)value, 2); 
        ResourceSaver.Save(Settings);
        UpdateVisibleValues(); 
    }
    private void ChangeMaxBranches(double value)
    {
        Settings.maxBranches = (int)value; 
        ResourceSaver.Save(Settings);
        UpdateVisibleValues(); 
    }
    public void UpdateVisibleValues()
	{
        perStair.SetValueNoSignal(Settings is not null ? Settings.corPerStair : 0);
        maxLength.SetValueNoSignal(Settings is not null ? Settings.corMaxTotal : 0);
        maxStraight.SetValueNoSignal(Settings is not null ? Settings.corMaxStraight : 0);
        minStraight.SetValueNoSignal(Settings is not null ? Settings.corMinStraight : 0);
        maxBranches.SetValueNoSignal(Settings is not null ? Settings.maxBranches : 0);
    }
}
#endif