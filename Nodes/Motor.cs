using System;
using Godot;
using Munglo.Movement.Base;
using Munglo.Commons;
namespace Munglo.Movement.Nodes;
/// <summary>
/// The component calss holding all the movement related code.
/// NOTE The MeshCollider has to be simple and flagged as convex.
/// </summary>
[GlobalClass]
public partial class Motor : MMNode
{
    [Export] private bool debug = false;
    [Export] private MOVEMENTSTATE state = MOVEMENTSTATE.WALK;
    // The max iteration we try to consume the full movement in one update
    [Export] private int recursion = 5;
    [Export] private RigidBody3D controlledRigidBody3D;
    [Export] private Node3D ViewPivvot;
    private PlayerInput CMD;
    public Vector3 velocity;
    //public Vector3 velocity { set { if (float.IsNaN(value.X)) GD.PushError(); velocityPRIV = value; } get => velocityPRIV; }

    [ExportCategory("Modules")]
    [Export] private MMPhysics mmPhysics;
    [Export] private GroundMovement groundMovement;
    [Export] private AirMovement airMovement;

    [ExportCategory("Moves")]
    [Export] private Moves.Jump jump;
    [Export] private Moves.Sliding sliding;

    #region  Need to be rewritten
    #region private
    private float currentSpeed = 0.0f;
    private float previousSpeed = 0.0f;
    private bool wasGrounded = true; // remember the groudned flag from last tic
    private bool isCrouched = false;
    private float stepDistance = 0.0f; // Keep track of distance moved to make step sounds
    private Vector3 focusPoint = Vector3.Zero;

    private Vector3 outerForcesForFrame = Vector3.Zero; // This stacks forces from none movement code to be applied at end of movement. Like moving platform and jumppad
    private Vector3 outerGravitySources = Vector3.Zero;
    private Plane groundPlane;
    private Vector3 wishDir;
    private GroundTest groundData;

    #endregion

    #region Local Events
    public EventHandler<GroundTest> OnJump;
    public EventHandler<GroundTest> OnLand;
    public EventHandler<GroundTest> OnStep;
    public EventHandler<PlayerMovementEventArguments> OnPlayerMovementEvent;
    public EventHandler<PlayerMovementResetArguments> OnPlayerMovementResetEvent;
    #endregion


    #region Properties
    public bool IsGrounded => groundData.grounded;
    public Vector3 Gravity => mmPhysics.Gravity;
    public Vector3 GroundNormal => groundData.groundNormal;
    public float GroundAngle => groundData.groundAngle;
    public float MaxIncline => sliding.MaxAngle;
    public float CurrentSpeed => currentSpeed;
    public float PreviousSpeed => previousSpeed;
    public float LastSpeedChange => currentSpeed - previousSpeed;
    public Plane GroundPlane => groundPlane;
    public Plane GravityPlane => mmPhysics.gravityPlane;
    public Vector3 Velocity => velocity;
    public Vector3 Up => RigidBody.Basis.Y;
    private RigidBody3D RigidBody => controlledRigidBody3D;
    public Vector3 Position => RigidBody.Position;
    public Vector3 GlobalPosition { get => RigidBody.GlobalPosition; set => RigidBody.GlobalPosition = value; }
    public Vector3 GlobalRotation { get => RigidBody.GlobalRotation; set => RigidBody.GlobalRotation = value; }
    #endregion



    #endregion

    /// <summary>
    /// Instantiate the data needed
    /// Set the required settingns on the rigidbody
    /// </summary>
    public override void _Ready()
    {
        groundData = new();
        mmPhysics.Setup();
        controlledRigidBody3D.GravityScale = 0.0f;
        velocity = Vector3.Zero;
    }
    /// <summary>
    /// Feed the playerinput isntance here that should control this avatar
    /// Returns true if the internal cmd reference is not NULL
    ///  SImplified for now Might nneed work
    /// </summary>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public bool TakeControl(PlayerInput cmd)
    {
        CMD = cmd;
        velocity = Vector3.Zero;
        return CMD is not null;
    }
    /// <summary>
    /// Run movement
    /// </summary>
    /// <param name="delta"></param>
    public override void _PhysicsProcess(double delta)
    {
        if (CMD is null) { return; }
        ProcessMouseRotation(delta);
        wishDir = mmPhysics.gravityPlane.Project(controlledRigidBody3D.Transform.Basis * CMD.inVec).Normalized();


        GravityCompliance((float)delta, false);

        // Apply outerforces
        //velocity += outerForcesForFrame;
        //outerForcesForFrame = Vector3.Zero;

        // Apply outergravity
        //velocity += outerGravitySources;
        //outerGravitySources = Vector3.Zero;

        GroundCheck();

        if (IsGrounded && jump.CanJump(this) && CMD.isJumping)
        {
            if (debug) { GD.Print("Motor::_PhysicsProcess() JUMPED!"); }
            velocity = jump.DoJump(this, velocity);
            RaiseOnJumpEvent(groundData);
            groundData.groundPoint = Vector3.Zero;
            groundData.groundNormal = Vector3.Zero;
            groundData.grounded = false;
        }
        if (IsGrounded) { GroundMovement((float)delta); } else { AirMovement((float)delta); ApplyGravity((float)delta); }

        Vector3 frameStep = velocity * (float)delta;
        // Consume framestep in max X steps
        for (int i = 0; i < recursion; i++)
        {
            switch (state)
            {
                case MOVEMENTSTATE.WALK:
                    frameStep = DoWalkMovement(frameStep, (float)delta);
                    break;
                default:
                    return;
            }
            if (frameStep.LengthSquared() < 0.002f) { break; }
        }
        // PUSH down
        if (IsGrounded && jump.CanJump(this))
        {
            KinematicCollision3D pushDown = controlledRigidBody3D.MoveAndCollide(-controlledRigidBody3D.Transform.Basis.Y * mmPhysics.pushDownDistance);
            if (pushDown is not null)
            {
                controlledRigidBody3D.Position += Up * mmPhysics.groundDistance;
            }
            else
            {
                if (debug) { GD.Print("Motor::_PhysicsProcess() PushDown Reversed!"); }
                DrawGizmo(0.5f, controlledRigidBody3D.Transform.Basis.Y * mmPhysics.pushDownDistance, Colors.SteelBlue);
                // Lost ground contact so negating the pushdown
                controlledRigidBody3D.Position += controlledRigidBody3D.Transform.Basis.Y * mmPhysics.pushDownDistance;
            }
        }

        // Check if we landed this update
        if (!wasGrounded && IsGrounded)
        {
            RaiseOnLandEvent(groundData);
        }
        wasGrounded = IsGrounded;

        // Update currentspeed
        previousSpeed = currentSpeed;
        currentSpeed = mmPhysics.gravityPlane.Project(velocity).Length();

        // Send step event in intervals of distance travelled
        stepDistance += currentSpeed * (float)delta;
        if (stepDistance > 2.0f)
        {
            stepDistance -= 2.0f;
            RaiseOnStepEvent(groundData);
        }
        return;
    }

    private Vector3 DoWalkMovement(Vector3 frameStep, float delta)
    {
        //if (IsGrounded && jump.CanJump(this))
        if (IsGrounded)
        {
            // Do GroundMovement
            // Try a straigth up move
            KinematicCollision3D movementCollision = controlledRigidBody3D.MoveAndCollide(frameStep);
            if (movementCollision is not null)
            {
                frameStep = movementCollision.GetRemainder();
                /////////////////////////////////
                // Handle collision when grounded
                /////////////////////////////////
                // Move upwards up to a full step height
                KinematicCollision3D vertMove = controlledRigidBody3D.MoveAndCollide(controlledRigidBody3D.Transform.Basis.Y * mmPhysics.stepHeight);
                // Limit step to a minimum of 2 cm 
                if (vertMove is null || vertMove.GetTravel().Length() > 0.02f)
                {
                    // After we moved up, try to continue the framestep movement
                    movementCollision = controlledRigidBody3D.MoveAndCollide(frameStep);
                    // Move down to plant on surface
                    controlledRigidBody3D.MoveAndCollide(-controlledRigidBody3D.Transform.Basis.Y * mmPhysics.stepHeight);
                    // Adjust so we have the desired gap to surface
                    controlledRigidBody3D.Position += Up * mmPhysics.groundDistance;
                    //GD.Print("Motor::_PhysicsProcess() STEPPED!");
                    if (movementCollision is not null)
                    {
                        return frameStep - movementCollision.GetTravel();
                    }
                    return Vector3.Zero;
                }
                else
                {
                    Plane colPlane = new Plane(movementCollision.GetNormal());
                    velocity = colPlane.Project(velocity);
                    frameStep = colPlane.Project(movementCollision.GetRemainder());
                    // STEP DENIED! Set back to OG position
                    controlledRigidBody3D.Position -= vertMove.GetTravel();
                    GD.Print("Motor::_PhysicsProcess() STEP NEGATED!");
                    return frameStep;
                }
            }
            // Groundmovemnet with no collision
            // Use jump cooldown to control pushdown
            return Vector3.Zero;
        }
        else
        {
            // Do Airmovement when not grounded or jump is in cooldown
            // How far we gonna move this update
            KinematicCollision3D movementCollision = controlledRigidBody3D.MoveAndCollide(frameStep);
            if (movementCollision is not null)
            {
                //if (debug) { GD.Print("Motor::_PhysicsProcess() Airmovement with collision"); }
                Plane colPlane = new Plane(movementCollision.GetNormal());
                velocity = colPlane.Project(velocity);
                return colPlane.Project(movementCollision.GetRemainder());
            }
            return Vector3.Zero;
        }
    }


    /* TODO FOR LATER
    // Rotate according to gravity
    if (controlledRigidBody3D.GlobalTransform.Basis.Y.Dot(-mmPhysics.Gravity.Normalized()) < 1.0f)
    {
        GravityCompliance((float)delta, CMD.doFastRotation);
    }

    // HANDLE RIGIDBODIES WE COLLIDE WITH 
    Vector3 velLoss = Vector3.Zero;
    int breaker = 10;
    while (breaker > 0 && hit is not null)
    {
        breaker--;
        if (hit.GetCollider() is RigidBody3D)
        {
            GD.Print($"MoveAndCollide() RigidBody3D hit! breaker[{breaker}]");
            RigidBody3D otherRB = hit.GetCollider() as RigidBody3D;

            Vector3 point = hit.GetPosition() - otherRB.GlobalPosition;
            otherRB.ApplyImpulse(velocity * (controlledRigidBody3D.Mass / otherRB.Mass), point);

            velLoss += velocity * (otherRB.Mass / controlledRigidBody3D.Mass);

            if (!controlledRigidBody3D.GetCollisionExceptions().Contains(otherRB))
            {
                rbCollisionTS = 1;
                controlledRigidBody3D.AddCollisionExceptionWith(otherRB);
            }
        }else if(IsGrounded){
            // try step up and move again

        }
        frameStep = hit.GetRemainder();
        hit = controlledRigidBody3D.MoveAndCollide(frameStep);
    }
    */


    /// <summary>
    /// Updates the grounddata from where the RB collider is right now
    /// </summary>
    private void GroundCheck()
    {
        // Probe for ground
        KinematicCollision3D groundCheck = controlledRigidBody3D.MoveAndCollide(-controlledRigidBody3D.Transform.Basis.Y * mmPhysics.pushDownDistance, testOnly: true);
        if (groundCheck is not null)
        {
            groundData.groundNormal = groundCheck.GetNormal();
            groundData.groundPoint = groundCheck.GetPosition();
            groundData.grounded = true;
            groundData.groundAngle = groundCheck.GetAngle(0, -mmPhysics.Gravity.Normalized());
            if (debug) { GD.Print($"Motor::GroundCheck() ground angle [{groundData.groundAngle}]"); }

        }
        else
        {
            groundData.groundNormal = Vector3.Zero;
            groundData.groundPoint = Position;
            groundData.grounded = false;
            //groundData.groundAngle = 0.0f;
        }
        groundPlane = new(groundData.groundNormal);
    }

    private double rbCollisionTS = 0;
    public override void _Process(double delta)
    {
        ProcessMouseInput(delta);

        if (!Multiplayer.IsServer()) { return; }
        if (rbCollisionTS > 0)
        {
            rbCollisionTS -= delta;
            if (rbCollisionTS <= 0)
            {
                foreach (PhysicsBody3D otherRB in controlledRigidBody3D.GetCollisionExceptions())
                {
                    controlledRigidBody3D.RemoveCollisionExceptionWith(otherRB);
                }
            }
        }
    }
    private void ProcessMouseInput(double delta)
    {
        // MouseInput
        Vector2 mVel = CMD.MouseIn;
        if (mVel != Vector2.Zero)
        {
            hRotation += (double)mVel.X;
            // Tilt Camera Up/Down
            ViewPivvot.Rotate(Vector3.Left, (float)((double)mVel.Y));
        }
    }
    private double hRotation = 0.0;
    private void ProcessMouseRotation(double delta)
    {
        // Rotate this body left/right
        controlledRigidBody3D.Rotate(-controlledRigidBody3D.GlobalBasis.Y, (float)(hRotation));
        hRotation = 0.0f;
    }
    /// <summary>
    /// Apply friction and project velocity over ground surface
    /// </summary>
    /// <param name="delta"></param>
    private void GroundMovement(float delta)
    {
        if (!sliding.Slide(this, wishDir, delta))
        {
            Vector3 flatVelocity = GravityPlane.Project(velocity);
            Vector3 fallVelocity = velocity - flatVelocity;
            flatVelocity = groundMovement.GroundMove(groundPlane.Project(flatVelocity), wishDir, (float)delta);
            flatVelocity = mmPhysics.ApplyFriction(1.0f, flatVelocity, (float)delta);
            velocity = flatVelocity + fallVelocity;
        }
    }

    private void AirMovement(float delta)
    {
        // Prepare Wish direction vector
        Plane gravityPlane = new(mmPhysics.Gravity.Normalized());
        // Break apart velocity over gravity vector
        Vector3 flatVelocity = gravityPlane.Project(velocity);
        Vector3 fallVelocity = velocity - flatVelocity;
        //if (debug) { GD.Print($"Motor::AirMovement() wishDir[{wishDir}] CMD.inVec[{CMD.inVec}] flatVelocity[{flatVelocity}] fallVelocity[{fallVelocity}]"); }
        // In air we adjust the flat velocity to let gravity build over frames in the fall velocity
        flatVelocity = airMovement.AirMove(flatVelocity, wishDir, CMD, delta);
        // Apply velocity changes
        velocity = flatVelocity + fallVelocity;
    }

    #region Motor internal local events
    private void RaiseOnJumpEvent(GroundTest groundData)
    {
        EventHandler<GroundTest> evt = OnJump;
        evt?.Invoke(null, groundData);
    }
    private void RaiseOnLandEvent(GroundTest groundData)
    {
        EventHandler<GroundTest> evt = OnLand;
        evt?.Invoke(null, groundData);
    }
    private void RaiseOnStepEvent(GroundTest groundData)
    {
        EventHandler<GroundTest> evt = OnStep;
        evt?.Invoke(null, groundData);
    }
    private void RaiseOnPlayerMovementEvent(PlayerMovementEventArguments args)
    {
        EventHandler<PlayerMovementEventArguments> evt = OnPlayerMovementEvent;
        evt?.Invoke(null, args);
    }
    private void RaiseOnPlayerMovementResetEvent(PlayerMovementResetArguments args)
    {
        EventHandler<PlayerMovementResetArguments> evt = OnPlayerMovementResetEvent;
        evt?.Invoke(null, args);
    }
    #endregion


    #region  UNTESTED AFTER GODOT MOVE!!!!

    /// <summary>
    /// This instantly freezes the rigidbody where it is by setting its velocity to 0
    /// </summary>
    internal void FreezeAvatar()
    {
        velocity = Vector3.Zero; // these all works says Zas
        previousSpeed = 0.0f; // these all works says Zas
        currentSpeed = 0.0f; // these all works says Zas
        outerForcesForFrame = Vector3.Zero; // these all works says Zas
        outerGravitySources = Vector3.Zero; // these all works says Zas
        controlledRigidBody3D.Transform = new Transform3D(Basis.Identity, Vector3.Zero); // these all works says Zas
                                                                                         //MovementEnabled = false;
    }

    private void ApplyGravity(float delta)
    {
        velocity += mmPhysics.Gravity * delta;
    }
    public void SetGravity(Vector3 newGravity)
    {
        if (debug) { GD.Print($"Motor::SetGravity() newGravity[{newGravity}]"); }
        mmPhysics.Gravity = newGravity;
    }
    public void SetGravity(Vector3 newGravitydirection, float strength)
    {
        if (debug) { GD.Print($"Motor::SetGravityDirection() newGravityDirection[{newGravitydirection}] strength[{strength}]"); }
        mmPhysics.Gravity = newGravitydirection.Normalized() * strength;
    }
    public void SetGravityDirection(Vector3 newGravityDirection)
    {
        if (debug) { GD.Print($"Motor::SetGravityDirection() newGravityDirection[{newGravityDirection}]"); }

        mmPhysics.Gravity = newGravityDirection.Normalized() * mmPhysics.Gravity.Length();
    }
    /// <summary>
    /// Rotates rigidBody towards gravity using rotationSpeed.
    /// If doFastRotation is true rotationSpeed is multiplied with rotationSpeedModifier.
    /// </summary>
    /// <param name="delta"></param>
    /// <param name="doFastRotation"></param>
    internal void GravityCompliance(float delta, bool doFastRotation)
    {
        // Don't comply to zero gravity
        if (mmPhysics.Gravity == Vector3.Zero) { return; }
        // Speed to rotate in. Degrees per second.
        float rotSpeed = doFastRotation ? mmPhysics.rotationSpeed * mmPhysics.rotationSpeedModifier : mmPhysics.rotationSpeed;
        // Calculate what forward should be
        Vector3 wishForward = mmPhysics.gravityPlane.Project(-controlledRigidBody3D.GlobalBasis.Z);
        //Vector3 wishForward = (_rb.Transform.Basis * Vector3.Forward).Project(_Physics.Gravity).Normalized();
        if (wishForward == Vector3.Zero)
        {
            GD.Print($"Motor::GravityCompliance() VECTOR ZERO ROTATION COOKIE SAYS THIS BE BAD!!!");
            return;
        }

        //Quaternion targetRotation = Quaternion.FromEuler(wishForward, -_Physics.Gravity);

        /*
        Quaternion rotationTweak = Quaternion.RotateTowards(
            _rb.rotation,
            targetRotation,
            rotationSpeed * delta
            );
        _rb.rotation = rotationTweak;

        // Horizontal first
        Vector3 focusDirection = Vector3.ProjectOnPlane(_focusPoint - _rb.position, _rb.rotation * Vector3.up);
        if (Vector3.Angle(focusDirection, _rb.rotation * Vector3.forward) > 90.0f)
        {
            focusDirection = -focusDirection;
        }
        //_rb.rotation = Quaternion.RotateTowards(_rb.rotation, Quaternion.LookRotation(focusDirection, _rb.rotation * Vector3.up), 30.0f * Time.fixedDeltaTime);
        */
        Quaternion ogRot = controlledRigidBody3D.Quaternion;
        controlledRigidBody3D.LookAt(controlledRigidBody3D.GlobalPosition + wishForward * 10.0f, -mmPhysics.Gravity);
        Quaternion targetRot = controlledRigidBody3D.Quaternion;

        float angle = Mathf.RadToDeg(ogRot.AngleTo(targetRot));

        controlledRigidBody3D.Quaternion = ogRot.Slerp(targetRot, Math.Clamp(rotSpeed * delta / angle, 0.0f, 1.0f));

    }
    /// <summary>
    /// This should be deltad
    /// </summary>
    /// <param name="forceVector"></param>
    public void AddOutsideForce(Vector3 forceVector)
    {
        outerForcesForFrame += forceVector;
    }
    public void ApplyJumppadForce(Vector3 direction, float force)
    {
        Vector3 jpRelativeVerticalSpeed = velocity.Project(direction);
        // Check if we have to compensate when player is moving opposite to jumppad launch direction
        // Makes for consistent launches
        //if (Vector3.Dot(direction, jpRelativeVerticalSpeed) < 0)
        if (direction.Dot(jpRelativeVerticalSpeed) < 0)
        {
            outerForcesForFrame += direction * force - jpRelativeVerticalSpeed;
        }
        else
        {
            outerForcesForFrame += direction * force;
        }
        //_groundCheck.ForceGroundLoss();
    }
    internal void ApplyOutsideExplosionForce(Vector3 direction, float force)
    {
        outerForcesForFrame += direction * force;
    }
    public void ApplyOutsideGravitationalPull(Vector3 sourcepoint, float deltadForce)
    {
        outerGravitySources += (sourcepoint - controlledRigidBody3D.Position).Normalized() * deltadForce;
    }
    /// <summary>
    /// Dampens player velocity by percent over second
    /// </summary>
    /// <param name="v">1-100</param>
    internal void Dampen(int percent, float delta, bool noGrav = false)
    {
        percent = Mathf.Clamp(percent, 0, 1000);
        //_playerVelocity *= 1.0f - percent * delta;
        velocity -= velocity * ((percent / 100.0f) * delta);
        if (noGrav)
        {
            velocity += -mmPhysics.Gravity * delta;
        }
    }
    #endregion

    private void DrawGizmo(float ttl, Vector3 velocity, Color col)
    {
        Commons.Gizmos.SegmentedGizmo gizmo = new Commons.Gizmos.SegmentedGizmo();
        gizmo.color = col;
        GetTree().Root.AddChild(gizmo);
        gizmo.Position = GlobalPosition;
        gizmo.ttl = ttl;
        gizmo.ClearSegments();
        gizmo.AddSegment(Vector3.Zero, velocity);
    }
}// End of Class