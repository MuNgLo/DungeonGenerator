using System.Collections.Generic;
using Godot;

namespace Munglo.Commons;
public class DamagePackage
{
    // This is the basedamage. Never change this in runtime
    public float damage;
    public DDIRECTIONAL directionStyle;
    public float physicalForce;
    //public bool stickyObject;
    public int cartAmount;
    //[HideInInspector]
    public DSOURCE source;
    //[HideInInspector]
    public Node3D dealer;
    //[HideInInspector]
    public int aiObjectId;
    //[HideInInspector]
    public DTYPE damageType;
    //[HideInInspector]
    public float damageMultiplier = 1.0f;
    //[HideInInspector]
    public float damageRemaining;
    //[HideInInspector]
    public Vector3 velocity; 
    public Vector3 Direction => velocity.Normalized(); // Direction of package container velocity
                             
    public Vector3 point; // hit point in worldspace
                          //[HideInInspector]
    public Vector3 normal; // normal of the hit
                           //[HideInInspector]
    public Node3D hitTransform;
    public Node3D damageObject;
    public Node3D projContainer;
    public Dictionary<string, ProjectileListenerData> lData = new Dictionary<string, ProjectileListenerData>();

    public Vector3 ForceVector => Direction * physicalForce;
    /// <summary>
    /// Straight copy of other package
    /// </summary>
    /// <param name="inComming"></param>
    public void Copy(DamagePackage inComming)
    {
        damage = inComming.damage;
        directionStyle = inComming.directionStyle;
        physicalForce = inComming.physicalForce;
        cartAmount = inComming.cartAmount;
        source = inComming.source;
        dealer = inComming.dealer;
        aiObjectId = inComming.aiObjectId;
        damageType = inComming.damageType != DTYPE.NONE ? inComming.damageType : damageType;
        damageMultiplier = inComming.damageMultiplier;
        damageRemaining = damage * damageMultiplier;
        velocity = inComming.velocity;
        point = inComming.point;
        normal = inComming.normal;
        hitTransform = inComming.hitTransform;
        damageObject = inComming.damageObject;
        projContainer = inComming.projContainer;
        lData = new Dictionary<string, ProjectileListenerData>(inComming.lData);
    }
}// EOF CLASS