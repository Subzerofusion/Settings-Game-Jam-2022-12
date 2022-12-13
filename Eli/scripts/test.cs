using Godot;
using System;

public partial class test : Node
{
	[Export]
	private int testinput;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Console.WriteLine("heyyy");
	}


}
