using Godot;
using System;
using CardGame.script;
using CardGame.script.constant;

public partial class CardSlot : Node2D
{
	public bool CardInSlot { get; set; }

	public override void _Ready()
	{
		Area2D area2D = GetNodeOrNull<Area2D>("Area2D");
		if (area2D != null)
		{
			area2D.CollisionLayer = Constant.LAYER_SLOT;
			
			//要检测卡，所以设置卡片层
			area2D.CollisionMask = Constant.LAYER_CARD;
		}
		else
		{
			Utils.PrintErr(this,"Area2D Not Found");
		}
		
	}
}
