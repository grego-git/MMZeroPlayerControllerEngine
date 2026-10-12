using Godot;
using System;

public partial class Stomper : Node2D
{
    public const float EXPAND_SPEED = 2.0f;
    public const float RETRACT_SPEED = 0.5f;
    public const float IDLE_DURATION = 2.0f;

    [Export]
    public PlayerController Player { get; set; }
    [Export]
    public float ExpandScale { get; set; }
    [Export]
    public bool InBackground { get; set; }

    private Node2D _pivot;
    private ShapeCast2D _playerDetector;
    private Sprite2D _mainSprite;
    private StomperHitBox _hitBox;

    private float _expandTimer;
    private float _expandIdleTimer;
    private bool _expand;

    public override void _Ready()
    {
        base._Ready();
    
        _pivot = (Node2D)GetNode("Pivot");
        _playerDetector = (ShapeCast2D)GetNode("PlayerDetector");
        _mainSprite = (Sprite2D)_pivot.GetNode("Sprite2D");
        _hitBox = (StomperHitBox)_pivot.GetNode("HitBox");

        _hitBox.Player = Player;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        _mainSprite.Modulate = Colors.Red * (InBackground ? 0.5f : 1.0f);
        _hitBox.Lethal = !InBackground;
        ZIndex = InBackground ? -10 : 10;
        
        if (ShouldExpand())
        {
            _expand = true;
            _expandIdleTimer = 0.0f;
        }

        if (_expand)
        {
            Expand((float)delta);
        }
        else
        {
            Retract((float)delta);
        }

        _pivot.Scale = new Vector2(1.0f, 1.0f).Lerp(new Vector2(1.0f, ExpandScale), _expandTimer);
    }

    private bool ShouldExpand()
    {
        return !InBackground && IsPlayerInAttackArea() && !_expand && _expandTimer == 0.0f;
    }

    private bool IsPlayerInAttackArea()
    {
        for (int i = 0; i < _playerDetector.GetCollisionCount(); i++)
        {
            Node2D collider = (Node2D)_playerDetector.GetCollider(i);

            if (collider.Name.ToString().Contains("Player"))
            {
                return true;
            }
        }

        return false;
    }

    private void Expand(float delta)
    {
        _expandTimer += delta * EXPAND_SPEED;
        _expandTimer = Mathf.Clamp(_expandTimer, 0.0f, 1.0f);

        if (_expandTimer == 1.0f)
        {
            if (InBackground)
            {
                _expandIdleTimer = IDLE_DURATION;
            }
            else
            {
                _expandIdleTimer += delta;
                _expandIdleTimer = Mathf.Clamp(_expandIdleTimer, 0.0f, IDLE_DURATION);
            }

            if (_expandIdleTimer == IDLE_DURATION)
            {
                _expand = false;
            }
        }
    }

    private void Retract(float delta)
    {
        _expandTimer -= (float)delta * RETRACT_SPEED;
        _expandTimer = Mathf.Clamp(_expandTimer, 0.0f, 1.0f);
    }
}
