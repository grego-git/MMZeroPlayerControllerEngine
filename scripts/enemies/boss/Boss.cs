using System.Collections.Generic;
using Godot;

public partial class Boss : Node2D
{
    public const float HIT_STUN_DURATION = 1.0f;
    public const float HIT_FLICKER_DURATION = 0.05f;
    public const float ENTER_DURATION = 2.0f;

    public enum BossState
    {
        WAITING,
        FIGHT
    }

    public BossState CurrentState { get; private set; }

    [Export]
    public int MaxHealth { get; private set; }
    public int Health { get; private set; }

    [Export]
    public PlayerController Player { get; private set; }
    [Export]
    public StomperRotatorManager StomperRotatorManager { get; private set; }
    [Export]
    public SideStomperManager SideStomperManager { get; private set; }

    private Sprite2D _sprite;
    private VisibleOnScreenNotifier2D _notifier;

    private float _hitStunTimer;
    private float _hitFlickerTimer;

    private List<IBossAttack> _bossAttacks;
    private int _bossAttackIndex;

    private bool _enterFight;
    private float _enterTimer;

    public override void _Ready()
    {
        base._Ready();

        Health = MaxHealth;
        
        _sprite = (Sprite2D)GetNode("Sprite2D");
        _notifier = (VisibleOnScreenNotifier2D)GetNode("VisibleOnScreenNotifier2D");

        _bossAttacks = new List<IBossAttack>
        {
            new RotatingStomperAttack(StomperRotatorManager),
            new SideStomperAttack(SideStomperManager)
        };

        _bossAttackIndex = -1;

        _enterFight = false;
        _enterTimer = ENTER_DURATION;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (!_enterFight && _notifier.IsOnScreen() && Player.PlayerState.IsOnFloor())
        {
            _enterFight = true;
        }

        switch (CurrentState)
        {
            case BossState.WAITING:
                if (_enterFight)
                {
                    EnterFight((float)delta);
                    
                    if (_enterTimer == 0.0f)
                    {
                        CurrentState = BossState.FIGHT;
                    }
                    
                    break;
                }
                
                _sprite.Modulate = new Color(0.5f, 0.5f, 0.5f, 1.0f);
                return;
            case BossState.FIGHT:
                if (Health <= 0)
                {
                    break;
                }

                if (_bossAttackIndex == -1)
                {
                    _bossAttackIndex = 0;
                    _bossAttacks[_bossAttackIndex].Start();
                }

                _bossAttacks[_bossAttackIndex].Update((float)delta);

                if (_bossAttacks[_bossAttackIndex].Complete())
                {
                    _bossAttackIndex++;
                    _bossAttackIndex = _bossAttackIndex == _bossAttacks.Count ? 0 : _bossAttackIndex;
                    _bossAttacks[_bossAttackIndex].Start();
                }
                break;
        }

        DrainHitStunTimer((float)delta);
        
        if (IsHit())
        {
            DrainFlickerTimer((float)delta);

            if (_hitFlickerTimer == 0.0f)
            {
                Visible = !Visible;
                _hitFlickerTimer = HIT_FLICKER_DURATION;
            }
        }
        else
        {
            Visible = true;
        }
    }

    private void EnterFight(float delta)
    {
        _enterTimer -= delta;
        _enterTimer = Mathf.Max(_enterTimer, 0.0f);
        _sprite.Modulate = Colors.White.Lerp(new Color(0.5f, 0.5f, 0.5f, 1.0f), _enterTimer / ENTER_DURATION);
    }

    private void DrainHitStunTimer(float delta) 
    {
        _hitStunTimer -= delta;
        _hitStunTimer = Mathf.Max(_hitStunTimer, 0.0f);
    }

    private void DrainFlickerTimer(float delta) 
    {
        _hitFlickerTimer -= delta;
        _hitFlickerTimer = Mathf.Max(_hitFlickerTimer, 0.0f);
    }

    public void Hit(int damage)
    {
        if (CurrentState == BossState.WAITING)
        {
            return;
        }

        if (IsHit())
        {
            return;
        }

        Health -= damage;

        if (Health <= 0)
        {
            Destroy();
        }
        else
        {
            _hitStunTimer = HIT_STUN_DURATION;
            _hitFlickerTimer = HIT_FLICKER_DURATION;
        }
    }

    public bool IsHit()
    {
        return _hitStunTimer > 0.0f;
    }

    public void Destroy()
    {
        Player.StageManager.Won();
        StomperRotatorManager.Destroy();
        SideStomperManager.Destroy();
        QueueFree();
    }
}
