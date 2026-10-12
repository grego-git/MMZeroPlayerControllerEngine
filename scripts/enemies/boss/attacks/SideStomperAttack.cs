public class SideStomperAttack : IBossAttack
{
    private SideStomperManager _sideStomperManager;

    public SideStomperAttack(SideStomperManager stomperRotator)
    {
        _sideStomperManager = stomperRotator;
    }

    public void Start()
    {
        _sideStomperManager.StartSideStomper();
    }

    public void Update(float delta)
    {
        _sideStomperManager.Attack(delta);
    }

    public bool Complete()
    {
        return _sideStomperManager.Complete();
    }
}