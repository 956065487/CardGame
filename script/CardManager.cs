using System;
using System.Reflection.Metadata.Ecma335;
using Godot;
using Godot.Collections;

namespace CardGame.script;

public partial class CardManager : Node2D
{
    #region 常量

    public const int COLLISION_MASK_CARD = 1;
    public const int COLLISION_MASK_CARD_SLOT = 2;

    #endregion

    #region 属性值

    public Node2D CardBeingDragged;
    public Vector2 ScreenSize;
    private Card _card;
    private bool _isHovering;
    private Card _oldCard;

    #endregion

    #region 生命周期中的方法

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        ScreenSize = GetViewportRect().Size;
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
                    CardBeingDragged = card;
                }
            }
            else
            {
                FinishedDragged();
            }
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        var mousePos = GetGlobalMousePosition();
        if (CardBeingDragged != null)
        {
            CardBeingDragged.Position = new Vector2(Mathf.Clamp(mousePos.X, 0, ScreenSize.X), Mathf.Clamp(mousePos.Y,
                0, ScreenSize.Y));
        }
    }

    #endregion


    #region 我的自定义方法

    /**
     * 高亮卡牌
     */
    public void HighLightCard(Card card, bool isHoured)
    {
        if (CardBeingDragged != null)
        {
            return;
        }

        if (isHoured)
        {
            card.Scale = new Vector2((float)1.2, (float)1.2);
            card.ZIndex = 2;
            card.MoveToFront();
        }
        else
        {
            card.Scale = new Vector2(1, 1);
        }
    }

    /**
     * card : 需要连接信号的卡片
     * 连接卡片信号的方法
     */
    public void ConnectCardSignals(Card card)
    {
        if (card == null)
        {
            GD.PrintErr("错误：没能获取到Card信息");
            return;
        }

        card.Hover += OnHoverOverCard;
        card.HoverOff += OnHoverOffCard;
    }

    /**
     * 当鼠标悬停在卡上时
     */
    private void OnHoverOverCard(Card card)
    {
        if (_isHovering)
        {
            HighLightCard(_oldCard, false);
        }
        else
        {
            _isHovering = true;
        }

        _oldCard = card; // 存入当前悬停的卡
        GD.Print("Manager 中的OnHoverOverCard");
        HighLightCard(card, true);
    }

    /**
     * 当鼠标离开卡片时
     */
    private void OnHoverOffCard(Card card)
    {
        // 尝试获取卡片
        Card checkForCard = CheckForCard();

        if (_isHovering && checkForCard == null)
        {
            _isHovering = false;
        }
        else if (checkForCard != null)
        {
            HighLightCard(checkForCard, true);
            _oldCard = checkForCard; //储存旧卡牌
        }

        GD.Print("Manager 中的OnHoverOffCard");
        HighLightCard(card, false);
    }

    /**
     * 鼠标放开，不再拖拽时
     */
    private void FinishedDragged()
    {
        CardSlot cardSlot = CheckForCardSlot();
        if (cardSlot != null && !cardSlot.CardInSlot)
        {
            // 说明在空卡槽上面
            CardBeingDragged.Position = cardSlot.Position;
            CardBeingDragged.GetNode<CollisionShape2D>("Area2D/CollisionShape2D").Disabled = true;
            cardSlot.CardInSlot = true;
        }
        CardBeingDragged = null;
    }

    /**
     * 获取卡片对象，用于拖动逻辑
     * 如果不为 Card 对象，返回Null
     */
    public Card CheckForCard()
    {
        // 获取被点击的Area2d的父级
        PhysicsDirectSpaceState2D spaceState2D = GetWorld2D().DirectSpaceState;
        var parameters2D = new PhysicsPointQueryParameters2D();
        parameters2D.Position = GetGlobalMousePosition();
        parameters2D.CollideWithAreas = true;
        parameters2D.CollisionMask = COLLISION_MASK_CARD;
        Array<Dictionary> result = spaceState2D.IntersectPoint(parameters2D);
        if (result.Count > 0)
        {
            Node2D colliderNode2D = result[0]["collider"].As<Node2D>();
            if (colliderNode2D.GetParent().GetType() != typeof(Card))
            {
                GD.PrintErr("CheckForCard ERROR:错误的类型，目标不为Card对象");
                return null;
            }

            Card card = colliderNode2D.GetParent<Card>();

            if (card != null)
            {
                return card;
            }
        }

        return null;
    }

    public CardSlot CheckForCardSlot()
    {
        // 获取被点击的Area2d的父级
        PhysicsDirectSpaceState2D spaceState2D = GetWorld2D().DirectSpaceState;
        var parameters2D = new PhysicsPointQueryParameters2D();
        parameters2D.Position = GetGlobalMousePosition();
        parameters2D.CollideWithAreas = true;
        parameters2D.CollisionMask = COLLISION_MASK_CARD_SLOT;
        Array<Dictionary> result = spaceState2D.IntersectPoint(parameters2D);
        if (result.Count > 0)
        {
            Node2D colliderNode2D = result[0]["collider"].As<Node2D>();
            if (colliderNode2D.GetParent().GetType() != typeof(CardSlot))
            {
                GD.PrintErr("CheckForSlot ERROR:错误的类型，目标不为CardSlot对象");
                return null;
            }

            CardSlot cardSlot = colliderNode2D.GetParent<CardSlot>();

            if (cardSlot != null)
            {
                return cardSlot;
            }
        }

        return null;
    }

    #endregion
}