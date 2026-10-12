using Godot;
using System;

public partial class BlasterEnemy : BaseEnemy
{
    public const float SHOOT_TIMER_DURATION = 3.0f;
    [Export]
    public PackedScene BlastPackedScene { get; set; }

    public bool OnScreen { get; set; }

    private float _shootTimer;
    private float _shootDir;

    public override void _Ready()
    {
        base._Ready();

        _shootTimer = 0.5f;
        _shootDir = -1.0f;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        UpdateShootDir();
        
        if (OnScreen)
        {
            Shoot((float)delta);
        }

        CheckForPlayerCollision();
    }

    private void UpdateShootDir()
    {
        if (Player.GlobalPosition.X < GlobalPosition.X)
        {
            _shootDir = -1.0f;
        }
        else if (Player.GlobalPosition.X > GlobalPosition.X)
        {
            _shootDir = 1.0f;
        }
    }

    private void Shoot(float delta)
    {
        _shootTimer -= delta;
        _shootTimer = Mathf.Max(_shootTimer, 0.0f);

        if (_shootTimer <= 0.0f)
        {
            var blastInstance = (EnemyBlast)BlastPackedScene.Instantiate();
            GetParent().AddChild(blastInstance);

            blastInstance.GlobalPosition = GlobalPosition;
            blastInstance.ShootDir = Vector2.Right * _shootDir;

            _shootTimer = SHOOT_TIMER_DURATION;
        }
    }
}
