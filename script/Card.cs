using System;
using System.Threading.Tasks;
using Godot;
using CardGame.script.constant;
using CardGame.script.pojo;

namespace CardGame.script;

[GlobalClass, Icon("res://asset/knight/Knight.png")]
public partial class Card : Node2D
{
    #region 信号、属性

    [Signal]
    public delegate void HoverEventHandler(Card card);

    [Signal]
    public delegate void HoverOffEventHandler(Card card);
    
    [Signal]
    public delegate void MouseClickedEventHandler();    // 鼠标点击

    public Vector2 PositionInHand { set; get; }
    public CardInfo CardInfo { set; get; }
    public RichTextLabel AttackLabel;
    public RichTextLabel HealthLabel;
    protected bool CheckEnemyCard;
    public bool PositionInCardSlot {set; get;}
    
    private CardSlot _usingCardSlot;
    
    public bool MouseChooseInBattle { set; get; }

    public bool AttackedInCurrentTurn { set; get; }

    #endregion

    #region 生命周期中的方法

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        CheckEnemyCard = false;
        // 设置卡牌出现的初始位置
        Node main = GetParent().GetParent();
        if (main == null)
        {
            Utils.PrintErr(this, "获取Main节点失败！");
        }

        Deck deck = main.GetNodeOrNull<Deck>("Deck");
        if (deck == null)
        {
            Utils.PrintErr(this, "获取Deck节点失败！");
        }

        SetPosition(deck.GlobalPosition);


        // 连接Area2D节点的OnMouseEnter信号
        Area2D cardArea2D = GetNode<Area2D>("Area2D");
        if (cardArea2D != null)
        {
            cardArea2D.CollisionLayer = Constant.LAYER_CARD;
            cardArea2D.CollisionMask = Constant.LAYER_SLOT; //卡牌需要检测卡槽，所以mask包含卡槽层

            // 连接信号
            cardArea2D.MouseEntered += OnMouseEnter;
            cardArea2D.MouseExited += OnMouseExit;
        }
        else
        {
            Utils.PrintErr(this, "当前卡牌设置layer和mask失败，检测不到Area2D");
        }


        // 用于每当卡片在父节点CardManager实例化时，直接连接信号，无需一个个手动连接
        try
        {
            CardManager cardManager = (CardManager)GetParent();
            cardManager.ConnectCardSignals(this);
        }
        catch (Exception e)
        {
            GD.PrintErr("Card实例名 = " + GetName());
            GD.PrintErr("Card错误信息:" + e.Message);
            // throw;
        }
        
        // 连接BattleManager 如果没有
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

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public override void _ExitTree()
    {
    }

    #endregion

    #region 自定义的方法

    /**
     * 鼠标进入后，发射悬停信号
     */
    public void OnMouseEnter()
    {
        //GD.Print("成功连接MouseEnter");
        // MyHoverSignal();
        EmitSignalHover(this);
    }

    public void OnMouseExit()
    {
        //GD.Print("成功连接MouseExit");
        // HoverOffSignal();
        EmitSignalHoverOff(this);
    }

    /**
     * 将卡牌信息展示到实例上
     * 例如，生命值，攻击力，名字以及描述
     * 目前只更新生命值和攻击力
     */
    public void UpdateCardInfoToLabel()
    {
        // 更新卡牌标签
        AttackLabel = GetNodeOrNull<RichTextLabel>("Attack");
        HealthLabel = GetNodeOrNull<RichTextLabel>("Health");
        AttackLabel.Text = $"{CardInfo.Attack}";
        HealthLabel.Text = $"{CardInfo.Hp}";
    }

    public bool GetCheckEnemyCard()
    {
        return CheckEnemyCard;
    }

    public CardInfo GetCardInfo()
    {
        return CardInfo;
    }
    
    /**
     * 平移动画更新卡牌位置
     */
    public void AnimateCardToPosition(Vector2 newPosition)
    { 
        var tween = GetTree().CreateTween();
        tween.TweenProperty(this, "position", newPosition,1);

    }
    
    /**
     * 平移动画更新卡牌位置
     * duration：速度
     */
    public async Task AnimateCardToPosition(Vector2 newPosition,double duration)
    { 
        var tween = GetTree().CreateTween();
        tween.TweenProperty(this, "position", newPosition, duration);
        await ToSignal(tween, "finished");

    }

    /**
     * 设置占用的卡槽
     */
    public void SetCardSlot(CardSlot cardSlot)
    {
        this._usingCardSlot = cardSlot;
    }

    /**
     * 获得占用的卡槽
     */
    public CardSlot GetCardSlot()
    {
        return _usingCardSlot;
    }

    public override string ToString()
    {
        return CardInfo.ToString();
    }

    #endregion
}