using Godot;
using System;

public class SimplePrintCs : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("SimplePrintCs: C# Running...");
	}
}
