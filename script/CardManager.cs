using Godot;
using Godot.Collections;

namespace CardGame.script;

public partial class CardManager : Node2D
{
    #region 属性值

    public Node2D cardBeingDragged;

    #endregion
    
    #region 生命周期中的方法

    
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            if (mouseEvent.Pressed)
            {
                GD.Print("Mouse Left Clicked!");
                Node2D card = CheckForCard();
                if (card != null)
                {
                    cardBeingDragged = card;
                }
                

            }
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        var mousePos = GetGlobalMousePosition();
        if (cardBeingDragged != null)
        {
            cardBeingDragged.Position = mousePos; 
        }
        
    }

    #endregion

    #region 我的自定义方法

    public Node2D CheckForCard()
    {
        // 获取被点击的Area2d的父级
        PhysicsDirectSpaceState2D spaceState2D = GetWorld2D().DirectSpaceState;
        var parameters2D = new PhysicsPointQueryParameters2D();
        parameters2D.Position = GetGlobalMousePosition();
        parameters2D.CollideWithAreas = true;
        parameters2D.CollisionMask = 1;
        Array<Dictionary> result = spaceState2D.IntersectPoint(parameters2D);
        Node2D colliderNode2D = result[0]["collider"].As<Node2D>();
        Node2D card = colliderNode2D.GetParent<Node2D>();
        
        if (card != null)
        {
            return card;
        }
        return null;
    }

    #endregion
}