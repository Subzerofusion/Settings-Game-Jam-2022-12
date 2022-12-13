using Godot;
using System;

public class BulletHole : Node2D {
  [Export]
  NodePath _asBulletHolePath;
  AnimatedSprite asBulletHole;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() {
    asBulletHole = GetNode<AnimatedSprite>(_asBulletHolePath);
    asBulletHole.Frame = new Random().Next(0, asBulletHole.Frames.GetFrameCount("default"));
  }
}
