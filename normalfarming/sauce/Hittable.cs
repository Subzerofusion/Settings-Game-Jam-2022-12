using System.Diagnostics;
using Godot;

namespace Normalfarming.sauce
{
	public class Hittable : Spatial
	{
		[Export] private float _activationForce = 1;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
		}

		public virtual void GetHit(Suckable projectile)
		{
			Debug.Print("Struck by a suckable");
		}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
	}
}
