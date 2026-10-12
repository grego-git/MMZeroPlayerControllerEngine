using Godot;

public partial class PlayerAttack : ShapeCast2D
{
    public const float ATTACK_DURATION = 0.3f;
    public const float ACTIVE_DURATION = 0.15f;

    public enum AttackType
    {
        HORIZONTAL,
        DOWNWARD
    }

    [Export]
    public int Damage { get; private set; }

    private PlayerController _playerController;
    private AttackType _currentAttackType;
    private float _attackTimer;

    public override void _Ready()
    {
        base._Ready();

        _playerController = (PlayerController)GetNode("../");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (!IsAttacking())
        {
            Visible = false;
            return;
        }

        if (!IsActive())
        {
            AttackRecovery((float)delta);
            return;
        }

        AttackActive((float)delta);
    }

    private void AttackRecovery(float delta)
    {
        Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 0.25f);
        DrainAttackTimer(delta);
    }

    private void AttackActive(float delta)
    {
        Visible = true;
        Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 1.0f);
        Reposition();
        CheckCollision();
        DrainAttackTimer(delta);
    }

    private void Reposition()
    {
        switch (_currentAttackType)
        {
            case AttackType.HORIZONTAL:
                float xDir;

                if (_playerController.PlayerState.FacingRight)
                {
                    if (_playerController.PlayerState.CurrentState == PlayerState.State.WALL_DRAG)
                    {
                        xDir = -1.0f;
                    }
                    else
                    {
                        xDir = 1.0f;
                    }
                }
                else
                {
                    if (_playerController.PlayerState.CurrentState == PlayerState.State.WALL_DRAG)
                    {
                        xDir = 1.0f;
                    }
                    else
                    {
                        xDir = -1.0f;
                    }
                }

                Position = new Vector2(40.0f * xDir, -12.0f);
                break;
            case AttackType.DOWNWARD:
                Position = new Vector2(0.0f, 48.0f);
                break;
        }
    }

    private void CheckCollision()
    {
        for (int i = 0; i < GetCollisionCount(); i++)
        {
            Node2D collider = (Node2D)GetCollider(i);

            if (collider.Name.ToString().Contains("EnemyHitBox"))
            {
                EnemyHitBox hitBox = (EnemyHitBox)collider;
                hitBox.Hit(Damage);
            }
            else if (collider.Name.ToString().Contains("BossHitBox"))
            {
                BossHitBox hitBox = (BossHitBox)collider;
                hitBox.Hit(Damage);
            }
        }
    }

    private void DrainAttackTimer(float delta)
    {
        _attackTimer -= delta;
        _attackTimer = Mathf.Max(_attackTimer, 0.0f);
    }

    public bool IsAttacking()
    {
        return _attackTimer > 0.0f;
    }

    public bool IsActive()
    {
        return _attackTimer > (ATTACK_DURATION - ACTIVE_DURATION);
    }

    public void Attack(AttackType type)
    {
        _currentAttackType = type;
        _attackTimer = ATTACK_DURATION;
    }

    public void ExitAttack()
    {
        _attackTimer = 0.0f;    
    }
}
