using Godot;
using System;

public partial class StageManager : Node2D
{
    public const float DEAD_DURATION = 1.0f;
    public const float FRAME_DURATION = 1.0f / 60.0f;

    private const string SAVE_PATH = "user://best_time.save";

    public enum State
    {
        PAUSED,
        RUNNING,
        LOST,
        WON
    }

    public State CurrentState { get; private set; }
    public float GameTime { get; private set; }
    public float BestTime { get; private set; }
    public bool FoundBestTime { get; private set;}
    
    private bool _tickDeadTimer;
    private bool _stopTimer;
    private float _deadTimer;
    private float _freezeTimer;

    public override void _Ready()
    {
        base._Ready();

        LoadBestTime();

        CurrentState = State.RUNNING;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        switch (CurrentState)
        {
            case State.PAUSED:
                if (Input.IsActionJustPressed("pause"))
                {
                    CurrentState = State.RUNNING;
                    GetTree().Paused = false;
                    break;
                }

                break;
            case State.RUNNING:
                if (Input.IsActionJustPressed("pause"))
                {
                    CurrentState = State.PAUSED;
                    GetTree().Paused = true;
                    break;
                }

                CheckForFreeze((float)delta);
                GameTime += _stopTimer ? 0.0f : (float)delta;
                break;
            case State.LOST:
            case State.WON:
                if (_tickDeadTimer)
                {
                    DrainDeadTimer((float)delta);
                }
                
                CheckForFreeze((float)delta);

                if (_deadTimer == 0.0f)
                {
                    GetTree().ReloadCurrentScene();
                }
                break;
        }
    }

    private void CheckForFreeze(float delta)
    {
        if (_freezeTimer > 0.0f)
        {
            _freezeTimer -= delta;
            _freezeTimer = Mathf.Max(_freezeTimer, 0.0f);
            
            GetTree().Paused = true;
            return;
        }

        GetTree().Paused = false;
    }

    private void DrainDeadTimer(float delta)
    {
        _deadTimer -= delta;
        _deadTimer = Mathf.Max(_deadTimer, 0.0f);
    }

    public void FreezeFrame(int frames)
    {
        _freezeTimer = FRAME_DURATION * (float)frames;
    }

    public void StopTimer()
    {
        _stopTimer = true;
    }

    public void Lost()
    {
        _tickDeadTimer = true;
        
        if (CurrentState == State.WON)
        {
            return;
        }
        
        CurrentState = State.LOST;
        _deadTimer = DEAD_DURATION;
    }

    public void Won()
    {
        CurrentState = State.WON;
        _deadTimer = DEAD_DURATION;
        SaveBestTime();
    }

    public void LoadBestTime()
    {
        if (FileAccess.FileExists(SAVE_PATH))
        {
            FoundBestTime = true;

            using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Read);
            BestTime = file.GetFloat();
        }
        else
        {
            BestTime = float.MaxValue;
            FoundBestTime = false;
        }
    }

    public void SaveBestTime()
    {
        if (GameTime < BestTime)
        {
            BestTime = GameTime;

            using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Write);
            file.StoreFloat(BestTime);
        }
    }
}
