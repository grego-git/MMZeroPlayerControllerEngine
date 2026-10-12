using Godot;
using System;

public partial class BossHitBox : Area2D
{
    [Export]
    public Boss Boss { get; set; }

    public void Hit(int damage)
    {
        Boss.Hit(damage);
    }
}
