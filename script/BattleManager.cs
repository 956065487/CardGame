using Godot;
using System;
using Microsoft.VisualBasic.CompilerServices;
using Utils = CardGame.script.Utils;

public partial class BattleManager : Node2D
{
	#region 属性，变量

	
	private TextureButton _endButton;	//回合结束按钮
	private OpponentDeck _opponentDeck;
	private Timer _battleTimer;

	#endregion
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNodes();	// 获取所有需要的对象
		ConnectSignals();	
		

		
		
	}



	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	#region 自定义方法、信号

	/**
	 * 用于统一获取实例对象
	 */
	private void GetNodes()
	{
		_endButton = GetNodeOrNull<TextureButton>("/root/Main/EndButton");
		_opponentDeck = GetNodeOrNull<OpponentDeck>("/root/Main/OpponentDeck");
		_battleTimer = GetNodeOrNull<Timer>("/root/Main/BattleTimer");
	}
	
	/**
	 * 用于统一管理信号链接
	 */
	private void ConnectSignals()
	{
		// 1、按钮按下信号连接
		if (_endButton != null)
		{
			_endButton.Pressed += OnEndButtonPressed;
		}
		else
		{
			Utils.PrintErr(this,"未能获取到回合结束按钮！");
		}
	}
	
	/**
	 * 回合结束按钮按下
	 */
	private void OnEndButtonPressed()
	{
		_endButton.Disabled = true;
		_endButton.Visible = false;
		Utils.Print(this,"成功连接回合结束按下信号，按钮按下");
		
		if (_opponentDeck != null)
		{
			_opponentDeck.drawEnemyCard();
		}
		else
		{
			Utils.PrintErr(this,"空指针，对方卡组未实例化");
		}
		
		// 等待1秒，模拟AI思考出牌
	}

	#endregion
	
}
