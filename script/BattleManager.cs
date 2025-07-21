using Godot;
using System;
using System.Collections.Generic;
using CardGame.script;
using CardGame.script.constant;
using CardGame.script.pojo;
using Godot.Collections;
using Microsoft.VisualBasic.CompilerServices;
using Array = System.Array;
using Utils = CardGame.script.Utils;

public partial class BattleManager : Node2D
{
    #region 属性，变量

    private TextureButton _endButton; //回合结束按钮
    private OpponentDeck _opponentDeck; //敌方牌堆
    private Timer _battleTimer; //计时器
    private List<CardSlot> _enemyMonsterCardSlots = []; //敌人怪物卡槽列表
    private List<CardSlot> _enemyMagicCardSlots = []; //敌人魔法卡槽列表
    private Node2D _opponentCardSlots; //对手卡槽父节点
    private EnemyHand _enemyHand; //敌方手牌
    private List<EnemyCard> _enemyCards; //敌人卡牌列表
    private PlayerHand _playerHand; //玩家手牌
    private Deck _deck; //玩家牌堆
    private List<Card> _playerBattleCards = new List<Card>();
    private List<EnemyCard> _enemyBattleCards = new List<EnemyCard>();
    private InputManager _inputManager;
    private RichTextLabel _playerHpLabel;
    private RichTextLabel _enemyHpLabel;

    private int playerHp;
    private int enemyHp;

    [Signal]
    private delegate void BattleFinishedEventHandler();

    #endregion

    #region 生命周期方法

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GetNodes(); // 获取所有需要的对象
        ConnectSignals(); // 连接信号
        InitEnemyCardSlots(); //初始化敌人卡槽

        InitPlayerAndEnemy(); // 初始化玩家和敌人
    }

    #endregion


    #region 自定义方法、信号

    /**
     * 初始化玩家和敌人
     */
    private void InitPlayerAndEnemy()
    {
        playerHp = Constant.STARTING_HP;
        enemyHp = Constant.STARTING_HP;
        UpdateHp();
    }

    private void UpdateHp()
    {
        _playerHpLabel.Text = "玩家生命值：" + playerHp.ToString();
        _enemyHpLabel.Text = "敌人生命值：" + enemyHp.ToString();
    }

    /**
     *  添加卡牌到玩家战场卡牌列表中
     */
    public void AddToPlayerBattleCards(Card card)
    {
        this._playerBattleCards.Add(card);
    }

    /**
     * 初始化敌人卡槽
     * _enemyMonsterCardSlots
     * _enemyMagicCardSlots
     */
    private void InitEnemyCardSlots()
    {
        // 获取所有敌人卡槽，并分类成怪物卡槽和魔法卡槽
        Array<Node> enemyCardSlots = _opponentCardSlots.GetChildren();
        foreach (Node enemyCardSlot in enemyCardSlots)
        {
            if (!(enemyCardSlot is CardSlot))
            {
                Utils.PrintErr(this, $"警告，不是CardSlot类型,{enemyCardSlot.Name}");
            }

            if (enemyCardSlot.Name.ToString().Contains("MonsterCardSlot"))
            {
                _enemyMonsterCardSlots.Add(enemyCardSlot as CardSlot);
            }
            else if (enemyCardSlot.Name.ToString().Contains("MagicCardSlot"))
            {
                _enemyMagicCardSlots.Add(enemyCardSlot as CardSlot);
            }
        }
    }

    /**
     * 用于统一获取实例对象
     */
    private void GetNodes()
    {
        _endButton = GetNodeOrNull<TextureButton>("/root/Main/EndButton");
        _opponentDeck = GetNodeOrNull<OpponentDeck>("/root/Main/OpponentDeck");
        _battleTimer = GetNodeOrNull<Timer>("/root/Main/BattleTimer");
        _opponentCardSlots = GetNodeOrNull<Node2D>("/root/Main/OpponentCardSlots");
        _enemyHand = GetNode<EnemyHand>("/root/Main/EnemyHand");
        _playerHand = GetNode<PlayerHand>("/root/Main/PlayerHand");
        _deck = GetNode<Deck>("/root/Main/Deck");
        _inputManager = GetNode<InputManager>("/root/Main/InputManager");
        _playerHpLabel = GetNode<RichTextLabel>("/root/Main/EnemyHp");
        _enemyHpLabel = GetNode<RichTextLabel>("/root/Main/PlayerHp");
        
        


        if (_endButton == null)
        {
            Utils.PrintErr(this, "未能获取到回合结束按钮实例！");
        }
        else if (_opponentDeck == null)
        {
            Utils.PrintErr(this, "未能获取到对手卡组实例！");
        }
        else if (_battleTimer == null)
        {
            Utils.PrintErr(this, "未能获取到战斗计时器实例");
        }
        else if (_opponentCardSlots == null)
        {
            Utils.PrintErr(this, "获取不到 _opponentCardSlots Node2D节点");
        }
        else if (_enemyHand == null)
        {
            Utils.PrintErr(this, "获取不到_enemyHand 节点");
        }
        else if (_playerHand == null)
        {
            Utils.PrintErr(this, "获取不到_playerHand节点");
        }
        else if (_deck == null)
        {
            Utils.PrintErr(this, "获取不到_deck节点");
        }
        else if (_inputManager == null)
        {
            Utils.PrintErr(this, "_inputManager获取失败");
        }
        else if (_playerHpLabel == null)
        {
            Utils.PrintErr(this, "_playerHpLabel获取失败");
        }
        else if (_enemyHpLabel == null)
        {
            Utils.PrintErr(this, "_enemyHpLabel获取失败");
        }
    }

    /**
     * 用于统一管理信号链接
     */
    private void ConnectSignals()
    {
        // 1、回合结束按钮按下信号连接
        _endButton.Pressed += OnEndButtonPressed;
    }


    /**
     * 回合结束按钮按下
     */
    private void OnEndButtonPressed()
    {
        //禁用玩家操作
        DisabledPlayer();
        _endButton.Disabled = true;
        _endButton.Visible = false;
        WaitForEndTurnTimerTimeOut();
    }

    /**
     * 禁用玩家
     */
    private void DisabledPlayer()
    {
        CollisionShape2D deckCollisionShape2D =
            _deck.GetNode<Area2D>("Area2D").GetNode<CollisionShape2D>("CollisionShape2D");
        deckCollisionShape2D.Disabled = true;

        List<Card> playerHandCards = _playerHand.GetPlayerHandCards();
        foreach (Card playerHandCard in playerHandCards)
        {
            CollisionShape2D cardCollisionShape2D =
                playerHandCard.GetNode<Area2D>("Area2D").GetNode<CollisionShape2D>("CollisionShape2D");
            cardCollisionShape2D.Disabled = true;
        }
    }

    /**
     *启动玩家
     */
    private void EnablePlayer()
    {
        CollisionShape2D deckCollisionShape2D =
            _deck.GetNode<Area2D>("Area2D").GetNode<CollisionShape2D>("CollisionShape2D");
        deckCollisionShape2D.Disabled = false;
        _deck._thisTurnDrawCard = false;

        List<Card> playerHandCards = _playerHand.GetPlayerHandCards();
        foreach (Card playerHandCard in playerHandCards)
        {
            CollisionShape2D cardCollisionShape2D =
                playerHandCard.GetNode<Area2D>("Area2D").GetNode<CollisionShape2D>("CollisionShape2D");
            cardCollisionShape2D.Disabled = false;
        }
    }

    /**
     * 等待信号结束，计时器3秒后结束，模拟AI思考。调用EnemyTurn，敌方回合
     */
    private async void WaitForEndTurnTimerTimeOut()
    {
        WaitTimerBySecond(3);
        Utils.Print(this, "等待3秒开始");
        // 给敌人发牌
        if (_opponentDeck != null)
        {
            _opponentDeck.DrawEnemyCard();
        }
        else
        {
            Utils.PrintErr(this, "空指针，对方卡组未实例化");
        }

        await ToSignal(_battleTimer, "timeout");
        EnemyTurn();
    }

    /**
     * 按秒计时
     */
    private void WaitTimerBySecond(int second)
    {
        _battleTimer.OneShot = true; // 标记为一次性
        _battleTimer.WaitTime = second;
        _battleTimer.Start();
    }


    /**
     * 在结束按钮按下3秒后触发，对手回合操作
     */
    private async void EnemyTurn()
    {
        Utils.Print(this, "3秒后 敌人行动");
        _enemyCards = _enemyHand.GetEnemyCards();
        if (_enemyCards.Count == 0 && _enemyBattleCards.Count == 0)
        {
            // 手牌为0，且战场卡牌为0
            EndEnemyTurn();
            return;
        } 
        else if (_enemyCards == null)
        {
            Utils.PrintErr(this, "EnemyTurn:未能获取到_enemyCards");
        }

        

        // 对手回合的后续逻辑
        // 将卡牌移动到目标卡槽上
        // 并存储卡牌到List中
        int randomCardSlot = GD.RandRange(0, _enemyMonsterCardSlots.Count - 1);
        int randomCard = GD.RandRange(0, _enemyCards.Count - 1);
        EnemyCard usingCard = _enemyCards[randomCard];
        CardSlot usingCardSlot = _enemyMonsterCardSlots[randomCardSlot];
        _enemyHand.AnimateCardToPosition(usingCard, usingCardSlot.GlobalPosition, true);
        _enemyBattleCards.Add(usingCard);
        _enemyHand.RemoveCardFromHand(usingCard);
        _enemyMonsterCardSlots.Remove(usingCardSlot);
        _enemyHand.UpdateHandPositions();

        // 5秒后玩家可操作
        WaitTimerBySecond(5);
        await ToSignal(_battleTimer, "timeout");

        // 人机尝试操作
        TryPlayCardWithRandomAttack();

        await ToSignal(this, "BattleFinished");
        EndEnemyTurn();
    }

    /**
     * 人机尝试使用随机攻击力的卡牌
     */
    private async void TryPlayCardWithRandomAttack()
    {
        foreach (var enemyCard in _enemyBattleCards)
        {
            if (_playerBattleCards.Count == 0)
            {
                //此时玩家场上没有怪物，直接攻击玩家，扣除玩家血量
                DirectAttack(enemyCard);
                WaitTimerBySecond(3);
                await ToSignal(_battleTimer, "timeout");
                continue;
            }

            int randomPlayerCard = GD.RandRange(0, _playerBattleCards.Count - 1);
            Attack(enemyCard, _playerBattleCards[randomPlayerCard]);
            WaitTimerBySecond(3);
            await ToSignal(_battleTimer, "timeout");
            Utils.Print("操作中");
        }

        Utils.Print(this, "Battle Finished emit");
        // 提交战斗结束信号
        EmitSignalBattleFinished();
        // EndEnemyTurn();
    }

    /**
     * attackerCard ：攻击的卡牌
     * attackingCard ： 被攻击的卡牌
     */
    private async void Attack(Card attackerCard, Card attackingCard)
    {
        Utils.Print(this, "Attack");
        var tween = GetTree().CreateTween();
        var OldPosition = attackerCard.GlobalPosition;
        attackingCard.ZIndex = attackerCard.ZIndex + 1;
        tween.TweenProperty(attackerCard, "position", attackingCard.GlobalPosition, 0.8);
        tween.TweenProperty(attackerCard, "position", OldPosition, 2);
        await ToSignal(tween, "finished");
        attackingCard.ZIndex = attackerCard.ZIndex - 1;
    }

    private async void DirectAttack(Card attackerCard)
    {
        // 每个分支需要播放攻击动画
        if (attackerCard is EnemyCard)
        {
            var tween1 = GetTree().CreateTween();
            var oldPosition = attackerCard.GlobalPosition;
            tween1.TweenProperty(attackerCard, "position", _playerHpLabel.GlobalPosition, 0.8);
            await ToSignal(tween1, "finished");
            var tween2 = GetTree().CreateTween();
            tween2.TweenProperty(attackerCard, "position", oldPosition, 0.8);
            playerHp = playerHp - attackerCard.CardInfo.Attack;
        }
        else
        {
            var tween1 = GetTree().CreateTween();
            var oldPosition = attackerCard.GlobalPosition;
            tween1.TweenProperty(attackerCard, "position", _enemyHpLabel.GlobalPosition, 0.8);
            await ToSignal(tween1, "finished");
            var tween2 = GetTree().CreateTween();
            tween2.TweenProperty(attackerCard, "position", oldPosition, 0.8);
            enemyHp = enemyHp - attackerCard.CardInfo.Attack;
        }
        UpdateHp();
    }


    /**
     * 敌人回合结束，启动玩家，给玩家发牌
     */
    private void EndEnemyTurn()
    {
        _endButton.Disabled = false;
        _endButton.Visible = true;
        EnablePlayer(); // 启用玩家操作
        GD.Print("EndEnemyTurn一次");
        _deck.DrawCard();
    }

    private void OnBattleFinished()
    {
        Utils.Print(this, "Battle Finished emit");
    }

    #endregion
}