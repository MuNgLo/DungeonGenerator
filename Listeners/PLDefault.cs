using Godot;
using Munglo.Commons;

namespace Munglo.WeaponsSystem.Listeners;

[GlobalClass]
public partial class PLDefault : PLBase, IProjectileListener
{
    [Export] public bool debug = false;
    // Time that the owner layer will be exluded as projectiled is fired
    [Export] public float selfDamageWindow = 0.05f;
    // Impact options
    [Export] public int numberOfImpact = 1;
    // Hit options
    [Export] public int numberOfHits = 1;
    // Attach options
    [Export] public bool attachWhenOnHit = false;
    [Export] public bool attachWhenOnImpact = false;
    // how far into hitpoint it attaches
    [Export(PropertyHint.Range, "-1.0f, 1.0f")] 
    public float penetration = 0.2f;
    /////////////////////
    //  Bounce options //
    /////////////////////
    [Export] public bool bounceWhenOnHit = false;
    [Export] public bool bounceWhenOnImpact = true;
    [Export(PropertyHint.Range, "0, 20")] public int numberOfBounces = 0;
    [Export(PropertyHint.Range, "0.01f, 1.0f")] public float damping = 0.85f;
    [Export] public bool faceDirectionAfterBounce = false;
    // In centimeters")]
    [Export] public float faceoffset = 0.0f;
    public override void OnHit(IProjectile pr, Node3D hitTR, Vector3 velocity, float distance, ref DamagePackage package)
    {
        //GD.Print($"PLDefault:OnHit() Hit[{hitTR.GetParent().Name}]/[{hitTR.Name}]");
        if (!package.lData.ContainsKey("DEFAULT")) { return; }
        (package.lData["DEFAULT"] as DefaultData).hasHit++;
        if (bounceWhenOnHit && (package.lData["DEFAULT"] as DefaultData).hasBounced < numberOfBounces)
        {
            Bounce(pr, ref package, distance, pr.LastDelta);
            return;
        }
        ResolveFateAfterHit(pr, hitTR, ref package);
    }
    private void ResolveFateAfterHit(IProjectile pr, Node3D hitTR, ref DamagePackage package)
    {
        if ((package.lData["DEFAULT"] as DefaultData).hasHit >= numberOfHits)
        {
            if (attachWhenOnHit)
            {
                // Attach
                pr.N3D.Position = package.point + pr.GloabalForward * penetration;
                pr.N3D.Reparent(hitTR);
                pr.Stop();
                return;
            }
            //if (package.lData.ContainsKey("Stake")) { return; }
            pr.Despawn();
        }
    }

    public override void OnImpact(IProjectile pr, Node3D hitTR, Vector3 velocity, float distance, ref DamagePackage package)
    {
        //GD.Print($"PLDefault:OnImpact() Hit[{hitTR.GetParent().Name}][{hitTR.Name}] Has DEFAULT Key in lData[{package.lData.ContainsKey("DEFAULT")}]");

        if (!package.lData.ContainsKey("DEFAULT")) { return; }
        (package.lData["DEFAULT"] as DefaultData).hasImpacted++;
        if (bounceWhenOnImpact && (package.lData["DEFAULT"] as DefaultData).hasBounced < numberOfBounces)
        {
            Bounce(pr, ref package, distance, pr.LastDelta);
            return;
        }
        ResolveFateAfterImpact(pr, hitTR, ref package);
    }
    private void ResolveFateAfterImpact(IProjectile pr, Node3D hitTR, ref DamagePackage package)
    {
        //GD.Print($"PLDefault:ResolveFateAfterImpact() attachWhenOnImpact[{attachWhenOnImpact}] hasImpacted >= numberOfImpact => [{(package.lData["DEFAULT"] as DefaultData).hasImpacted}] >= [{numberOfImpact}]");

        if ((package.lData["DEFAULT"] as DefaultData).hasImpacted >= numberOfImpact)
        {
            // If we hit a rigidbody, apply force to it
            if(hitTR is RigidBody3D){
                if (debug) { GD.Print($"PLDefault::ResolveFateAfterImpact() Applying force[{package.ForceVector.Length()}] to the rigidbody we hit"); }

               (hitTR as RigidBody3D).ApplyImpulse(package.ForceVector, package.point - hitTR.GlobalPosition);
            }
            if (attachWhenOnImpact)
            {
                //GD.Print($"PLDefault:ResolveFateAfterImpact() Attaching");
                // Attach
                pr.N3D.Position = package.point + pr.GloabalForward * penetration;
                //pr.N3D.Reparent(hitTR);
                pr.Stop();
                return;
            }
            pr.Despawn();
        }
    }



    private void Bounce(IProjectile pr, ref DamagePackage package, float remainingDistance, float delta)
    {
        //GD.Print($"PLDefault:Bounce()");

        (package.lData["DEFAULT"] as DefaultData).hasBounced++;
        // Bounce the projectile
        //float remainingDistance = pr.Velocity.magnitude * Time.deltaTime - Vector3.Distance(pr.transform.position, package.point);

        Vector3 OutVelocity = pr.Velocity.Reflect(package.normal);

        remainingDistance -= pr.N3D.Position.DistanceTo(package.point);

        // draw bounce normal
        //Debug.DrawLine(package.point, package.point + package.normal * 0.1f, Colors.Magenta, 2.0f);
        // draw bounce in vector
        //Debug.DrawLine(package.point, package.point + -pr.Velocity.normalized * 0.1f, Color.green, 2.0f);
        // draw bounce out vector
        Vector3 OutPoint = package.point + package.normal * faceoffset * 0.01f;
        //Debug.DrawLine(
        //    OutPoint,
        //    OutPoint + OutVelocity.normalized * 0.1f,
        //    Color.yellow, 2.0f);

        pr.SkipMovementOneFrame = true;
        pr.ChangeVelocity(OutVelocity * damping);
        pr.MovePrecision(OutPoint, OutVelocity, remainingDistance, delta);
        if (faceDirectionAfterBounce) { pr.N3D.LookAt(pr.N3D.Position + pr.Velocity); }
        pr.RaiseBounce(package.hitTransform, pr.Velocity, OutVelocity, remainingDistance);
    }

    #region Self damage negation
    public override void OnFire(IProjectile tr, Vector3 velocity, ref DamagePackage package)
    {
        //GD.Print($"PLDefault:OnFire()");
        
        DefaultData data = new DefaultData();
        if (package.dealer is not null)
        {
            //data.excludedLayer = package.dealer;
            data.timeToExlude = selfDamageWindow;
        }
        //if (tr.GetComponent<IProjectile>().WillHitLayers == (tr.GetComponent<IProjectile>().WillHitLayers | (1 << data.excludedLayer)))
        //{
        //    tr.GetComponent<IProjectile>().RemoveLayer(data.excludedLayer);
        //    data.isExcluding = true;
        //}
        package.lData["DEFAULT"] = data;
    }

    public override void OnTravelUpdate(ref Vector3 velocity, IProjectile proj, ref DamagePackage package, float framedistance)
    {
        DefaultData data = package.lData["DEFAULT"] as DefaultData;
        if (!data.isExcluding) { return; }

        data.timeToExlude -= proj.LastDelta;
        //if (data.timeToExlude < 0.0f)
        //{
        //    transform.GetComponent<IProjectile>().AddLayer(data.excludedLayer);
        //    data.isExcluding = false;
        //}
    }
    #endregion

    public class DefaultData : ProjectileListenerData
    {
        public int hasHit = 0;
        public int hasImpacted = 0;
        public int hasBounced = 0;
        public int excludedLayer = -1;
        public bool isExcluding = false;
        public float timeToExlude = 0.0f;
    }
}//EOF CLASS
