using System.Collections.Generic;
using Godot;

namespace CardGame.script;

public partial class EnemyHand : Node2D
{
    #region 常量、属性

    // 常量
    // public const int CARD_COUNT_IN_HAND = 8;

    private const float HAND_Y_POSITION = 120; // 手牌上的Y坐标

    // 变量
    private int _cardWidth; // 卡牌宽度，用于计算间隔等
    private List<EnemyCard> _enemyHandCards = new List<EnemyCard>();
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
    public void AddToHand(EnemyCard card)
    {
        if (card == null)
        {
            return;
        }

        if (_cardWidth == 0)
        {
            // 获取卡牌宽度
            Sprite2D cardImgSprite2D = card.GetNode<Sprite2D>("CardImg");
            int currentCardWidth = cardImgSprite2D.Texture.GetWidth();
            this._cardWidth = currentCardWidth;
        }


        if (!_enemyHandCards.Contains(card))
        {
            // 卡片不再手牌中时 
            _enemyHandCards.Insert(0, card);
            UpdateHandPositions();
        }
        else
        {
            AnimateCardToPosition(card, card.PositionInHand);
        }
    }

    /**
     * 更新手牌位置
     */
    public void UpdateHandPositions()
    {
        for (int i = 0; i < _enemyHandCards.Count; i++)
        {
            Vector2 newPosition = new Vector2(CalculateCardPositon(i), HAND_Y_POSITION);
            Card card = _enemyHandCards[i];
            if (card == null)
            {
                continue;
            }

            card.PositionInHand = newPosition;
            AnimateCardToPosition(card, newPosition);
        }
    }

    /**
     * 动画移动卡牌位置，默认不翻转
     */
    public void AnimateCardToPosition(Card card, Vector2 newPosition)
    {
        var tween = GetTree().CreateTween();
        tween.TweenProperty(card, "position", newPosition, 1);
    }
    
    /**
     * 动画移动卡牌位置，changeCardDirection为true时，翻转
     */
    public void AnimateCardToPosition(Card card, Vector2 newPosition,bool changeCardDirection)
    {
        if (changeCardDirection)
        {
            // 播放动画并更新位置
            AnimateCardToPosition(card, newPosition);
            card.GetNode<AnimationPlayer>("AnimationPlayer").Play("CardSlip");
            card.ZIndex = 2;
        }
        else
        {
            // 更新位置
            AnimateCardToPosition(card, newPosition);
        }
    }


    /**
     * 按手牌下标计算卡牌位置(X)
     * index: 卡牌在手中的下标
     */
    private float CalculateCardPositon(int index)
    {
        var totalWidth = (_enemyHandCards.Count - 1) * _cardWidth;
        var xOffset = _centerScreenX - index * _cardWidth + totalWidth / 2;
        return xOffset;
    }

    /**
     * 从手牌中移除卡片
     */
    public void RemoveCardFromHand(EnemyCard card)
    {
        if (_enemyHandCards.Contains(card))
        {
            _enemyHandCards.Remove(card);
            UpdateHandPositions();
        }
    }

    /**
     * 外部调用获取敌人卡牌列表
     */

    public List<EnemyCard> GetEnemyCards()
    {
        return _enemyHandCards;
    }

    #endregion
}