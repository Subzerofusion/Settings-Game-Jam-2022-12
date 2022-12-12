using Godot;
using System;

public class ParticleSystem : Spatial
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    private Particles _particles;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _particles = GetNode<Particles>("Particles");
        _particles.Emitting = true;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (!_particles.Emitting)
        {
            (this.Owner ?? this).QueueFree();
        }
    }
}
