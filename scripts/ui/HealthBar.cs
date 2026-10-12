using Godot;
using System;

public partial class HealthBar : ColorRect
{
    [Export]
    public PlayerController Player { get; private set; }

    private ColorRect _fill;
    private Vector2 _initSize;

    public override void _Ready()
    {
        base._Ready();

        _fill = (ColorRect)GetNode("HealthBarFill");
        _initSize = _fill.Size;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        
        _fill.Size = new Vector2(_initSize.X, 0.0f).Lerp(_initSize, (float)Player.PlayerState.GetHealth() / (float)Player.MaxHealth);
    }
}
