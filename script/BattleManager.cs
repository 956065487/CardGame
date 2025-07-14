using Godot;
using System;
using Microsoft.VisualBasic.CompilerServices;
using Utils = CardGame.script.Utils;

public partial class BattleManager : Node2D
{
	#region 属性，变量

	
	private Button _endButton;	//回合结束按钮
	private OpponentDeck _opponentDeck;

	#endregion
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNodes();	// 获取所有需要的对象
		if (_endButton != null)
		{
			_endButton.Pressed += OnButtonPressed;
		}
		else
		{
			Utils.PrintErr(this,"未能获取到回合结束按钮！");
		}

		if (_opponentDeck != null)
		{
			_opponentDeck.drawEnemyCard();
		}
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	#region 自定义方法、信号

	private void GetNodes()
	{
		_endButton = GetNodeOrNull<Button>("/root/Main/EndButton");
		_opponentDeck = GetNodeOrNull<OpponentDeck>("/root/Main/OpponentDeck");
	}
	
	private void OnButtonPressed()
	{
		_endButton.Disabled = true;
		_endButton.Visible = false;
		Utils.Print(this,"成功连接回合结束按下信号，按钮按下");
	}

	#endregion
	
}
