using System;
using System.Reflection.Metadata.Ecma335;
using CardGame.script.constant;
using Godot;
using Godot.Collections;

namespace CardGame.script;

public partial class CardManager : Node2D
{

    #region 属性值

    public Card CardBeingDragged;
    public Vector2 ScreenSize;
    private Card _card;
    private bool _isHovering;
    private Card _oldCard;
    private InputManager _inputManager;

    public PlayerHand PlayerHandNode2d = new PlayerHand();
    
    #endregion

    #region 生命周期中的方法

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        ScreenSize = GetViewportRect().Size;
        _inputManager = GetNode<InputManager>("/root/Main/InputManager");
        _inputManager.LeftMouseClicked += OnLeftMouseClicked;
        _inputManager.LeftMouseReleased += OnLeftMouseReleased;
        PlayerHandNode2d = GetNode<PlayerHand>("/root/Main/PlayerHand");
        
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
            card.Scale = new Vector2((float)Constant.CARD_HIGH_LIGHT_SIZE, (float)Constant.CARD_HIGH_LIGHT_SIZE);
            card.ZIndex = 2;
            card.MoveToFront();
        }
        else
        {
            card.Scale = new Vector2(Constant.DEFAULT_CARD_SCALE_SIZE, Constant.DEFAULT_CARD_SCALE_SIZE);
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
 
        if (_isHovering && CardBeingDragged == null)
        {
            _isHovering = false;
        }
        else if (CardBeingDragged != null)
        {
            HighLightCard(CardBeingDragged, true);
            _oldCard = CardBeingDragged; //储存旧卡牌
        }

        GD.Print("Manager 中的OnHoverOffCard");
        HighLightCard(card, false);
    }

    /**
     * 当鼠标点击时
     */
    public void OnLeftMouseClicked()
    {
        // GD.Print("OnLeftMouseClicked");
    }

    /**
     * 当鼠标放开时
     */
    public void OnLeftMouseReleased()
    {
        // GD.Print("OnLeftMouseReleased");
    }
    
    #endregion
}