using Godot;
using System;
using System.Collections.Generic;
using CardGame.script;

public partial class PlayerHand : Node2D
{
    #region 常量、属性

    // 常量
    // public const int CARD_COUNT_IN_HAND = 8;
    
    private const float HAND_Y_POSITION = 900;  // 手牌上的Y坐标

    // 变量
    private int _cardWidth;   // 卡牌宽度，用于计算间隔等
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
        if (card == null || card.PositionInCardSlot == true)
        {
            return;
        }
        
        card.SetCheckEnemyCard(false);
        
        if (_cardWidth == 0)
        {
            // 获取卡牌宽度
            Sprite2D cardImgSprite2D = card.GetNode<Sprite2D>("CardImg");
            int currentCardWidth = cardImgSprite2D.Texture.GetWidth();
            this._cardWidth = currentCardWidth;
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
        var totalWidth = (_playerHandCards.Count) * _cardWidth;
        var xOffset = _centerScreenX + index*_cardWidth - totalWidth / 2;
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

    public List<Card> GetPlayerHandCards()
    {
        return _playerHandCards;
    }

    #endregion


}