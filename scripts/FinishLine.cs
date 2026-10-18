using Godot;
using System;

public partial class FinishLine : Area2D
{
    [Export]
    public StageManager StageManager { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        BodyEntered += HitFinishLine;
    }

    private void HitFinishLine(Node2D body)
    {
        if (body.Name == "Player")
        {
            StageManager.StopTimer();
            QueueFree();
        }
    }
}
