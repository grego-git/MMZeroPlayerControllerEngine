using Godot;

public class Buffer
{
    private float bufferTimer;
    private float bufferDuration;
    

    public Buffer(float duration)
    {
        bufferDuration = duration;
    }

    public void StartBuffer()
    {
        bufferTimer = bufferDuration;
    }

    public void DrainBuffer(float delta)
    {
        bufferTimer -= delta;
        bufferTimer = Mathf.Max(bufferTimer, 0.0f);
    }

    public void ClearBuffer()
    {
        bufferTimer = 0.0f;
    }

    public bool Buffered()
    {
        return bufferTimer > 0.0f;
    }
}