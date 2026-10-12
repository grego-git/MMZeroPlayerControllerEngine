using Godot;
using System;

public partial class BestTime : RichTextLabel
{
    [Export]
    public StageManager StageManager { get; private set; }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (StageManager.FoundBestTime)
        {
            TimeSpan time = TimeSpan.FromSeconds(StageManager.BestTime);
            Text = $"BEST TIME: {(int)time.TotalHours:D2}:{time.Minutes:D2}:{time.Seconds:D2}.{time.Milliseconds:D3}";
            Visible = true;
        }
        else
        {
            Visible = false;
        }
    }
}
