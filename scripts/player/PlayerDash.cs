using Godot;

public class PlayerDash
{
    public const float DASH_METER_CAPACITY = 0.5f;

    private bool dashing;
    private float dashMeter;

    public void EnterDash()
    {
        dashing = true;
        dashMeter = DASH_METER_CAPACITY;
    }

    public void ExitDash()
    {
        dashing = false;
        dashMeter = 0.0f;
    }

    public void DrainDashMeter(float delta)
    {
        dashMeter -= delta;
        dashMeter = Mathf.Max(dashMeter, 0.0f);
    }

    public bool CanEnterDash()
    {
        return Input.IsActionJustPressed("dash") && !dashing && dashMeter == 0.0f;
    }

    public bool CanEnterWallDash()
    {
        return Input.IsActionPressed("dash") && !dashing && dashMeter == 0.0f;
    }

    public bool IsDashing()
    {
        return Input.IsActionPressed("dash") && dashing && dashMeter > 0.0f;
    }
}