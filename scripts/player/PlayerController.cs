using System;
using Godot;

public partial class PlayerController : CharacterBody2D
{
    public const float GRAVITY = 1500.0f;
    public const float FLOOR_OFFSET = 12.0f;
    public const float SPRITE_FLICKER_DURATION = 0.05f;

    [Export]
    public int MaxHealth { get; set; }
    [Export]
    public float Speed { get; set; }
    [Export]
    public float DashSpeed { get; set; }
    [Export]
    public float JumpSpeed { get; set; }
    [Export]
    public float WallDragSpeed { get; set; }

    [Export]
    public StageManager StageManager { get; private set; }
    public PlayerState PlayerState { get; private set; }
    public Buffer JumpButtonPressedBuffer { get; private set; }
    public Buffer DashButtonPressedBuffer { get; private set; }
    public Buffer ExitedLeftWallBuffer { get; private set; }
    public Buffer ExitedRightWallBuffer { get; private set; }
    public Buffer ExitedGroundBuffer { get; private set; }

    public PlayerAttack PlayerAttack { get; private set; }
    public Sprite2D MainSprite { get; private set; }
    public RayCast2D FloorRayCast { get; private set; }
    public RayCast2D LeftWallRayCast { get; private set; }
    public RayCast2D RightWallRayCast { get; private set; }

    private Vector2 _hitDir;
    private float _spriteFlickerTimer;

    public override void _Ready()
    {
        base._Ready();

        PlayerState = new PlayerState(this);
        JumpButtonPressedBuffer = new Buffer(1.0f / 60.0f * 5.0f);
        DashButtonPressedBuffer = new Buffer(1.0f / 60.0f * 5.0f);
        ExitedGroundBuffer = new Buffer(1.0f / 60.0f * 10.0f);
        ExitedLeftWallBuffer = new Buffer(1.0f / 60.0f * 20.0f);
        ExitedRightWallBuffer = new Buffer(1.0f / 60.0f * 20.0f);
        
        PlayerAttack = (PlayerAttack)GetNode("PlayerAttack");
        MainSprite = (Sprite2D)GetNode("MainSprite");
        FloorRayCast = (RayCast2D)GetNode("FloorRayCast");
        LeftWallRayCast = (RayCast2D)GetNode("LeftWallRayCast");
        RightWallRayCast = (RayCast2D)GetNode("RightWallRayCast");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        Vector2 joyStickInput = GetJoyStickInput();

        switch (PlayerState.CurrentState)
        {
            case PlayerState.State.AIR:
                if (ExitAirController(PlayerState.IsHit() ? _hitDir.X : joyStickInput.X))
                {
                    break;
                }
                
                AirController(PlayerState.IsHit() ? _hitDir : joyStickInput, (float)delta);
                break;
            case PlayerState.State.GROUNDED:
                if (ExitGroundController())
                {
                    break;
                }

                GroundController(PlayerState.IsHit() ? _hitDir : joyStickInput, (float)delta);
                break;
            case PlayerState.State.WALL_DRAG:
                if (ExitWallDragController(PlayerState.IsHit() ? _hitDir.X : joyStickInput.X))
                {
                    break;
                }

                WallDragController(PlayerState.IsHit() ? _hitDir : joyStickInput, (float)delta);
                break;
            case PlayerState.State.DEAD:
                DeadController(Vector2.Zero, (float)delta);
                return;
        }

        if (PlayerState.IsInvulnerable())
        {
            _spriteFlickerTimer -= (float)delta;
            _spriteFlickerTimer = Mathf.Max(_spriteFlickerTimer, 0.0f);

            if (_spriteFlickerTimer == 0.0f)
            {
                MainSprite.Visible = !MainSprite.Visible;
                _spriteFlickerTimer = SPRITE_FLICKER_DURATION;
            }
        }
        else
        {
            MainSprite.Visible = true;
            _spriteFlickerTimer = 0.0f;
        }

        PlayerState.DrainHitStunTimer((float)delta);
        PlayerState.DrainInvulnerableTimer((float)delta);

        JumpButtonPressedBuffer.DrainBuffer((float)delta);
        DashButtonPressedBuffer.DrainBuffer((float)delta);
        ExitedGroundBuffer.DrainBuffer((float)delta);
        ExitedLeftWallBuffer.DrainBuffer((float)delta);
        ExitedRightWallBuffer.DrainBuffer((float)delta);
    }
#region CONTROLLERS
    private void AirController(Vector2 joyStickInput, float delta)
    {        
        if (JumpReleased() && PlayerState.Rising())
        {
            Velocity = new Vector2(Velocity.X, 0.0f);
            PlayerState.ExitJump();
        }

        Velocity += Vector2.Down * GRAVITY * delta;
        
        float speed = Speed;

        if (PlayerState.IsDashing())
        {
            speed = DashSpeed;
        }
        else
        {
            PlayerState.ExitDash();
        }

        PlayerState.DrainWallJumpCoolDown(delta);

        if (!PlayerState.OnWallJumpCoolDown())
        {
            Velocity = (Vector2.Right * joyStickInput.X * speed * (PlayerState.IsHit() ? 0.5f : 1.0f)) + (Vector2.Down * Velocity.Y);
        }

        MoveAndSlide();

        CheckWallCollisions();
        PlayerState.SetFacingRight(joyStickInput.X);
        CheckForAttack(PlayerAttack.AttackType.DOWNWARD);
    }

    private void GroundController(Vector2 joyStickInput, float delta)
    {
        float speed = Speed;
        float joyStickX = joyStickInput.X;
        
        if (PlayerState.IsDashing())
        {
            PlayerState.DrainDashMeter(delta);    
            speed = DashSpeed;
            joyStickX = PlayerState.FacingRight ? 1.0f : -1.0f;
        }
        else
        {
            PlayerState.ExitDash();
        }

        Velocity = GetRightDirection() * joyStickX * speed * (PlayerState.IsHit() ? 0.5f : 1.0f);
        GlobalPosition += Velocity * delta;

        if (PlayerState.LeftWallDetected() || 
            PlayerState.RightWallDetected() ||
            !PlayerState.FloorDetected())
        {
            Velocity = Vector2.Zero;
            MoveAndSlide();
        }
        
        ClearWallCollisions();
        SnapToFloor();
        PlayerState.SetFacingRight(joyStickInput.X);
        CheckForAttack(PlayerAttack.AttackType.HORIZONTAL);
    }

    private void WallDragController(Vector2 joyStickInput, float delta)
    {
        Velocity = (Vector2.Right * joyStickInput * Speed) + (Vector2.Down * WallDragSpeed);

        MoveAndSlide();

        CheckWallCollisions();
        SnapToLeftWall();
        SnapToRightWall();
        PlayerState.SetFacingRight(joyStickInput.X);
        CheckForAttack(PlayerAttack.AttackType.HORIZONTAL);
    }

    private void DeadController(Vector2 joyStickInput, float delta)
    {
        MainSprite.Visible = false;
    }
#endregion
#region EXIT CONTROLLER
    private bool ExitAirController(float horizontalInput)
    {
        if (PlayerState.IsDead())
        {
            PlayerState.EnterDead();
            StageManager.Lost();
            return true;
        }

        if (!PlayerState.IsHit() && JumpPressed())
        {
            JumpButtonPressedBuffer.StartBuffer();
        }
        
        if (PlayerState.CanEnterDash())
        {
            DashButtonPressedBuffer.StartBuffer();
        }

        if (JumpButtonPressedBuffer.Buffered())
        {
            if (ExitedGroundBuffer.Buffered())
            {
                Jump(JumpSpeed);
                return true;
            }
            else if ((ExitedLeftWallBuffer.Buffered() || PlayerState.LeftWallDetected()) && horizontalInput >= 0.0f)
            {
                WallJump(JumpSpeed, -1.0f);
                return true;
            }
            else if ((ExitedRightWallBuffer.Buffered() || PlayerState.RightWallDetected()) && horizontalInput <= 0.0f)
            {
                WallJump(JumpSpeed, 1.0f);
                return true;
            }
        }

        if (PlayerState.Falling())
        {
            if (PlayerState.IsOnFloor())
            {
                PlayerState.SetState(PlayerState.State.GROUNDED);
                return true;
            }
            else if (PlayerState.PressingLeftWall(horizontalInput) || PlayerState.PressingRightWall(horizontalInput))
            {
                PlayerState.SetState(PlayerState.State.WALL_DRAG);
                return true;
            }
        }

        return false;
    }

    private bool ExitGroundController()
    {
        if (PlayerState.IsDead())
        {
            PlayerState.EnterDead();
            StageManager.Lost();
            return true;
        }

        if (PlayerState.CanEnterDash() || DashButtonPressedBuffer.Buffered())
        {
            Dash();
        }

        if (!PlayerState.IsHit() && (JumpPressed() || JumpButtonPressedBuffer.Buffered()))
        {
            Jump(JumpSpeed);
            return true;
        }
        else if (!PlayerState.IsOnFloor() && !PlayerState.FloorDetected())
        {
            PlayerState.SetState(PlayerState.State.AIR);

            if (!PlayerState.IsHit())
            {
                ExitedGroundBuffer.StartBuffer();
            }

            return true;
        }

        return false;
    }

    private bool ExitWallDragController(float horizontalInput)
    {
        if (PlayerState.IsDead())
        {
            PlayerState.EnterDead();
            StageManager.Lost();
            return true;
        }

        if (PlayerState.IsHit())
        {
            PlayerState.SetState(PlayerState.State.AIR);
            return true;
        }
        else if (!PlayerState.PressingLeftWall(horizontalInput) && !PlayerState.PressingRightWall(horizontalInput))
        {
            if (PlayerState.LeftWallDetected() || PlayerState.IsOnLeftWall())
            {
                ExitedLeftWallBuffer.StartBuffer();
            }
            else
            {
                ExitedRightWallBuffer.StartBuffer();
            }

            PlayerState.SetState(PlayerState.State.AIR);
            return true;
        }
        else if (JumpPressed() || JumpButtonPressedBuffer.Buffered())
        {
            WallJump(JumpSpeed, horizontalInput);
            return true;
        }
        else if (PlayerState.IsOnFloor())
        {
            PlayerState.SetState(PlayerState.State.GROUNDED);
            return true;
        }

        return false;
    }
#endregion
#region ACTIONS
    private void Jump(float verticalSpeed)
    {
        Velocity = new (Velocity.X, -verticalSpeed);
        
        PlayerState.EnterJump();
        
        if (JumpButtonPressedBuffer.Buffered())
        {
            GD.Print("JUMP BUFFERED");
            JumpButtonPressedBuffer.ClearBuffer();
        }

        if (ExitedGroundBuffer.Buffered())
        {
            GD.Print("GROUND EXIT BUFFERED");
            ExitedGroundBuffer.ClearBuffer();
        }
    }

    private void WallJump(float verticalSpeed, float horizontalInput)
    {
        float horizontalDir = -Mathf.Sign(horizontalInput);
        float speed = Speed;

        if (PlayerState.CanEnterWallDash())
        {
            Dash();
            speed = DashSpeed;
        }
        
        PlayerState.SetFacingRight(horizontalDir);

        Velocity = new Vector2(horizontalDir * speed, -verticalSpeed);

        PlayerState.EnterWallJump();
        
        if (JumpButtonPressedBuffer.Buffered())
        {
            GD.Print("JUMP BUFFERED");
            JumpButtonPressedBuffer.ClearBuffer();
        }

        if (ExitedGroundBuffer.Buffered())
        {
            GD.Print("GROUND EXIT BUFFERED");
            ExitedGroundBuffer.ClearBuffer();
        }

        if (ExitedLeftWallBuffer.Buffered() || ExitedRightWallBuffer.Buffered())
        {
            GD.Print("WALL JUMP BUFFERED");
            ExitedLeftWallBuffer.ClearBuffer();
            ExitedRightWallBuffer.ClearBuffer();
        }
    }

    private void Dash()
    {
        PlayerState.EnterDash();

        if (DashButtonPressedBuffer.Buffered())
        {
            GD.Print("DASH BUFFERED");
            DashButtonPressedBuffer.ClearBuffer();
        }
    }

    public void Hit(float hitDir, int damage)
    {
        if (PlayerState.CurrentState == PlayerState.State.DEAD)
        {
            return;    
        }

        if (PlayerState.IsInvulnerable())
        {
            return;
        }

        if (hitDir == 0.0f)
        {
            _hitDir = Vector2.Right * (PlayerState.FacingRight ? -1.0f : 1.0f);
        }
        else
        {
            _hitDir = Vector2.Right * hitDir;
        }

        JumpButtonPressedBuffer.ClearBuffer();
        DashButtonPressedBuffer.ClearBuffer();
        ExitedGroundBuffer.ClearBuffer();
        ExitedLeftWallBuffer.ClearBuffer();
        ExitedRightWallBuffer.ClearBuffer();

        PlayerState.Hit(damage);
        PlayerAttack.ExitAttack();
    }

    public void CheckForAttack(PlayerAttack.AttackType type)
    {
        if (!PlayerState.IsHit() && AttackPressed() && !PlayerAttack.IsAttacking())
        {
            PlayerAttack.Attack(type);
        }
    }
#endregion
#region INPUT
    private Vector2 GetJoyStickInput()
    {
        return new Vector2(Input.GetAxis("ui_left", "ui_right"), Input.GetAxis("ui_down", "ui_up"));
    }

    private bool JumpPressed()
    {
        return Input.IsActionJustPressed("jump");
    }

    private bool AttackPressed()
    {
        return Input.IsActionJustPressed("attack");
    }

    private bool JumpReleased()
    {
        return PlayerState.Jumping && Input.IsActionJustReleased("jump");
    }
#endregion
#region READJUST POSITION
    private void SnapToFloor()
    {
        FloorRayCast.ForceRaycastUpdate();

        if (PlayerState.FloorDetected())
        {
            Vector2 floorPoint = FloorRayCast.GetCollisionPoint();
            Vector2 floorOffsetVector = Vector2.Up * FLOOR_OFFSET;

            GlobalPosition = floorPoint + floorOffsetVector;
        }
    }

    private void SnapToLeftWall()
    {
        LeftWallRayCast.ForceRaycastUpdate();

        if (PlayerState.LeftWallDetected())
        {
            Vector2 wallPoint = LeftWallRayCast.GetCollisionPoint();
            Vector2 wallOffsetVector = Vector2.Right * FLOOR_OFFSET;

            GlobalPosition = wallPoint + wallOffsetVector;
        }
    }

    private void SnapToRightWall()
    {
        RightWallRayCast.ForceRaycastUpdate();

        if (PlayerState.RightWallDetected())
        {
            Vector2 wallPoint = RightWallRayCast.GetCollisionPoint();
            Vector2 wallOffsetVector = Vector2.Left * FLOOR_OFFSET;

            GlobalPosition = wallPoint + wallOffsetVector;
        }
    }
#endregion
    private Vector2 GetRightDirection()
    {
        Vector2 right = Vector2.Right;

        if (PlayerState.FloorDetected())
        {
            Vector2 floorNormal = FloorRayCast.GetCollisionNormal();
            float angleToFloorNormal = Vector2.Up.AngleTo(floorNormal);

            right = right.Rotated(angleToFloorNormal);
        }

        return right;
    }

    private void CheckWallCollisions()
    {
        ClearWallCollisions();

        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            KinematicCollision2D collision = GetSlideCollision(i);
            Vector2 normal = collision.GetNormal();
            
            if (normal == Vector2.Right)
            {
                PlayerState.SetLeftWallCollision(true);
            }
            else if (normal == Vector2.Left)
            {
                PlayerState.SetRighttWallCollision(true);
            }
        }
    }

    private void ClearWallCollisions()
    {
        PlayerState.SetLeftWallCollision(false);
        PlayerState.SetRighttWallCollision(false);
    }
}
