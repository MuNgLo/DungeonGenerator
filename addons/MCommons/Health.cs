using System;
using Godot;


namespace Munglo.Commons
{
    [GlobalClass]
    public partial class Health : Node, ICanTakeDamage
    {
        [Export] private bool ignoreLocalDamage = false; // Ignores damage on this components gameobject
        [Export] private int maxLife = 100;
        [Export] private float currentLife = 100;
        [Export] private float customDataUpdateInterval = 0.5f;
        [Export] private float lastCustomDataUpdate = -10.0f;
        [Export] private bool isPlayer = false;
        public float CurrentLife { get => currentLife; private set => currentLife = Mathf.Clamp(value, 0.0f, 100.0f); }
        public bool IsFull => currentLife > maxLife - 0.1f;

        public EventHandler<DamagePackage> OnTookDamage;
        public override void _Ready()
        {
            if (Multiplayer.HasMultiplayerPeer() && Multiplayer.MultiplayerPeer.GetConnectionStatus() == MultiplayerPeer.ConnectionStatus.Connected && Multiplayer.IsServer())
            {
                CurrentLife = maxLife;
            }
        }
        public override void _PhysicsProcess(double delta)
        {
            UpdateCustomData((float)delta);
        }

        public void Heal(float value)
        {
            if(value > 0.0f)
            {
                CurrentLife += value;
            }
        }

        private void UpdateCustomData(float delta)
        {
            lastCustomDataUpdate -= delta;
            if (lastCustomDataUpdate < 0.0f)
            {
                lastCustomDataUpdate = customDataUpdateInterval;
                AI.Debug.AIDebugSignals.Singleton.RaiseCustomDataEvent(
                        new AI.Debug.AICustomDataStruct()
                        {
                            message = "Health",
                            sourceClass = this.GetType().Name, // this.GetType().AssemblyQualifiedName,
                            value = CurrentLife,
                            normalizedValue = CurrentLife / 100.0f
                        }
                    );
            }
        }
    public bool TakeDamage(DamagePackage package, bool isLocalDamage = true, bool skipTookDamageEvent=false)
        {
            if (!GetTree().GetMultiplayer().IsServer()) { return false; }
            CurrentLife = CurrentLife - package.damage;
            if (isPlayer == false)
            {
                /*GameEvents.Events.Units.RaiseUnitTookDamage(new GameEvents.UnitTookDamageEventArguments()
                {
                    damage = package.damage,
                    location = package.point,
                    normal = package.velocity.Normalized()
                });
                */
            }
            if (CurrentLife <= 0.0f)
            {
                Die(package);
                return true;
            }
            return false;
        }

        public bool TakeDirectDamage(DamagePackage package, bool isLocalDamage = true, bool skipTookDamageEvent = false)
        {
            return TakeDamage(package, isLocalDamage, skipTookDamageEvent);
        }
        /// <summary>
        /// Just kill health carrier and raise the event
        /// </summary>
        /// <param name="package"></param>
        public void Die(DamagePackage package)
        {
            if (!isPlayer)
            {
                /*GameEvents.Events.Units.RaiseUnitDeath(
                    new GameEvents.UnitDeathEventArguments()
                    {
                        GlobalPosition = package.point, 
                        node = GetParent() as Node3D,
                        AIObject = GetParent() as AIObject, 
                        dealer = package.dealer.Name
                    }
                    );
                    */
                GetParent().GetParent().QueueFree();
                return;
            }
            else
            {
                //Core.RPCs.RaisePlayerDeath(GetParent().GetParent().GetMultiplayerAuthority(), package.Point, package.Dealer);
                GetParent().GetParent().QueueFree();
            }
        }

        public void SetFullHealth()
        {
            CurrentLife = maxLife;
        }
    }// EOF CLASS
}