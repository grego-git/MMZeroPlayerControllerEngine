public class RotatingStomperAttack : IBossAttack
{
    private StomperRotatorManager _stomperRotator;

    public RotatingStomperAttack(StomperRotatorManager stomperRotator)
    {
        _stomperRotator = stomperRotator;
    }

    public void Start()
    {
        _stomperRotator.StartRotator();
    }

    public void Update(float delta)
    {
        _stomperRotator.RotateStompers(delta);
    }

    public bool Complete()
    {
        return _stomperRotator.Complete();
    }
}