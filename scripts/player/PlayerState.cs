using Godot;

public class PlayerState
{
    public const float GRAVITY = 1500.0f;
    public const float FLOOR_OFFSET = 12.0f;
    public const float WALL_DAMP_FACTOR = 40.0f;
    public const float WALL_JUMP_COOL_DOWN = 0.1f;
    public const float HIT_STUN_DURATION = 0.5f;
    public const float INVULNERABLE_DURATION = 1.0f;

    public enum State
    {
        GROUNDED,
        AIR,
        WALL_DRAG,
        DEAD
    };

    public State CurrentState { get; private set; }
    public bool Jumping { get; private set; }
    public bool FacingRight { get; private set; }

    private PlayerController _playerController;
    private PlayerDash _playerDash;

    private bool _leftWallCollision;
    private bool _rightWallCollision;

    private float _wallJumpCoolDownTimer;
    private float _hitStunTimer;
    private float _invulnerableTimer;
    
    private int _health;

    public PlayerState(PlayerController playerController)
    {
        _playerController = playerController;
        _playerDash = new PlayerDash();
        
        SetState(State.AIR);
        SetFacingRight(1.0f);
        
        _health = _playerController.MaxHealth;
    }

    public void SetState(State newState)
    {
        switch (newState)
        {
            case State.WALL_DRAG:
            case State.GROUNDED:
                ExitJump();
                ExitDash();
                break;
        }

        CurrentState = newState;
    }

    public void SetFacingRight(float xInput)
    {
        if (xInput == 0.0f)
        {
            return;
        }

        FacingRight = xInput > 0.0f;
    }

    public void SetLeftWallCollision(bool colliding)
    {
        _leftWallCollision = colliding;
    }

    public void SetRighttWallCollision(bool colliding)
    {
        _rightWallCollision = colliding;
    }

    public void EnterJump()
    {
        SetState(State.AIR);
        Jumping = true;
        _wallJumpCoolDownTimer = 0.0f;
    }
    
    public void EnterWallJump()
    {
        SetState(State.AIR);
        Jumping = true;
        _wallJumpCoolDownTimer = WALL_JUMP_COOL_DOWN;
    }

    public void ExitJump()
    {
        Jumping = false;
        _wallJumpCoolDownTimer = 0.0f;
    }

    public void EnterDead()
    {
        SetState(State.DEAD);
    }

    public void Hit(int damage)
    {
        _hitStunTimer = HIT_STUN_DURATION;
        _invulnerableTimer = INVULNERABLE_DURATION;
        _wallJumpCoolDownTimer = 0.0f;

        _health -= damage;
        _health = Mathf.Max(_health, 0);

        ExitJump();
        ExitDash();
    }

    public void DrainHitStunTimer(float delta)
    {
        _hitStunTimer -= delta;
        _hitStunTimer = Mathf.Max(_hitStunTimer, 0.0f);
    }

    public void DrainInvulnerableTimer(float delta)
    {
        _invulnerableTimer -= delta;
        _invulnerableTimer = Mathf.Max(_invulnerableTimer, 0.0f);
    }

    public void DrainWallJumpCoolDown(float delta)
    {
        _wallJumpCoolDownTimer -= delta;
        _wallJumpCoolDownTimer = Mathf.Max(_wallJumpCoolDownTimer, 0.0f);
    }

    public int GetHealth()
    {
        return _health;
    }

    public bool IsDead()
    {
        return _health == 0;
    }

    public bool IsHit()
    {
        return _hitStunTimer > 0.0f;
    }

    public bool IsInvulnerable()
    {
        return _invulnerableTimer > 0.0f;
    }
    
    public bool Falling()
    {
        return _playerController.Velocity.Y >= 0.0f;
    }

    public bool Rising()
    {
        return _playerController.Velocity.Y < 0.0f;
    }

    public bool IsOnFloor()
    {
        return _playerController.IsOnFloor();
    }

    public bool FloorDetected()
    {
        return _playerController.FloorRayCast.IsColliding();
    }

    public bool LeftWallDetected()
    {
        return _playerController.LeftWallRayCast.IsColliding() && _playerController.LeftWallRayCast.GetCollisionNormal() == Vector2.Right;
    }

    public bool IsOnLeftWall()
    {
        return _leftWallCollision;
    }

    public bool PressingLeftWall(float horizontalInput)
    {
        return !IsHit() && _wallJumpCoolDownTimer == 0.0f && IsOnLeftWall() && horizontalInput < 0.0f;
    }

    public bool RightWallDetected()
    {
        return _playerController.RightWallRayCast.IsColliding() && _playerController.RightWallRayCast.GetCollisionNormal() == Vector2.Left;
    }

    public bool IsOnRightWall()
    {
        return _rightWallCollision;
    }

    public bool PressingRightWall(float horizontalInput)
    {
        return !IsHit() && _wallJumpCoolDownTimer == 0.0f && IsOnRightWall() && horizontalInput > 0.0f;
    }

    public bool OnWallJumpCoolDown()
    {
        return _wallJumpCoolDownTimer > 0.0f;
    }

    #region PLAYER DASH WRAPPER
    public bool CanEnterDash()
    {
        return !IsHit() && _playerDash.CanEnterDash();
    }

    public bool CanEnterWallDash()
    {
        return !IsHit() && _playerDash.CanEnterWallDash();
    }

    public bool IsDashing()
    {
        return _playerDash.IsDashing();
    }

    public void EnterDash()
    {
        _playerDash.EnterDash();
    }

    public void DrainDashMeter(float delta)
    {
        _playerDash.DrainDashMeter(delta);
    }

    public void ExitDash()
    {
        _playerDash.ExitDash();
    }
    #endregion
}