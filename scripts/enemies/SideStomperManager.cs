using Godot;

public partial class SideStomperManager : Node2D
{
    public const float DORMANT_DURATION = 5.0f;
    public const float ATTACK_BUILD_UP_DURATION = 2.0f;

    public enum State
    {
        DORMANT,
        ATTACK_1,
        ATTACK_2,
        HOLD
    }

    [Export]
    public PlayerController Player { get; private set; }
    [Export]
    public Vector2 XBounds { get; private set; }
    [Export]
    public Vector2 YBounds { get; private set; }
    [Export]
    public Stomper[] Stompers { get; private set; }
    [Export]
    public float Timer { get; private set; }

    public State CurrentState { get; private set; }

    private Vector2[] _initPositions;
    private Vector2 _attackTarget;
    private bool _left;

    public override void _Ready()
    {
        base._Ready();

        _initPositions = new Vector2[Stompers.Length];

        for (int i = 0; i < _initPositions.Length; i++)
        {
            _initPositions[i] = Stompers[i].GlobalPosition;
        }
    }

    public void StartSideStomper()
    {
        EnterAttack(State.ATTACK_1);
    }

    public void Attack(float delta)
    {
        switch (CurrentState)
        {
            case State.DORMANT:
                DrainTimer(delta);
                PositionForDormant();
                break;
            case State.ATTACK_1:
                DrainTimer(delta);
                PositionStomperForAttack(1);
                CheckForEndOfAttack(1);
                break;
            case State.ATTACK_2:
                DrainTimer(delta);
                PositionStomperForAttack(2);
                CheckForEndOfAttack(2);
                break;
            case State.HOLD:
                DrainTimer(delta);

                if (Timer == 0)
                {
                    EnterDormant();
                }
                break;
        }
    }

    public bool Complete()
    {
        return CurrentState == State.DORMANT && Timer == 0.0f;
    }

    public void Destroy()
    {
        foreach (Stomper stomper in Stompers)
        {
            stomper.QueueFree();
        }

        QueueFree();
    }

    private int GetCurrentStomperIndex(int attackIndex)
    {
        return _left ? (attackIndex == 1 ? 2 : 3) : (attackIndex == 1 ? 0 : 1);
    }

    private void DrainTimer(float delta)
    {
        Timer -= delta;
        Timer = Mathf.Max(Timer, 0.0f);
    }

    private void CheckForEndOfAttack(int attackIndex)
    {
        int stomperIndex = GetCurrentStomperIndex(attackIndex);
        Stomper stomper = Stompers[stomperIndex];

        if (Timer == 0.0f)
        {
            stomper.InBackground = false;

            if (attackIndex == 1)
            {
                EnterAttack(State.ATTACK_2);
            }
            else
            {
                EnterHold();
            }
        }
    }

    private void PositionForDormant()
    {
        for (int i = 0; i < Stompers.Length; i++)
        {
            Stompers[i].GlobalPosition = Stompers[i].GlobalPosition.Lerp(_initPositions[i], 1.0f - (Mathf.Max(Timer - (DORMANT_DURATION * 0.9f), 0.0f) / (DORMANT_DURATION * 0.1f)));
        }
    }

    private void PositionStomperForAttack(int attackIndex)
    {
        int stomperIndex = GetCurrentStomperIndex(attackIndex);
        Stomper stomper = Stompers[stomperIndex];
        float weight = Mathf.Pow(1.0f - (Mathf.Max(Timer - 0.25f, 0.0f) / (ATTACK_BUILD_UP_DURATION - 0.25f)), 2.0f);

        if (weight > 0.0f)
        {
            _attackTarget = Player.GlobalPosition + (Vector2.Right * (_left ? -512.0f : 512.0f));
        }

        if (_attackTarget.X < XBounds.X)
        {
            _attackTarget.X = XBounds.X;
        }
        else if (_attackTarget.X > XBounds.Y)
        {
            _attackTarget.X = XBounds.Y;
        }

        if (_attackTarget.Y < YBounds.X)
        {
            _attackTarget.Y = YBounds.X;
        }
        else if (_attackTarget.Y > YBounds.Y)
        {
            _attackTarget.Y = YBounds.Y;
        }

        stomper.GlobalPosition = _initPositions[stomperIndex].Lerp(_attackTarget, weight);
        
    }

    private void EnterAttack(State attackState)
    {
        PickSide();
        CurrentState = attackState;
        Timer = ATTACK_BUILD_UP_DURATION;
    }

    private void EnterHold()
    {
        CurrentState = State.HOLD;
        Timer = ATTACK_BUILD_UP_DURATION;
    }

    private void EnterDormant()
    {
        CurrentState = State.DORMANT;
        Timer = DORMANT_DURATION;

        foreach (Stomper stomper in Stompers)
        {
            stomper.InBackground = true;
        }
    }

    private void PickSide()
    {
        Vector2 leftPoint = new Vector2(XBounds.X, 0.0f);
        Vector2 rightPoint = new Vector2(XBounds.Y, 0.0f);

        float dist = leftPoint.DistanceSquaredTo(Player.GlobalPosition);

        if (dist < rightPoint.DistanceSquaredTo(Player.GlobalPosition))
        {
            _left = false;
        }
        else
        {
            _left = true;
        }
    }    
}
