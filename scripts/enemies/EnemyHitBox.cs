using Godot;

public partial class EnemyHitBox : Area2D
{
    [Export]
    public BaseEnemy AttachedEnemy { get; private set; }

    public void Hit(int damage)
    {
        AttachedEnemy.Hit(damage);
    }
}
