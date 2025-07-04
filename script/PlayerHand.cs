using Godot;
using System;
using System.Collections.Generic;
using CardGame.script;

public partial class PlayerHand : Node2D
{
    #region 常量、属性

    // 常量
    // public const int CARD_COUNT_IN_HAND = 8;
    private const int CARD_WIDTH = 200;
    private const float HAND_Y_POSITION = 880;  // 手牌上的Y坐标

    // 变量
    private List<Card> _playerHandCards = new List<Card>();
    private float _centerScreenX;
    private float _centerScreenY;

    #endregion

    #region 默认生命周期方法

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _centerScreenX = GetViewportRect().Size.X / 2;
        _centerScreenY = GetViewportRect().Size.Y / 2;
        GD.Print("viewport  X= " + _centerScreenX);
        
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    #endregion

    #region 自定义方法

    /**
     * 将卡片添加到手中
     */
    public void AddToHand(Card card)
    {
        if (card == null)
        {
            return;
        }
        if (!_playerHandCards.Contains(card))
        {
           // 卡片不再手牌中时 
           _playerHandCards.Insert(0,card);
           UpdateHandPositions();
        }
        else
        {
            AnimateCardToPosition(card,card.PositionInHand);
        }
    }

    /**
     * 更新手牌位置
     */
    private void UpdateHandPositions()
    {
        for (int i = 0; i < _playerHandCards.Count; i++)
        {
            Vector2 newPosition = new Vector2(CalculateCardPositon(i),HAND_Y_POSITION);
            Card card = _playerHandCards[i];
            if (card == null)
            {
                continue;
            }
            card.PositionInHand = newPosition;
            AnimateCardToPosition(card, newPosition);
        }
    }
    
    /**
     * 动画移动卡牌位置
     */
    private void AnimateCardToPosition(Card card, Vector2 newPosition)
    {
        var tween = GetTree().CreateTween();
        tween.TweenProperty(card, "position", newPosition,1);

    }


    /**
     * 按手牌下标计算卡牌位置(X)
     * index: 卡牌在手中的下标
     */
    private float CalculateCardPositon(int index)
    {
        var totalWidth = (_playerHandCards.Count - 1) * CARD_WIDTH;
        var xOffset = _centerScreenX + index*CARD_WIDTH - totalWidth / 2;
        return xOffset;
    }

    /**
     * 从手牌中移除卡片
     */
    public void RemoveCardFromHand(Card card)
    {
        if (_playerHandCards.Contains(card))
        {
            _playerHandCards.Remove(card);
            UpdateHandPositions();
        }
    }

    #endregion


}