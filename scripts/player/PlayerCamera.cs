using Godot;
using System;

public partial class PlayerCamera : Camera2D
{
    [Export]
    public PlayerController Player { get; set; }
    
    [Export]
    public float YDampFactor { get; set; }
    [Export]
    public float MaxYDelta { get; set; }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        GlobalPosition = new Vector2(Player.GlobalPosition.X, GetYPosition((float)delta));
    }

    private float GetYPosition(float delta)
    {
        float yDelta = Player.GlobalPosition.Y - GlobalPosition.Y;

        if (Mathf.Abs(yDelta) > MaxYDelta)
        {
            return ClampYPosition(yDelta);
        }
        else
        {
            return GlobalPosition.Y + (yDelta * YDampFactor * delta);
        }
    }

    private float ClampYPosition(float yDelta)
    {
        float yClamp = Mathf.Sign(yDelta) * -MaxYDelta; 
        return Player.GlobalPosition.Y + yClamp;
    }
}
