using Godot;
using System;
using System.Collections.Generic;
using CardGame.script;
using Godot.Collections;
using Microsoft.VisualBasic.CompilerServices;
using Array = System.Array;
using Utils = CardGame.script.Utils;

public partial class BattleManager : Node2D
{
    #region 属性，变量

    private TextureButton _endButton; //回合结束按钮
    private OpponentDeck _opponentDeck;
    private Timer _battleTimer;
    private List<CardSlot> _enemyMonsterCardSlots = [];
    private List<CardSlot> _enemyMagicCardSlots = [];
    private Node2D _opponentCardSlots;
    private EnemyHand _enemyHand;
    private PlayerHand _playerHand;
    private Deck _deck;

    #endregion

    #region 生命周期方法

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GetNodes(); // 获取所有需要的对象
        ConnectSignals(); // 连接信号
        InitEnemyCardSlots(); //初始化敌人卡槽
        
        
    }

    #endregion


    #region 自定义方法、信号

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
        } else if (_deck == null)
        {
            Utils.PrintErr(this, "获取不到_deck节点");
        }
    }

    /**
     * 用于统一管理信号链接
     */
    private void ConnectSignals()
    {
        // 1、回合结束按钮按下信号连接
        _endButton.Pressed += OnEndButtonPressed;

        //2、计时器
        // _battleTimer.Timeout += OnBattleTimerTimeOut;
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
        CollisionShape2D deckCollisionShape2D = _deck.GetNode<Area2D>("Area2D").GetNode<CollisionShape2D>("CollisionShape2D");
        deckCollisionShape2D.Disabled = true;

        List<Card> playerHandCards = _playerHand.GetPlayerHandCards();
        foreach (Card playerHandCard in playerHandCards)
        {
            CollisionShape2D cardCollisionShape2D = playerHandCard.GetNode<Area2D>("Area2D").GetNode<CollisionShape2D>("CollisionShape2D");
            cardCollisionShape2D.Disabled = true;
        }
    }

    /**
     *启动玩家
     */
    private void EnablePlayer()
    {
        CollisionShape2D deckCollisionShape2D = _deck.GetNode<Area2D>("Area2D").GetNode<CollisionShape2D>("CollisionShape2D");
        deckCollisionShape2D.Disabled = false;

        List<Card> playerHandCards = _playerHand.GetPlayerHandCards();
        foreach (Card playerHandCard in playerHandCards)
        {
            CollisionShape2D cardCollisionShape2D = playerHandCard.GetNode<Area2D>("Area2D").GetNode<CollisionShape2D>("CollisionShape2D");
            cardCollisionShape2D.Disabled = false;
        }
    }
    
    /**
     * 等待信号结束
     */
    private async void WaitForEndTurnTimerTimeOut()
    {
        _battleTimer.OneShot = true; // 标记为一次性
        _battleTimer.WaitTime = 5.0;
        _battleTimer.Start();
        Utils.Print(this,"等待5秒开始");
        await ToSignal(_battleTimer, "timeout");
        EnemyTurn();
    }
    
    
    /**
     * 对手回合，在结束按钮按下时，触发
     */
    private void EnemyTurn()
    {

        Utils.Print(this, "5秒后 敌人行动");

        if (_enemyMonsterCardSlots.Count == 0)
        {
            EndEnemyTurn();
            return;
        }
        
        if (_opponentDeck != null)
        {
            // 给敌人发牌
            _opponentDeck.DrawEnemyCard();
        }
        else
        {
            Utils.PrintErr(this, "空指针，对方卡组未实例化");
        }

        // 对手回合的后续逻辑
        
    }
    
    



    

    /**
     * 敌人回合结束
     */
    private void EndEnemyTurn()
    {
        _endButton.Disabled = false;
        _endButton.Visible = true;
        EnablePlayer(); // 启用玩家操作
    }

    #endregion
}