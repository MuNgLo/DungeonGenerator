using Godot;
using Munglo.Commons;

namespace Munglo.AI.Base;

public partial class UnitAnimations : AnimationPlayer
{
    [Export] Node3D body;
    [Export] private UNITANIMSTATE animState = UNITANIMSTATE.UNSET;
    [Export] private float walkAnimationSpeed = 1.0f;
    [Export] private float walkAnimationDistance = 1.0f;
    [Export] private float currentMoveSpeed = 0.0f;

    [Export, ExportCategory("Ragdoll things")] public bool _ragdollOnDeath = true;

    private Hitbox[] hitBoxes;
    private Vector3 lastLocation = Vector3.Zero;


    public UNITANIMSTATE AnimState { get => animState; set => ChangeAnimState(value); }

    public override void _PhysicsProcess(double delta)
    {
        currentMoveSpeed = (body.GlobalPosition - lastLocation).LengthSquared() / (float)delta;
        if(currentMoveSpeed > 0.01f){ChangeAnimState(UNITANIMSTATE.MOVING);}
        //anim.SetFloat("MovementSpeed", _currentMoveSpeed);
        lastLocation = body.GlobalPosition;
    }

    private void OnEnable()
    {
        RagdollOff();
        ChangeAnimState(UNITANIMSTATE.IDLE);
    }

    private void ChangeAnimState(UNITANIMSTATE value)
    {
        if (animState == value) { return; }
        animState = value;

        switch (animState)
        {
            case UNITANIMSTATE.IDLE:
                Play("Idle");
                break;
            case UNITANIMSTATE.MOVING:
                Play("Walk", walkAnimationSpeed);
                break;
            case UNITANIMSTATE.DEAD:
                if (_ragdollOnDeath) { RagdollOn(); }
                break;
            case UNITANIMSTATE.UNSET:
            default:
                Play("Idle");

                GD.Print($"Animations on {body.Name} went to idle through ({value}) This should not happen when thinnks work as intended.");
                break;
        }

    }

    public void RunMeleeStandard()
    {
        //anim.SetTrigger("MeleeStandard");
    }
    #region Hitbox and ragdoll stuff
    public void SetupHitboxes()
    {
        foreach (Hitbox hitbox in hitBoxes)
        {
            hitbox.SetupHitbox();
        }
    }
    public void ToggleRagdoll()
    {

    }
    public void HitboxOn()
    {
        foreach (Hitbox hitbox in hitBoxes)
        {
            hitbox.HitBoxOn();
        }
    }
    public void HitboxOff()
    {
        foreach (Hitbox hitbox in hitBoxes)
        {
            hitbox.HitBoxOff();
        }
    }
    /// <summary>
    /// Turns ragdoll on and disables animator
    /// </summary>
    private void RagdollOn()
    {
        //Debug.Log("RagdollOn");
        //GetComponent<AIUnit>().KillAI();
        //anim.enabled = false;
        //GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
    }
    private void RagdollOff()
    {
        //Debug.Log("RagdollOff");
        //anim.enabled = true;
    }
    #endregion
}// EOF CLASS