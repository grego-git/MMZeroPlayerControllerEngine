using Godot;
using System;

public partial class EnemyBlast : Area2D
{
    [Export]
    public float Speed { get; private set; }
    [Export]
    public int Damage { get; private set; }

    public Vector2 ShootDir { get; set;}

    public override void _Ready()
    {
        base._Ready();
        
        BodyEntered += Collision;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        GlobalPosition += ShootDir * Speed * (float)delta;
    }

    public void Collision(Node2D body)
    {
        GD.Print(body.Name);

        if (body.Name.ToString().Contains("Player"))
        {
            PlayerController player = (PlayerController)body;
            player.Hit(ShootDir.X, Damage);
        }

        QueueFree();
    }
}
