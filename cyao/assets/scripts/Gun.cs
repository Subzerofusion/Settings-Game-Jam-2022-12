using Godot;
using System;

public class Gun : AnimatedSprite {
  // Declare member variables here. Examples:
  // private int a = 2;
  // private string b = "text";

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() {

  }

  //  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta) {
  }

  public override void _PhysicsProcess(float delta) {
    base._PhysicsProcess(delta);
    Position = new Vector2(GetViewport().GetMousePosition().x, GetViewport().Size.y);
  }
}
