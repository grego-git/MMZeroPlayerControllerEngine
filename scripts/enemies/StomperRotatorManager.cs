using Godot;

public partial class StomperRotatorManager : Node2D
{
    [Export]
    public Vector2 Bounds { get; set; }
    [Export]
    public Stomper[] Stompers { get; set; }
    [Export]
    public float RotateSpeed { get; set; }
    [Export]
    public float Timer { get; private set; }
    [Export]
    public bool Running { get; private set; }

    private float[] _initXPositions;

    public override void _Ready()
    {
        base._Ready();

        _initXPositions = new float[Stompers.Length];

        for (int i = 0; i < _initXPositions.Length; i++)
        {
            _initXPositions[i] = Stompers[i].GlobalPosition.X;
        }
    }

    public void StartRotator()
    {
        Timer = 25.0f;
        Running = true;
    }

    public void RotateStompers(float delta)
    {
        for (int i = 0; i < Stompers.Length; i++)
        {
            Stomper stomper = Stompers[i];
            Vector2 dir = GetRotateDir(stomper);

            stomper.GlobalPosition += dir * RotateSpeed * delta;
            
            if (stomper.InBackground)
            {
                if (Running)
                {
                    if (stomper.GlobalPosition.X <= Bounds.X)
                    {
                        stomper.GlobalPosition = new Vector2(Bounds.X, stomper.GlobalPosition.Y);
                        stomper.InBackground = false;
                    }
                }
                else
                {
                    if (stomper.GlobalPosition.X <= _initXPositions[i])
                    {
                        stomper.GlobalPosition = new Vector2(_initXPositions[i], stomper.GlobalPosition.Y);
                    }
                }
            }
            else if (!stomper.InBackground && stomper.GlobalPosition.X >= Bounds.Y)
            {
                stomper.GlobalPosition = new Vector2(Bounds.Y, stomper.GlobalPosition.Y);
                stomper.InBackground = true;
            }
        }

        DrainTimer(delta);
    }

    public bool Complete()
    {
        if (Running)
        {
            return false;
        }

        foreach (Stomper stomper in Stompers)
        {
            if (!stomper.InBackground)
            {
                return false;
            }
        }

        return true;
    }

    public void Destroy()
    {
        foreach (Stomper stomper in Stompers)
        {
            stomper.QueueFree();
        }

        QueueFree();
    }

    private Vector2 GetRotateDir(Stomper stomper)
    {
        if (stomper.InBackground)
        {
            return Vector2.Left;    
        }
        else
        {
            return Vector2.Right;
        }
    }

    private void DrainTimer(float delta)
    {
        Timer -= delta;
        Timer = Mathf.Max(Timer, 0.0f);

        if (Timer == 0.0f)
        {
            Running = false;
        }
    }
}
