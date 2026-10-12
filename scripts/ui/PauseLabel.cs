using Godot;
using System;

public partial class PauseLabel : RichTextLabel
{
    [Export]
    public StageManager StageManager { get; private set; }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Visible = StageManager.CurrentState == StageManager.State.PAUSED;
    }
}
