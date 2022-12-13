using Godot;
using System;
using System.Diagnostics;

namespace Normalfarming.sauce
{
    public class HittableChangeSceneButton : Hittable
    {
        [Export] private PackedScene _targetScene;
        
        public override void GetHit(Suckable projectile)
        {
            if (_targetScene is null)
            {
                Debug.Fail($"No target scene set");
                return;
            }

            GetTree().ChangeSceneTo(_targetScene);
        }
    }

}
