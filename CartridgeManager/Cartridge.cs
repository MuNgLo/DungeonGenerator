using System.Collections.Generic;
using Godot;
using Munglo.Commons;
using Munglo.WeaponsSystem.Cartridges;
using Players;

namespace Munglo.WeaponsSystem;
/// <summary>
/// Mounted on the mountpoint on weapon and determines what happens when weapon calls the Fire()
/// Handled and refered to by index through CoreGameLogic/Armory/CartridgeManager
/// Also pooled through the manager
/// </summary>
/// [globv
[GlobalClass]
public partial class Cartridge : RigidBody3D, ICartridge
{
    [Export] public bool debug = false;
    [Export] private bool goToPool = false; // Set true to make this return to objectpool when fired
    [Export] private AMMOTYPES ammoType = AMMOTYPES.UNSET;
    [Export] private int projectileIndex = -1;
    [Export] private int cartridgeIndex = -1;
    [Export] private int amount = 1;
    [Export] private MeshInstance3D model;
    
    private bool spent = false;
    private DamagePackage damage;
    
    public int Amount { get => amount; }
    public DamagePackage Damage { get => damage; }
    public int ProjectileIndex { get => projectileIndex; set => projectileIndex = value; }
    public bool Spent { get => spent; }

    public override void _Ready()
    {
        if (!Multiplayer.IsServer())
        {
            RpcId(1, "RPCGetClientMesh");
            return;
        }
    }
    public void Fire(Node3D muzzle, float speed, float damageMultiplier, IWeapon weapon, int ownerAIOID)
    {
        if(!Multiplayer.IsServer()){ return; }
        if (spent) { return; }
        if(debug){GD.Print($"{Core.WHO}Cartridge::Fire()");}
        spent = true;
        damage.projContainer = Core.ProjectileContainer;
        damage.lData = new Dictionary<string, ProjectileListenerData>();
        damage.cartAmount = Amount;
        List<int> addL = new List<int>();
        damage.aiObjectId = ownerAIOID;
        damage.dealer = AI.AIManager.Matrix.GetAIObject(damage.aiObjectId).Body;

        for (int i = 0; i < Amount; i++)
        {
            Node3D proj = Projectiles.ProjectileManager.GetProjectile(projectileIndex) as Node3D;
            Core.ProjectileContainer.AddChild(proj);
            proj.Owner = Core.ProjectileContainer;
            proj.GlobalPosition = muzzle.GlobalPosition;
            proj.GlobalRotation = muzzle.GlobalRotation;
            proj.Show();
            (proj as IProjectile).Fire(speed, damage, true);
        }
    }
     public void SetCartridgeValues(int index, CartridgeDefinition cDef)
    {
        ammoType = cDef.ammoType;
        projectileIndex = cDef.projectileIndex;
        cartridgeIndex = index;
        amount = cDef.amount;
        SetDamagePackage(cDef);
        model.Mesh = cDef.mesh;
    }
    public void UnSpend(){
        spent = false;
    }
    public void Eject()
    {
        if (ProcessMode != ProcessModeEnum.Disabled) { GD.Print($"{Core.WHO}Cartridge::Eject() Eject called on disabled cartridge"); }
        Reparent(Core.TempContainer);
        //rb.isKinematic = false;
        //GetNode<TimeToLive>("").Enable();
    }
    public void Eject(Vector3 direction, float force)
    {
        Eject();
        //transform.localRotation = Quaternion.Euler(new Vector3(0.0f, Random.Range(0.0f, 5.0f), Random.Range(0.0f, 360.0f))) * transform.localRotation;
        //rb.AddForce(direction.normalized * force, ForceMode.Impulse);
        //rb.AddTorque(transform.right * force * 2.0f, ForceMode.Impulse);
    }
    public void ReturnToPool()
    {
        GetNode<IPoolableObject>("Poolable").ReturnToPool();
        spent = false;
    }
     public void SetDamagePackage(CartridgeDefinition cartDef)
    {
        damage = new DamagePackage();
        damage.damage = cartDef.damage;
        damage.damageType = cartDef.damageType;
        damage.directionStyle = cartDef.directionStyle;
        damage.physicalForce = cartDef.physicalForce;
        damage.cartAmount = cartDef.amount;
    }
    public void SetDamagePackage(DamagePackage dmg){
        damage = new DamagePackage();
        damage.Copy(dmg);
    }

    #region RPC's to sync on client
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void RPCGetClientMesh()
    {
        if (!Multiplayer.IsServer() || model.Mesh is null) { return; }
        int pID = Multiplayer.GetRemoteSenderId() == 0 ? 1 : Multiplayer.GetRemoteSenderId();
        RpcId(pID, "RPCSetClientMesh", model.Mesh.ResourcePath);
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void RPCSetClientMesh(string path)
    {
        if(debug){GD.Print($"{Core.WHO}Cartridge::RPCSetClientMesh({path})");}
        model.Mesh = GD.Load<Mesh>(path);
    }
    #endregion
}// EOF CLASS