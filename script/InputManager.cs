using Godot;
using System;
using CardGame.script;
using Godot.Collections;

public partial class InputManager : Node2D
{
    #region 常量、变量、信号

    public const int COLLISION_MASK_CARD = 1;
    public const int COLLISION_MASK_DECK = 2;
    
    public CardManager CardManager;
    public Deck Deck;
    
    [Signal]
    public delegate void LeftMouseClickedEventHandler();
    
    [Signal]
    public delegate void LeftMouseReleasedEventHandler();

    #endregion

    #region 生命周期方法

    public override void _Ready()
    {
        CardManager = GetNode<CardManager>("/root/Main/CardManager");
        Deck = GetNode<Deck>("/root/Main/Deck");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            if (mouseEvent.Pressed)
            {
                EmitSignalLeftMouseClicked();
                // 
                Node2D checkForCursor = CheckForCursor();
                if (checkForCursor is Card)
                {
                    CardManager.CardBeingDragged = (Card)checkForCursor;
                }
                
            }
            else
            {
                EmitSignalLeftMouseReleased();
                FinishedDragged();
            }
        }
    }

    #endregion

    #region 自定义方法

    /**
     * 获取光标下的东西
     * 如果不为 Card 对象，返回Null
     */
    public Node2D CheckForCursor()
    {
        // 获取被点击的Area2d的父级
        PhysicsDirectSpaceState2D spaceState2D = GetWorld2D().DirectSpaceState;
        var parameters2D = new PhysicsPointQueryParameters2D();
        parameters2D.Position = GetGlobalMousePosition();
        parameters2D.CollideWithAreas = true;
        Array<Dictionary> result = spaceState2D.IntersectPoint(parameters2D);
        if (result.Count > 0)
        {
            //判断 CollisionMask 是属于 什么类型的对象
            Node2D colliderNode2D = result[0]["collider"].As<Node2D>();
            if (colliderNode2D == null)
            {
                Utils.PrintErr(this,"colliderNode2D is null");
            }
            CollisionObject2D colliderObject = colliderNode2D as CollisionObject2D;
            if (colliderObject == null)
            {
                Utils.PrintErr(this,"colliderNode2D is not a CollisionObject2D");
            }

            var resultCollisionMask = colliderObject.CollisionMask;
            if (resultCollisionMask == COLLISION_MASK_CARD)
            {
                // Card对象 被 点击
                if (colliderObject.GetParent().GetType() == typeof(Card))
                {
                    return colliderObject.GetParent() as Card;
                }

            } else if (resultCollisionMask == COLLISION_MASK_DECK)
            {
                // 牌组 被 点击
                if (colliderObject.GetParent() is CardSlot)
                {
                    return colliderObject.GetParent() as CardSlot;
                }
            } 
        }
        return null;
    }
    
    /**
     * 鼠标放开，不再拖拽时
     */
    private void FinishedDragged()
    {
        Node2D checkForCursor = CheckForCursor();
        CardSlot cardSlot = null;
        if (checkForCursor is CardSlot)
        {
            cardSlot = checkForCursor as CardSlot;
        }
        
        if (cardSlot != null && !cardSlot.CardInSlot && CardManager.CardBeingDragged != null)
        {
            // 说明在空卡槽上面
            CardManager.CardBeingDragged.Position = cardSlot.Position;
            CardManager.CardBeingDragged.GetNode<CollisionShape2D>("Area2D/CollisionShape2D").Disabled = true;
            cardSlot.CardInSlot = true;
            CardManager.PlayerHandNode2d.RemoveCardFromHand(CardManager.CardBeingDragged);
        }
        else
        {
            CardManager.PlayerHandNode2d.AddToHand(CardManager.CardBeingDragged);
        }
        CardManager.CardBeingDragged = null;
    }

    #endregion
}