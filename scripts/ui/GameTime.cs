using Godot;
using System;

public partial class GameTime : RichTextLabel
{
    [Export]
    public StageManager StageManager { get; private set; }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        TimeSpan time = TimeSpan.FromSeconds(StageManager.GameTime);

        Text = $"TIME: {(int)time.TotalHours:D2}:{time.Minutes:D2}:{time.Seconds:D2}.{time.Milliseconds:D3}";
    }
}
