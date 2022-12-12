using Godot;
using System;

namespace Normalfarming.sauce
{
    public class SuckableRock : Suckable
    {
        protected override void OnHitPlayer(Player player)
        {
            player.GetClogged(this);
        }
    }
}
