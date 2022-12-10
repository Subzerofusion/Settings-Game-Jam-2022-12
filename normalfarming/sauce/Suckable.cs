using Godot;

namespace NormalFarming.sauce
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Suckable : RigidBody
    {
        [Export] public float Succeleration = 3;
        
        private Spatial _suckedTo = null;
        private float _suckPower = 0;

        // // Called when the node enters the scene tree for the first time.
        // public override void _Ready()
        // {
        //
        // }

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