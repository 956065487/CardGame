using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CardGame.script.cardAbility;
using Godot;
using CardGame.script.constant;
using CardGame.script.pojo;

namespace CardGame.script;


public partial class EnemyCard : Card
{
    #region 信号、属性
    
    // public new Vector2 PositionInHand {set; get;}
    // public CardInfo CardInfo {set; get;}
    private RichTextLabel _attackLabel;
    private RichTextLabel _healthLabel;
    private OpponentDeck _opponentDeck;
    private Area2D _area2D;
    private BattleManager _battleManager;

    #endregion

    #region 生命周期中的方法

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GetNodes();
        CheckEnemyCard = true;
        // 设置卡牌出现的初始位置
        SetPosition(_opponentDeck.GlobalPosition);
        
        // 更新卡牌标签
        _attackLabel = GetNodeOrNull<RichTextLabel>("Attack");
        _healthLabel = GetNodeOrNull<RichTextLabel>("Health");
        
        _area2D.MouseEntered += OnMouseEnter;
        _area2D.MouseExited += OnMouseExit;
        
        BattleManager battleManager = GetNodeOrNull<BattleManager>("/root/Main/BattleManager");
        if (battleManager == null)
        {
            Utils.PrintErr(this, "BattleManager获取不到,自动连接卡牌信号失败");
        }
        else
        {
            battleManager.ConnectCardSignal(this);
        }
    }

    private void GetNodes()
    {
        Node main = GetParent().GetParent();
        _area2D = GetNodeOrNull<Area2D>("Area2D");

        if (main == null)
        {
            Utils.PrintErr(this,"获取Main节点失败！");
        }
        _opponentDeck = main.GetNodeOrNull<OpponentDeck>("OpponentDeck");

        if (_opponentDeck == null)
        {
            Utils.PrintErr(this,"OpponentDeck获取失败");
        }
        
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public override void _ExitTree()
    {
    }

    #endregion

    #region 自定义方法

    /**
     * 将卡牌信息展示到实例上，并初始化CardInfo
     * 例如，生命值，攻击力，名字以及描述
     * 目前只更新生命值和攻击力
     */
    public void UpdateCardInfo()
    {
        _attackLabel.Text = $"{CardInfo.Attack}";
        _healthLabel.Text = $"{CardInfo.Hp}";
    }

    public override string ToString()
    {
        return CardInfo.ToString();
    }
    
    private async Task BaseStormAbility<T>(List<T> cards) where T : Card
    {
        if (!"龙卷风".Equals(CardInfo.Name))
        {
            return;
        }
        _battleManager = GetNodeOrNull<BattleManager>("/root/Main/BattleManager");
        for (var i = 0; i < cards.Count; i++)
        {
            cards[i].CardInfo.Hp = Mathf.Max(cards[i].CardInfo.Hp - Tornado.MAGIC_DAMAGE, 0);
            cards[i].UpdateCardInfoToLabel();
            if (cards[i].CardInfo.Hp <= 0)
            {
                _battleManager.DestroyCard(cards[i]);
                i = i - 1;
            }
        }

        bool checkEnemyCard = CheckEnemyCard;
        if (checkEnemyCard)
        {
            SetGlobalPosition(new Vector2(514,617));
            await AnimateCardToPosition(new Vector2(1255, 617),1.2);
        }
        else
        {
            SetGlobalPosition(new Vector2(514,387));
            await AnimateCardToPosition(new Vector2(1255, 387),1.2);
        }
    }
    
    public async Task StormAbility(List<EnemyCard> monsterCards)
    {
        await BaseStormAbility(monsterCards);
    }
    
    public async Task StormAbility(List<Card> monsterCards)
    {
        await BaseStormAbility(monsterCards);
    }

    #endregion
}