using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardGame.script;
using CardGame.script.cardAbility;
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
    private List<EnemyCard> _enemyMonsterCards; //敌人卡牌列表
    private PlayerHand _playerHand; //玩家手牌
    private Deck _deck; //玩家牌堆
    public List<Card> _playerBattleCards = new List<Card>();
    public List<EnemyCard> _enemyBattleMonsterCards = new List<EnemyCard>();
    private InputManager _inputManager;
    private RichTextLabel _playerHpLabel;
    private RichTextLabel _enemyHpLabel;

    private Card _hoveringCard; //用于储存悬停中的卡牌
    private Card _currentAttackerCard;
    private Card _currentDefenderCard;
    private Vector2 _originPosition;

    private int playerHp;
    private int enemyHp;
    private bool _isAnimate;
    private bool _endButtonMouseEntered;
    private List<MagicCard> _playerMagicBattleCards = new List<MagicCard>();
    private List<EnemyCard> _enemyMagicCards;

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

        _inputManager.LeftMouseClicked += OnMouseClicked;

        _endButton.MouseEntered += OnEndButtonMouseEntered;
        _endButton.MouseExited += OnEndButtonMouseExited;
    }

    private void OnEndButtonMouseExited()
    {
        _endButtonMouseEntered = false;
    }

    private void OnEndButtonMouseEntered()
    {
        _endButtonMouseEntered = true;
        if (_currentAttackerCard != null)
        {
            var currentCardPosition = _currentAttackerCard.GlobalPosition;
            Vector2 newPos = new Vector2(currentCardPosition.X, currentCardPosition.Y + 30);
            _currentAttackerCard.AnimateCardToPosition(newPos);
            _currentAttackerCard = null;
        }
    }

    /**
     * 当卡牌生成时，自动连接卡片信号
     */
    public void ConnectCardSignal(Card card)
    {
        if (card == null)
        {
            Utils.PrintErr(this, "获取不到Card信息，连接卡牌信号失败");
            return;
        }

        card.Hover += OnHoverOnCard;
        card.HoverOff += OnHoverOffCard;
    }

    private void OnHoverOffCard(Card card)
    {
        //Utils.Print(this,"离开卡牌，不在悬停");
        _hoveringCard = null;
    }

    private void OnHoverOnCard(Card card)
    {
        // Utils.Print(this,"卡牌悬停");
        _hoveringCard = card;
        // Utils.Print(this,card.ToString());
    }


    /**
     * 当鼠标左键点击时
     */
    private void OnMouseClicked()
    {
        BattleCardClicked();
    }

    /**
     * 玩家战斗中卡牌点击时
     */
    private async void BattleCardClicked()
    {
        if (_isAnimate)
        {
            // 动画时，不触发逻辑
            // 如果点击的卡牌已经攻击过了，返回
            return;
        }

        // 敌人战场上没怪时，并且点击的是空白处，且已经选择了要攻击的卡牌
        // 进行直接攻击敌方生命值
        if (_enemyBattleMonsterCards.Count == 0 && _hoveringCard == null && _currentAttackerCard != null &&
            !_endButtonMouseEntered)
        {
            if (_currentAttackerCard.CardInfo.CardType.Equals("Magic"))
            {
                // 防止魔法卡触发直接攻击，导致位移的bug
                return;
            }

            _isAnimate = true;
            await DirectAttack(_currentAttackerCard);
            _currentAttackerCard = null;
            _isAnimate = false;
            return;
        }

        if (_hoveringCard != null && !_hoveringCard.AttackedInCurrentTurn)
        {
            if (_hoveringCard == _currentAttackerCard)
            {
                _isAnimate = true;
                // 如果玩家再次点击了卡牌,收回卡牌
                var currentCardPosition = _currentAttackerCard.GlobalPosition;
                Vector2 newPos = new Vector2(currentCardPosition.X, currentCardPosition.Y + 30);
                await _currentAttackerCard.AnimateCardToPosition(newPos, 0.3);
                _currentAttackerCard = null;
                _isAnimate = false;
                return;
            }

            if (_playerBattleCards.Contains(_hoveringCard))
            {
                // 此时，玩家选择某张牌
                _isAnimate = true;
                if (_currentAttackerCard != null && _currentAttackerCard != _hoveringCard &&
                    !_currentAttackerCard.AttackedInCurrentTurn)
                {
                    // 重置选择的卡牌
                    var oldCardPosition = _currentAttackerCard.GlobalPosition;
                    Vector2 oldCardPos = new Vector2(oldCardPosition.X, oldCardPosition.Y + 30);
                    await _currentAttackerCard.AnimateCardToPosition(oldCardPos, 0.3);
                }

                // 如果玩家战场中存在当前选中点击的卡牌，卡牌事件
                // 偏移一部分
                if (_hoveringCard.AttackedInCurrentTurn)
                {
                    // 已经攻击过了
                    _isAnimate = false;
                    return;
                }

                _currentAttackerCard = _hoveringCard;
                var currentCardPosition = _currentAttackerCard.GlobalPosition;
                Vector2 newPos = new Vector2(currentCardPosition.X, currentCardPosition.Y - 30);
                await _currentAttackerCard.AnimateCardToPosition(newPos, 0.3);
                _isAnimate = false;
            }

            if (_enemyBattleMonsterCards.Contains(_hoveringCard) && _currentAttackerCard != null)
            {
                _isAnimate = true;
                _currentDefenderCard = _hoveringCard;
                await Attack(_currentAttackerCard, _currentDefenderCard);
                _isAnimate = false;
            }
        }

        ChooseEnemyAndAttack();
    }

    /**
     * 当选择了攻击卡牌后，再点击敌方卡牌，可以进行卡牌间的攻击
     */
    private async void ChooseEnemyAndAttack()
    {
        if (_currentAttackerCard == null || _currentAttackerCard.AttackedInCurrentTurn)
        {
            // Utils.PrintErr(this, "没有攻击者或者攻击者已经攻击过了");
            return;
        }

        if (_hoveringCard == null || !(_enemyBattleMonsterCards.Contains(_hoveringCard)))
        {
            // 没有攻击目标，返回
            // 不是敌人的卡牌，无法攻击
            // Utils.PrintErr(this, "没有攻击目标或者目标不在敌方战场上");
            return;
        }

        _isAnimate = true;
        _currentDefenderCard = _hoveringCard;
        await Attack(_currentAttackerCard, _currentDefenderCard);
        _currentAttackerCard.AttackedInCurrentTurn = true;
        _currentAttackerCard = null;
        _isAnimate = false;
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
        _isAnimate = false;
        CollisionShape2D deckCollisionShape2D =
            _deck.GetNode<Area2D>("Area2D").GetNode<CollisionShape2D>("CollisionShape2D");
        deckCollisionShape2D.Disabled = false;
        _deck._thisTurnDrawCard = false;

        // 遍历玩家战场，重新启用设置为未攻击过
        for (var i = 0; i < _playerBattleCards.Count; i++)
        {
            _playerBattleCards[i].AttackedInCurrentTurn = false;
        }

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
        // 给敌人发牌
        if (_opponentDeck != null)
        {
            await _opponentDeck.DrawEnemyCard();
        }
        else
        {
            Utils.PrintErr(this, "空指针，对方卡组未实例化");
        }

        WaitTimerBySecond(3);
        await ToSignal(_battleTimer, "timeout");
        EnemyTurn();
    }

    /**
     * 按秒计时
     */
    public void WaitTimerBySecond(int second)
    {
        _battleTimer.OneShot = true; // 标记为一次性
        _battleTimer.WaitTime = second;
        //Utils.Print(this, "启用TEST模式，计时器为1秒！");
        //_battleTimer.WaitTime = 1;
        _battleTimer.Start();
    }


    /**
     * 在结束按钮按下3秒后触发，对手回合操作
     */
    private async void EnemyTurn()
    {
        _currentAttackerCard = null;
        _currentDefenderCard = null;
        Utils.Print(this, "3秒后 敌人行动");
        _enemyMonsterCards = _enemyHand.GetEnemyMonsterCards();
        _enemyMagicCards = _enemyHand.GetEnemyMagicCards();
        if (_enemyMagicCards != null && _playerBattleCards.Count >= 3)
        {
            Utils.Print(this, "玩家战场怪物大于等于3，敌人使用魔法卡");
            await EnemyUseMagicCard();
        }

        if (_enemyMonsterCards.Count == 0 && _enemyBattleMonsterCards.Count == 0)
        {
            // 手牌为0，且战场卡牌为0
            EndEnemyTurn();
            return;
        }
        else if (_enemyMonsterCards == null)
        {
            Utils.PrintErr(this, "EnemyTurn:未能获取到_enemyCards");
        }

        // 对手回合的后续逻辑
        // 怪物卡逻辑
        // 将卡牌移动到目标卡槽上
        // 并存储卡牌到List中
        //Utils.Print(this, "enemyUseMonsterCardTask running");
        await EnemyUseMonsterCard();

        //Utils.Print(this, "enemyUseMonsterCardTask finished");

        // 人机尝试操作
        await TryPlayCardWithRandomAttack();

        EndEnemyTurn();
    }

    // TODO
    // 使用魔法卡概率性卡死，（很可能是使用了相同的魔法卡卡槽，导致的卡死）
    // 原因猜测：使用后再次扔到了已经使用过的卡槽，使用完没有恢复卡槽（可能性很大）
    private async Task EnemyUseMagicCard()
    {
        int randomCardSlot = GD.RandRange(0, _enemyMagicCardSlots.Count - 1);
        int randomCard = GD.RandRange(0, _enemyMagicCards.Count - 1);
        if (_enemyMagicCards.Count > 0)
        {
            EnemyCard usingMagicCard = _enemyMagicCards[randomCard];

            CardSlot usingCardSlot = _enemyMagicCardSlots[randomCardSlot];
            await _enemyHand.AnimateCardToPosition(usingMagicCard, usingCardSlot.GlobalPosition, true);
            usingMagicCard.SetCardSlot(usingCardSlot);

            if ("龙卷风".Equals(usingMagicCard.CardInfo.Name))
            {
                Utils.Print("龙卷风魔法卡使用！");

                Utils.Print("-------------------玩家战场卡牌信息如下-------------");
                foreach (Card playerBattleCard in _playerBattleCards)
                {
                    Utils.Print($"{playerBattleCard}");
                }

                Utils.Print("-------------------玩家战场卡牌信息如下-------------");
                await usingMagicCard.StormAbility(_playerBattleCards);
            }

            _enemyMagicCardSlots.Remove(usingCardSlot);
            
            _enemyHand.RemoveCardFromHand(usingMagicCard);
            _enemyHand.UpdateHandPositions();
            _enemyMagicCards.Remove(usingMagicCard);
            await DestroyCard(usingMagicCard);

            WaitTimerBySecond(5);
        }
    }

    /**
     * 敌人回合使用卡牌到卡槽上
     */
    private async Task EnemyUseMonsterCard()
    {
        int randomCardSlot = GD.RandRange(0, _enemyMonsterCardSlots.Count - 1);
        int randomCard = GD.RandRange(0, _enemyMonsterCards.Count - 1);
        /*
        Utils.Print(this, "------------------------EnemyUseMonsterCard running -----------------");
        Utils.PrintErr(this,$"_enemyMonsterCards.Count = {_enemyMonsterCards.Count}");
        Utils.PrintErr(this,$"_enemyBattleMonsterCards.Count = {_enemyBattleMonsterCards.Count}");
        */

        if (_enemyMonsterCards.Count != 0)
        {
            EnemyCard usingCard = _enemyMonsterCards[randomCard];
            if (_enemyMonsterCardSlots.Count != 0)
            {
                CardSlot usingCardSlot = _enemyMonsterCardSlots[randomCardSlot];
                await _enemyHand.AnimateCardToPosition(usingCard, usingCardSlot.GlobalPosition, true);
                usingCard.SetCardSlot(usingCardSlot);
                _enemyBattleMonsterCards.Add(usingCard);
                _enemyHand.RemoveCardFromHand(usingCard);
                _enemyMonsterCards.Remove(usingCard);
                _enemyMonsterCardSlots.Remove(usingCardSlot);
                _enemyHand.UpdateHandPositions();
                // 添加一个等待完成Animatecard的信号
            }
        }

        if (_enemyBattleMonsterCards.Count <= 2 && _enemyMonsterCards.Count > 0)
        {
            await EnemyUseMonsterCard();
        }
    }

    /**
     * 人机尝试使用随机攻击力的卡牌
     */
    private async Task TryPlayCardWithRandomAttack()
    {
        List<EnemyCard> enemyBattleCardsCopy = _enemyBattleMonsterCards.ToList();
        foreach (var enemyCard in enemyBattleCardsCopy.Distinct()) // 用于删除重复值，避免重复调用
        {
            if (_playerBattleCards.Count == 0)
            {
                //此时玩家场上没有怪物，直接攻击玩家，扣除玩家血量
                await DirectAttack(enemyCard);
                continue;
            }

            int randomPlayerCard = GD.RandRange(0, _playerBattleCards.Count - 1);
            await Attack(enemyCard, _playerBattleCards[randomPlayerCard]);
            WaitTimerBySecond(3);
            await ToSignal(_battleTimer, "timeout");
            Utils.Print("操作中");
        }
    }

    /**
     * attackerCard ：攻击的卡牌
     * attackingCard ： 被攻击的卡牌
     */
    private async Task Attack(Card attackerCard, Card attackingCard)
    {
        if (attackerCard.CardInfo.CardType.Equals("Magic") || attackingCard.CardInfo.CardType.Equals("Magic"))
        {
            return;
        }

        Utils.Print(this, "Attack");
        var tween = GetTree().CreateTween();
        var OldPosition = attackerCard.GlobalPosition;
        attackerCard.ZIndex = 5;
        attackingCard.ZIndex = 4;

        if (!(attackerCard is EnemyCard))
        {
            OldPosition = new Vector2(OldPosition.X, OldPosition.Y + 30);
        }

        // 敌人卡牌攻击，位置默认*/
        tween.TweenProperty(attackerCard, "position", attackingCard.GlobalPosition, 0.8);
        tween.TweenProperty(attackerCard, "position", OldPosition, 2);

        await ToSignal(tween, "finished");
        attackingCard.ZIndex = 2;
        attackerCard.ZIndex = 2;

        // 更新卡牌受到攻击后的新的生命值
        attackerCard.CardInfo.Hp = Math.Max(0, attackerCard.CardInfo.Hp - attackingCard.CardInfo.Attack);
        attackingCard.CardInfo.Hp = Math.Max(0, attackingCard.CardInfo.Hp - attackerCard.CardInfo.Attack);

        attackerCard.UpdateCardInfoToLabel();
        attackingCard.UpdateCardInfoToLabel();

        // 死亡结算
        if (attackingCard.CardInfo.Hp <= 0)
        {
            DestroyCard(attackingCard);
        }

        if (attackerCard.CardInfo.Hp <= 0)
        {
            DestroyCard(attackerCard);
        }

        attackerCard.AttackedInCurrentTurn = true;
    }

    public async Task DestroyCard(Card destroyCard)
    {
        // 防御性检查：卡牌为空或已销毁
        if (destroyCard == null || !IsInstanceValid(destroyCard) ||　!destroyCard.IsInsideTree())
        {
            return;
        }
        
        // 被攻击方代码逻辑
        if (destroyCard is EnemyCard)
        {
            // 如果是敌人卡牌
            Vector2 deadCardPosition =
                new Vector2(_opponentDeck.GlobalPosition.X, _opponentDeck.GlobalPosition.Y + 300);
            await destroyCard.AnimateCardToPosition(deadCardPosition);
            _enemyBattleMonsterCards.Remove(destroyCard as EnemyCard);
            if (destroyCard.CardInfo.CardType.Equals("Magic"))
            {
                _enemyMagicCardSlots.Add(destroyCard.GetCardSlot());
            }
            else
            {
                _enemyMonsterCardSlots.Add(destroyCard.GetCardSlot());
            }
            
            CardSlot cardSlot = destroyCard.GetCardSlot();
            if (cardSlot == null)
            {
                Utils.PrintErr(this,"Destroy Card enemy cardslot为空");
                return;
            }
            cardSlot.CardInSlot = false;
        }
        else
        {
            // 玩家卡牌     墓地deck Y - 300
            Vector2 deadCardPosition = new Vector2(_deck.GlobalPosition.X, _deck.GlobalPosition.Y - 300);
            await destroyCard.AnimateCardToPosition(deadCardPosition);
            _playerBattleCards.Remove(destroyCard);
            CardSlot cardSlot = destroyCard.GetCardSlot();
            if (cardSlot == null)
            {
                Utils.PrintErr(this,"Destroy Card cardslot为空");
                return;
            }
            cardSlot.CardInSlot = false;
        }
    }

    /**
     * 直接攻击对方
     * 玩家攻击的时候，会将位置偏移到下方30位置（因为点击了卡牌）
     */
    private async Task DirectAttack(Card attackerCard)
    {
        if (attackerCard.CardInfo.CardType.Equals("Magic"))
        {
            return;
        }

        if (attackerCard.CardInfo.Hp <= 0)
        {
            return;
        }

        // 每个分支需要播放攻击动画
        if (attackerCard is EnemyCard)
        {
            var tween1 = GetTree().CreateTween();
            var oldPosition = attackerCard.GlobalPosition;
            tween1.TweenProperty(attackerCard, "position", _playerHpLabel.GlobalPosition, 0.8);
            await ToSignal(tween1, "finished");
            var tween2 = GetTree().CreateTween();
            tween2.TweenProperty(attackerCard, "position", oldPosition, 0.8);
            await ToSignal(tween2, "finished");
            playerHp = playerHp - attackerCard.CardInfo.Attack;
        }
        else
        {
            var tween1 = GetTree().CreateTween();
            var oldPosition = new Vector2(attackerCard.GlobalPosition.X, attackerCard.GlobalPosition.Y + 30);
            tween1.TweenProperty(attackerCard, "position", _enemyHpLabel.GlobalPosition, 0.8);
            await ToSignal(tween1, "finished");
            var tween2 = GetTree().CreateTween();
            tween2.TweenProperty(attackerCard, "position", oldPosition, 0.8);
            await ToSignal(tween2, "finished");
            enemyHp = enemyHp - attackerCard.CardInfo.Attack;
        }

        attackerCard.AttackedInCurrentTurn = true;
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
        // GD.Print("EndEnemyTurn一次");
        _deck.DrawCard();
    }

    private void OnBattleFinished()
    {
        Utils.Print(this, "Battle Finished emit");
    }

    /**
     * 将魔法卡牌加入到战场中
     */
    public void AddTOPlayerBattleMagicCards(Card cardManagerCardBeingDragged)
    {
        if (!"Magic".Equals(cardManagerCardBeingDragged.CardInfo.CardType))
        {
            Utils.PrintErr("警告：不是魔法卡牌，加入魔法卡牌列表失败！");
            return;
        }

        _playerMagicBattleCards.Add(cardManagerCardBeingDragged as MagicCard);
    }

    /**
     * InputManager中，FinishedDragged后，使用魔法卡，调用此方法
     * 后续不同的魔法卡能力，都从这个方法中调用
     */
    public async void UsingMagicCard<T>(T magicCard) where T : MagicCard
    {
        // TODO
        // 各种魔法卡能力实现
        if (magicCard == null)
        {
            Utils.PrintErr("错误，magicCard为null");
            return;
        }

        if ("龙卷风".Equals(magicCard.CardInfo.Name))
        {
            Utils.Print("龙卷风魔法卡使用！");
            Tornado tornado = magicCard as Tornado;
            await tornado.StormAbility(_enemyBattleMonsterCards);
        }

        // 魔法卡使用成功，移除卡槽占用
        CardSlot cardSlot = magicCard.GetCardSlot();
        await DestroyCard(magicCard);
        cardSlot.CardInSlot = false;
    }

    public Timer GetBattleTimer()
    {
        return _battleTimer;
    }

    #endregion
}