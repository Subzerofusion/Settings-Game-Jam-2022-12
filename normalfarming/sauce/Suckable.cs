using System.Diagnostics;
using Godot;

namespace Normalfarming.sauce
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Suckable : RigidBody
    {
        // ReSharper disable once IdentifierTypo
        [Export] public float Succeleration = 3;
        
        private Spatial _suckedTo = null;
        private float _suckPower = 0;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            ContactMonitor = true;
            ContactsReported = 5;
            var err = Connect("body_entered", this, "_on_body_entered");
            if (err != Error.Ok)
            {
                Debug.Fail($"Error connecting signal: {err}");
            }
        }

        public void _on_body_entered(Node body)
        {
            if (body is Player player)
            {
                OnHitPlayer(player);
            }
            
            if (body.Owner is Hittable hittable)
            {
                hittable.GetHit(this);
            }
        }

        protected virtual void OnHitPlayer(Player player)
        {
            player.GetClogged(this);
        }

        public override void _PhysicsProcess(float delta)
        {
            // Check if I'm being sucked
            if (_suckedTo is null)
            {
                return;
            }

            UpdateSucking(delta);
        }

        protected virtual void UpdateSucking(float delta)
        {
            var target = _suckedTo.GlobalTranslation;
            var direction = target - GlobalTranslation;

            _suckPower += Succeleration * delta;
            
            SetAxisVelocity(direction.Normalized() * _suckPower);
        }

        public void BeSucked(Spatial suckTarget)
        {
            GravityScale = 0;
            // LinearDamp = 1f;
            _suckedTo = suckTarget;
        }

        public void StopSucking()
        {
            GravityScale = 1;
            // LinearDamp = -1.0f;
            _suckedTo = null;
            _suckPower = 0;
        }
        
    }
}
