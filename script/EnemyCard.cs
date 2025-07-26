using System;
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

    #endregion
}