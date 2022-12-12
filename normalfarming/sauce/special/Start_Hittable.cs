using Godot;
using System;
using System.Diagnostics;

namespace Normalfarming.sauce
{

    public class Start_Hittable : Hittable
    {
        public override void GetHit(Suckable projectile)
        {
            Debug.Print("DO START!");
        }
    }

}
