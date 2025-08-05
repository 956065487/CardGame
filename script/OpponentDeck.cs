using Godot;
using System;
using System.Collections.Generic;
using CardGame.script;
using CardGame.script.constant;
using CardGame.script.pojo;

public partial class OpponentDeck : Deck
{
    #region 属性

    private List<String> _enemyDeckList = [];

    private RichTextLabel _enemyNumLabel;
    private EnemyHand _enemyHand;
    private int _startCardsNum;
    private Area2D _opponentArea2D;
    private CollisionShape2D _deckCollisionShape2D;
    private Sprite2D _deckSprite2D;

    #endregion

    #region 生命周期方法

    public override void _Ready()
    {
        GetNodes();
        HideArea2D();
        _startCardsNum = 4;
        
        // 生成卡组
        bool success = Utils.RandomCardInList(_enemyDeckList,8,10);
        if (!success)
        {
            Utils.PrintErr(this,"生成卡组失败，请检查输入值是否正确！");
        }
        
        for (int i = 0; i < _startCardsNum; i++)
        {
            DrawEnemyCard();
        }
        
        
    }


    #endregion
    #region 自定义方法

    /**
     * 统一获取节点实例
     */
    private void GetNodes()
    {
        _opponentArea2D = GetNodeOrNull<Area2D>("Area2D");
        _deckCollisionShape2D = GetNodeOrNull<CollisionShape2D>("Area2D/CollisionShape2D");
        _deckSprite2D = GetNodeOrNull<Sprite2D>("Sprite2D");
        _enemyNumLabel = GetNodeOrNull<RichTextLabel>("NumberLabel");
        _enemyHand = GetNodeOrNull<EnemyHand>("/root/Main/EnemyHand");

        if (_opponentArea2D == null)
        {
            Utils.PrintErr(this, "获取Area2D失败");
        }
        else if (_deckCollisionShape2D == null)
        {
            Utils.PrintErr(this, "获取_deckCollisionShape2D失败");
        }
        else if (_deckSprite2D == null)
        {
            Utils.PrintErr(this, "获取_deckSprite2D失败");
        }
        else if (_enemyNumLabel == null)
        {
            Utils.PrintErr(this, "获取_enemyNumLabel失败");
        }
        else if (_enemyHand == null)
        {
            Utils.PrintErr(this,"获取_enemyHand失败");
        }
    }


    /**
     * 禁用area2d
     */
    private void HideArea2D()
    {
        _opponentArea2D.Monitorable = false; //检测碰撞体
        _opponentArea2D.Monitoring = false; //检测进入和退出
    }




    public void DrawEnemyCard()
    {
        // 当牌堆没牌时
        if (_enemyDeckList.Count == 0)
        {
            // 此时是最后一张被发出去，重置隐藏等
            return;
        }
        else if (_enemyDeckList.Count == 1)
        {
            _deckSprite2D.Visible = false;
            _enemyNumLabel.Visible = false;
        }

        int randomCardIndex = GD.RandRange(0, _enemyDeckList.Count - 1);
        var cardDraw = _enemyDeckList[randomCardIndex];
        _enemyDeckList.Remove(cardDraw);
        _enemyNumLabel.Text = _enemyDeckList.Count.ToString();
        PackedScene cardScene = GD.Load<PackedScene>(Constant.ENEMY_CARD_SCENE_PATH);

        EnemyCard newCard = (EnemyCard)cardScene.Instantiate();
        GetNode("/root/Main/CardManager").AddChild(newCard);
        newCard.Name = "EnemyCard";
        Sprite2D CardImg = newCard.GetNodeOrNull<Sprite2D>("CardImg");
        CardImg.Texture = GD.Load<Texture2D>($"res://asset/CardImg/" + cardDraw + "Card.png");

        CardInfo cardInfo = CardDataLoader.GetCardInfo(cardDraw);
        newCard.CardInfo = cardInfo;
        newCard.UpdateCardInfo();
        // Utils.Print(this,$"创建时，卡牌的信息是{newCard.CardInfo}");

        if ("Magic".Equals(newCard.CardInfo.CardType))
        {
            newCard.GetNode<RichTextLabel>("Attack").Visible = false;
            newCard.GetNode<RichTextLabel>("Health").Visible = false;
        }
        _enemyHand.AddToHand(newCard);
        // 播放翻转动画，敌方卡牌不翻转
        // newCard.GetNode<AnimationPlayer>("AnimationPlayer").Play("CardSlip");
    }
    
    #endregion
}