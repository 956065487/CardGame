using System;
using Godot;

namespace CardGame.script;

public partial class MagicCard : Card
{
	#region 属性

	public AnimatedSprite2D AnimatedSprite2D;
	private AnimationPlayer _animationPlayer;

	#endregion
	#region 生命周期方法

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		try
		{
			base._Ready();
		}
		catch (Exception e)
		{
			Utils.PrintErr(this, "调用父节点异常，继续执行");
		}

		GetNodes();
		ConnectSignals();
		

		
	}
	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
		
	}

	#endregion
	
	
	#region 自定义方法

	
	
	private void GetNodes()
	{
		// GD.Print("MagicCard get nodes");
		AnimatedSprite2D = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		_animationPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
		if (AnimatedSprite2D == null)
		{
			Utils.PrintErr(this, "AnimatedSprite2D get nodes null");
		} else if (_animationPlayer == null)
		{
			Utils.PrintErr(this, "AnimationPlayer get nodes null");
		}
	}

	private void ConnectSignals()
	{
		// Utils.Print(this,"MagicCard connect signals");
		_animationPlayer.AnimationFinished += OnAnimationPlayerFinished;
	}

	public void OnAnimationPlayerFinished(StringName animName)
	{
		// Utils.Print($"{animName} Magic AnimationPlayer finished");
		AnimatedSprite2D.Scale = new Vector2(1.5f, 1.73f);
		AnimatedSprite2D.SpriteFrames.SetAnimationLoop("state1",true);
		AnimatedSprite2D.Play("state1");
	}

	#endregion
}