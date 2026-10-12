using Godot;
using System;

public partial class BlasterEnemySpawner : VisibleOnScreenNotifier2D
{
    [Export]
    public PlayerController Player { get; set; }
    [Export]
    public PackedScene BlasterEnemyScene { get; set; }

    private BlasterEnemy _enemy;
    private bool _oldOnScreen;

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        CheckForClearEnemy();

        if (JustGotOnScreen() && _enemy == null)
        {
            SpawnBlasterEnemy();
        }

        if (_enemy != null)
        {
            _enemy.OnScreen = IsOnScreen();
        }

        _oldOnScreen = IsOnScreen();
    }

    private bool JustGotOnScreen()
    {
        return !_oldOnScreen && IsOnScreen();
    }

    private void SpawnBlasterEnemy()
    {
        var enemyInstance = (BlasterEnemy)BlasterEnemyScene.Instantiate();
        enemyInstance.GlobalPosition = GlobalPosition;
        enemyInstance.Player = Player;

        GetParent().GetParent().AddChild(enemyInstance);

        _enemy = enemyInstance;
    }

    private void CheckForClearEnemy()
    {
        if (_enemy != null && _enemy.IsQueuedForDeletion())
        {
            _enemy = null;
        }
    }
}
