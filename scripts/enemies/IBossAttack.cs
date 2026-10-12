using System.ComponentModel;

public interface IBossAttack
{
    public void Start();
    public void Update(float delta);
    public bool Complete();
}