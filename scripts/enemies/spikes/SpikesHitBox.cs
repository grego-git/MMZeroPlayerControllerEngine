using Godot;
using System;

public partial class SpikesHitBox : BaseEnemy
{
    public bool Lethal { get; set; }

    public override void _Ready()
    {
        base._Ready();
        Lethal = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        CheckForPlayerCollision();
    }
}
