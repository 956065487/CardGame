using Godot;
using System;
using System.Collections.Generic;
using CardGame.script;
using CardGame.script.constant;

public partial class Deck : Node2D
{
	#region 变量

	private List<String> _playerDeck = ["Knight", "Knight", "Knight"];

	#endregion
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Area2D area2D = GetNode<Area2D>("Area2D");
		area2D.CollisionLayer = Constant.LAYER_DECK;
	}

	public void DrawCard()
	{
		GD.Print("DrawCard");
		PackedScene cardScene = GD.Load<PackedScene>(Constant.CARD_SCENE_PATH);
		PlayerHand playerHand = GetNodeOrNull<PlayerHand>("/root/Main/PlayerHand");
		if (playerHand == null)
		{
			Utils.PrintErr(this,"获取不到playerHand实例！");
			return;
		}
		
		for (int i = 0; i < _playerDeck.Count; i++)
		{
			Card newCard = (Card)cardScene.Instantiate();
			GetNode("/root/Main/CardManager").AddChild(newCard);
			newCard.Name = "Card";
			playerHand.AddToHand(newCard);
		}
	}
}
