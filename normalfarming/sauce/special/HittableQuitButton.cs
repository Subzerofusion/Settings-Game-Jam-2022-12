using Godot;
using System;

namespace Normalfarming.sauce
{
    public class HittableQuitButton : Hittable
    {
        public override void GetHit(Suckable projectile)
        {
            GetTree().Quit(0);
        }
    }
}
