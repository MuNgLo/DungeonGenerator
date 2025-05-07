using Godot;

namespace Munglo.Commons
{
    public partial class Hitbox : Node3D
    {
        public bool _debug = false;
        public bool _canTakeDamage = true;
        private Health health;
        private Node _collider;

        private Vector3 _ogLocalPos;
        private Vector3 _ogLocalRot;
        private Vector3 _lastHitPoint = Vector3.Zero;
        private Vector3 _lastHitForce = Vector3.Zero;
        public Munglo.AI.AIUnit unit;

        public void SetupHitbox()
        {
            //_collider = GetComponent<Collider>();
            _ogLocalPos = Position;
            _ogLocalRot = Rotation;
        }

        public void HitBoxOn()
        {
            //_collider.enabled = true;
            //if(unit != null){
            //    if(health.GetComponent<Munglo.AI.AIUnit>()){
            //        unit = health.GetComponent<Munglo.AI.AIUnit>();
            //    }else{
            //        unit = null;
            //    }
            //}
        }
        public void HitBoxOff()
        {
            //_collider.enabled = false;
        }

        public bool TakeDamage(DamagePackage package, bool isLocal = true, bool skipTakeDamageEvent = false)
        {
            if (_debug) { GD.Print($"Hitbox::TakeDamage() on {this.Name}"); }
            SetLastPhysForce(package);
            if (!_canTakeDamage) { package.damage = 0; }
            return  health.TakeDamage(package);
        }

        public bool TakeDirectDamage(DamagePackage package, bool isLocal = true, bool skipTakeDamageEvent=false)
        {
            SetLastPhysForce(package);
            if (_debug) { GD.Print($"Hitbox::TakeDirectDamage() on {this.Name}"); }
            if (!_canTakeDamage) { package.damage = 0; }
            return health.TakeDamage(package);
        }

        private void SetLastPhysForce(DamagePackage package)
        {
            _lastHitPoint = package.point;
            _lastHitForce = package.velocity.Normalized() * package.physicalForce;
        }

        public void ForgetLastPhysForce()
        {
            _lastHitPoint = Vector3.Zero;
            _lastHitForce = Vector3.Zero;
        }

        private void OnEnable()
        {
            //gameObject.layer = 14;
        }
        private void OnDisable()
        {
            HitBoxOff();
            Position = _ogLocalPos;
            Rotation = _ogLocalRot;
            _lastHitPoint = Vector3.Zero;
            _lastHitForce = Vector3.Zero;
    }
    }// EOF CLASS
}