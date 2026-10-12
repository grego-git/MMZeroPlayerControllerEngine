using Godot;
using System;

public partial class AfterImageSpriteEffect : Node2D
{
    private PlayerController playerController;
    private Sprite2D[] sprites;
    private Vector2[] afterImagePositions;

    public override void _Ready()
    {
        base._Ready();

        playerController = (PlayerController)GetNode("../");

        sprites = new Sprite2D[3];

        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i] = (Sprite2D)GetNode($"AfterImageSprite{i + 1}");
        }

        afterImagePositions = new Vector2[3];
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    
        if (playerController.PlayerState.IsDashing())
        {
            AfterImageEffect();
        }
        else
        {
            ClampAfterImagesToMainSprite();
        }
    }

    private void AfterImageEffect()
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            Vector2 deltaTranslate = playerController.MainSprite.GlobalPosition - afterImagePositions[i];
            afterImagePositions[i] += deltaTranslate * (0.33f / (float)(i + 1));
            sprites[i].GlobalPosition = afterImagePositions[i];
            sprites[i].Visible = true;
        }
    }

    private void ClampAfterImagesToMainSprite()
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            afterImagePositions[i] = playerController.MainSprite.GlobalPosition;
            sprites[i].GlobalPosition = afterImagePositions[i];
            sprites[i].Visible = false;
        }
    }
}
