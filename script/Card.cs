using System;
using Godot;

namespace CardGame.script;

[GlobalClass,Icon("res://asset/knight/Knight.png")]
public partial class Card : Node2D
{
    #region 信号、属性

    [Signal]
    public delegate void HoverEventHandler(Card card);

    [Signal]
    public delegate void HoverOffEventHandler(Card card);
    
    #endregion

    #region 生命周期中的方法

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // 连接Area2D节点的OnMouseEnter信号
        Area2D area2D = GetNode<Area2D>("Area2D");
        area2D.MouseEntered += OnMouseEnter;
        area2D.MouseExited += OnMouseExit;

        /*if (GetParent().GetType() != typeof(CardManager))
        {
            GD.PrintErr("Card ERROR:获取不到父对象CardManager");
            return;
        }*/
        // 用于每当卡片在父节点CardManager实例化时，直接连接信号，无需一个个手动连接
        try
        {
            CardManager cardManager = (CardManager)GetParent();
            cardManager.ConnectCardSignals(this);
        }
        catch (Exception e)
        {
            GD.PrintErr("Card实例名 = " + GetName());
            GD.PrintErr("Card 错误信息:" + e.Message);
            throw;
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

    #region 信号相关的方法

    /**
     * 鼠标进入后，发射悬停信号
     */
    private void OnMouseEnter()
    {
        //GD.Print("成功连接MouseEnter");
        // MyHoverSignal();
        EmitSignalHover(this);
    }

    private void OnMouseExit()
    {
        //GD.Print("成功连接MouseExit");
        // HoverOffSignal();
        EmitSignalHoverOff(this);
    }

    #endregion
}