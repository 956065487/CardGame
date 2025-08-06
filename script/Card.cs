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
    private RichTextLabel _ability;
    protected bool CheckEnemyCard;
    public bool PositionInCardSlot {set; get;}
    
    private CardSlot _usingCardSlot;
    
    public bool MouseChooseInBattle { set; get; }

    public bool AttackedInCurrentTurn { set; get; }

    private Sprite2D _cardImg;

    #endregion

    #region 生命周期中的方法

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GetNodes();
        CheckEnemyCard = false;
        // 设置卡牌出现的初始位置
        Node main = GetNodeOrNull("/root/Main");
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
        
        // 更新卡牌描述
        _ability.Text = CardInfo.Description;
        _ability.FitContent = true; // 自动更新尺寸
        _ability.Visible = false;   // 默认隐藏
        
        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Color(0.6f, 0.6f, 0.6f); // 如果颜色有问题，这一行注释
        styleBox.BorderWidthLeft = 2;
        styleBox.BorderWidthRight = 2;
        styleBox.BorderWidthBottom = 2;
        styleBox.BorderWidthTop = 2;
        styleBox.BorderColor = new Color(255, 255, 255);

        _ability.AddThemeStyleboxOverride("normal",styleBox);
        _ability.PushBgcolor(Colors.Black);
    }

    private void GetNodes()
    {
        // 更新卡牌标签
        AttackLabel = GetNodeOrNull<RichTextLabel>("Attack");
        HealthLabel = GetNodeOrNull<RichTextLabel>("Health");
        _cardImg = GetNodeOrNull<Sprite2D>("CardImg");
        _ability = GetNodeOrNull<RichTextLabel>("AbilityDescription");
        // GD.Print("card GetNodes");
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
     * 初始化魔法卡
     */
    public void InitMagicCard()
    {
        AttackLabel = GetNodeOrNull<RichTextLabel>("Attack");
        HealthLabel = GetNodeOrNull<RichTextLabel>("Health");
        AttackLabel.Visible = false;
        HealthLabel.Visible = false;
        // _cardImg.Visible = false;
        
        // _animationPlayer.Play("CardSlip");
    }

    /**
     * 鼠标进入后，发射悬停信号
     * 显示卡牌能力
     */
    public void OnMouseEnter()
    {
        //GD.Print("成功连接MouseEnter");
        // MyHoverSignal();
        EmitSignalHover(this);
        if (this is EnemyCard)
        {
            return;
        }
        _ability.Visible = true;
        
    }

    /**
     * 退出时，关闭卡牌能力描述
     */
    public void OnMouseExit()
    {
        //GD.Print("成功连接MouseExit");
        // HoverOffSignal();
        EmitSignalHoverOff(this);
        
        if (this is EnemyCard)
        {
            return;
        }
        _ability.Visible = false;
    }

    /**
     * 将卡牌信息展示到实例上
     * 例如，生命值，攻击力，名字以及描述
     * 目前只更新生命值和攻击力
     */
    public void UpdateCardInfoToLabel()
    {
        AttackLabel = GetNodeOrNull<RichTextLabel>("Attack");
        HealthLabel = GetNodeOrNull<RichTextLabel>("Health");
        AttackLabel.Text = $"{CardInfo.Attack}";
        HealthLabel.Text = $"{CardInfo.Hp}";
    }

    public bool GetCheckEnemyCard()
    {
        return CheckEnemyCard;
    }
    
    public void SetCheckEnemyCard(bool isEnemyCard)
    {
        CheckEnemyCard = isEnemyCard;
    }

    public CardInfo GetCardInfo()
    {
        return CardInfo;
    }
    
    /**
     * 平移动画更新卡牌位置
     */
    public async Task AnimateCardToPosition(Vector2 newPosition)
    { 
        var tween = GetTree().CreateTween();
        tween.TweenProperty(this, "position", newPosition,1);
        await ToSignal(tween, "finished");
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