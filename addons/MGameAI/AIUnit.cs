using System;
using System.Linq;
using Godot;
using Munglo.AI.Base;
using Munglo.Commons;
using Munglo.AI.Movement;
using Munglo.AI.Inventory;
using System.Collections.Generic;

namespace Munglo.AI;
/// <summary>
/// This is the container class for all relevant values and classes needed to run a basic AI
/// </summary>
public partial class AIUnit : AIObject
{
    internal Node3D weaponMountPoint;
    private IWeapon weapon;
    internal IWeapon Weapon
    {
        get => weapon;
        set
        {
            if (value is null) { currentWeapon = string.Empty; } else { currentWeapon = value.WeaponName; }
            weapon = value;
        }
    }

    [Export] private string currentWeapon = string.Empty;
    private StateEngine state;
    private ActionBaseClass currentAction;
    private UnitMovementMaster[] mMaster;
    private ActionBaseClass[] actions;
    // Parts of the Unit
    // awareness
    private AIAwareness awareness;
    // View ZOne
    private AwernessZone zone;
    private int currentMGroup = -1; // which managed group this mind is registered too. -1 being not registered at all
    private DebugSettings debugConfig;
    private NavigationAgent3D navAgent;
    // Mind
    private AIMind mind;
    //[Tooltip("This is how long an object is remembered"), SerializeField]
    [Export] private float rememberFor = 10.0f;
    private ZoneSettings zoneConfig = new ZoneSettings();
    private MovementEngine movement;
    private Health health;
    private AIInventory inventory;
    public AIInventory Inventory => inventory;
    public Health GetHealth => health;
    public AwernessZone Zone { get => zone; }
    public ZoneSettings ZoneConfig { get => zoneConfig; }
    public Vector3 EyeLocation { get => zone.GlobalPosition; }
    public AIMind Mind { get => mind; }
    public bool IsSelected => AIManager.Selection.Selected != null && AIManager.Selection.Selected.aiObjectID == aiObjectID;
    public AIAwareness Awareness { get => awareness; }
    public DebugSettings DebugConfig { get => debugConfig; }
    public float KeepFor { get => rememberFor; set => rememberFor = value; }
    public StateEngine States { get => state; }
    public AISTATE State { get => state.State; set => state.State = value; }
    public ATTACKSTATE FightState { get => state.FState; set => state.FState = value; }
    public AIMOVEMENTSTATE MovementState { get => state.MovementState; set => state.MovementState = value; }
    public ActionBaseClass CurrentAction { get => currentAction; set => currentAction = value; }
    public ActionBaseClass[] Actions { get => actions; }
    public MovementEngine Movement { get => movement; }
    internal EventHandler OnUnitDoneSetup;
    public override void _EnterTree()
    {

        body = GetParent<CharacterBody3D>();
        navAgent = GetParent().GetNode<NavigationAgent3D>("NavigationAgent3D");
        zone = GetParent().GetNode<AwernessZone>("AwernessZone");
        weaponMountPoint = GetParent().GetNode<Node3D>("ViewMount");
        health = GetNode<Health>("Health");
        inventory = GetNode<AIInventory>("AIInventory");

        awareness = new AIAwareness(this);
        state = new StateEngine();
        mind = new AIMind(this, navAgent);

        List<Node> children = new List<Node>();
        children.AddRange(GetNode("Actions").GetChildren());
        if (children != null)
        {
            children.RemoveAll(x => x is not ActionBaseClass);
            actions = children.Cast<ActionBaseClass>().ToArray();
        }
        else
        {
            GD.PrintErr("AIUnit::__EnterTree() Could not find Actions child node!");
        }
        // TODO Refactor below
        IMovement movementSettings = GetNode<IMovement>("MovementEngineSettings");
        movement = new MovementEngine(this, navAgent, movementSettings);
    }



    public override async void _Ready()
    {
        health.SetFullHealth();
        inventory.AttachInventoryToUnit(this);
        (FindChild("UnitSynchronizer") as MultiplayerSynchronizer).DeltaSynchronized += WhenSynced;

        if (Actions.Length < 1) { GD.PrintErr($"AIUnit _Ready() with no actions! [{Name}]"); }
        else
        {
            if (Actions[0] == null)
            {
                GD.PrintErr($"AIUnit _Ready() with NULL action! [{Name}]");
            }
            else
            {
                // Build array of available actions       
                for (int i = 0; i < Actions.Length; i++)
                {
                    (Actions[i] as IAction).Initialize(this);
                }
            }
        }

        // Randomize navagent priority
        navAgent.AvoidancePriority = (float)GD.RandRange(0.0f, 1.0f);
        navAgent.DebugEnabled = false;

        // Whern Unit has a default weapon when spawned in we make sure to have it setup here
        if (weapon is null && weaponMountPoint.GetChildCount() > 0)
        {
            Node3D child = weaponMountPoint.GetChild(0) as Node3D;
            if (child is IWeapon)
            {
                weapon = (IWeapon)child;
                weapon.AIControlWeapon(weaponMountPoint, aiObjectID);
            }
        }


        await ToSignal(GetTree(), "process_frame");
        await ToSignal(GetTree(), "process_frame");
        await ToSignal(GetTree(), "process_frame");
        PostSetup();
    }

    private void PostSetup()
    {
        OnUnitDoneSetup?.Invoke(this, null);
        //WhenSynced();
        //ActivateUnit();
    }
    /// <summary>
    /// This runs on clients to make the avatar have the correct weapon up
    /// </summary>
    private void WhenSynced()
    {
        // Equip clientside unit
        if (!Multiplayer.IsServer())
        {
            GD.Print($"AIUnit::WhenSynced() 1 currentWeapon[{currentWeapon}] weapon.WeaponName)[{weapon?.WeaponName}]");

            if (weapon == null || currentWeapon != weapon.WeaponName)
            {
                //IWeapon weapon;
                //weapon = Core.Weapons.GetWeapon(currentWeapon);
                //Mind.EquipWeapon(weapon);
                //GD.Print($"AIUnit::WhenSynced() 2 Syncing up the weapon on unit clientside. currentWeapon[{currentWeapon}] weapon.WeaponName)[{weapon.WeaponName}]");
            }
        }
    }
    internal void UpdateUnit(float delta, float minUnitSleep)
    {
        if (ToSoon(delta, minUnitSleep))
        {
            //GD.Print("AIUnit::UpdateUnit() TO SOOON!");
            return;
        }
        //GD.Print("AIUnit::UpdateUnit()");
        mind.UpdateMind(delta);
    }
    /// <summary>
    /// This returns true if to little time has passed since last time the unit was updated
    /// </summary>
    /// <param name="delta"></param>
    /// <param name="minUpdateStep"></param>
    /// <returns></returns>
    public bool ToSoon(float delta, float minUnitSleep)
    {
        TimeSinceLastUpdate += delta;
        if (TimeSinceLastUpdate < minUnitSleep)
        {
            return true;
        }
        return false;
    }


    public override void _PhysicsProcess(double delta)
    {
        Movement.PhysTickUpdate(delta);
    }

    void Start()
    {
        // Register all modifiers to actions
        mind.OnCallModifiersToRegister?.Invoke(this, mind);
        // Register all Type relevencys to actions
        mind.OnCallTypesToRegister?.Invoke(this, mind);
    }

    public void ActivateUnit()
    {
        GD.Print("AIUNIT::ActivateUnit()");
        currentMGroup = AIManager.Instance.RegisterUnit(this);
        mind.StartAI();
    }
    /// <summary>
    /// Stops AI, deregister and destroy them
    /// </summary>
    public void StopUnit()
    {
        GD.Print("AIUNIT::StopUnit()");
        Body.Hide();
        mind.StopAI();
        AIManager.Instance.DeRegisterUnit(currentMGroup, this, true); // NOTE This is for development. when pooled the unit shouldnt be detroyed
    }

    [Serializable]
    public class DebugSettings
    {
        public bool MindDebug;
        public bool AwernessDebug;
        public bool ZoneDebug;
        public bool ZoneCage;
        public bool ZoneVertIndex;
        public bool showBounds;
    }

    /// <summary>
    /// sets faction ID on AIUnit. Alos informs AIManager about the change.
    /// </summary>
    /// <param name="id"></param>
    public void SetFaction(int id)
    {
        AIManager.UpdateFaction(aiObjectID, id);
        factionID = id;
    }

    internal float DistanceToPathEnd(Vector3 worldPoint)
    {
        return GlobalPosition.DistanceTo(worldPoint);
    }

    internal void DebugPath(bool v)
    {
        navAgent.DebugEnabled = v;
    }
}// EOF CLASS