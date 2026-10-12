using Godot;

public partial class BaseEnemy : ShapeCast2D
{
    [Export]
    public PlayerController Player { get; set; }
    [Export]
    public int Health { get; private set; }
    [Export]
    public int Damage { get; private set; }

    public void CheckForPlayerCollision()
    {
        for (int i = 0; i < GetCollisionCount(); i++)
        {
            Node2D collider = (Node2D)GetCollider(i);

            if (collider == Player)
            {
                HitPlayer(Player);
            }
        }
    }

    protected void HitPlayer(PlayerController player)
    {
        float hitDir = 0.0f;

        if (Player.GlobalPosition.X < GlobalPosition.X)
        {
            hitDir = -1.0f;
        }
        else if (Player.GlobalPosition.X > GlobalPosition.X)
        {
            hitDir = 1.0f;
        }

        player.Hit(hitDir, Damage);
    }

    public void Hit(int damage)
    {
        Health -= damage;

        if (Health <= 0)
        {
            QueueFree();
        }
    }
}