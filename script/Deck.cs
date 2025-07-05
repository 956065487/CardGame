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
		if (_playerDeck.Count == 0)
		{
			CollisionShape2D deckCollisionShape2D = 
				GetNodeOrNull<CollisionShape2D>("Area2D/CollisionShape2D");
			deckCollisionShape2D.Disabled = true;
			Sprite2D deckSprite2D = GetNodeOrNull<Sprite2D>("Sprite2D");
			deckSprite2D.Visible = false;
			return;
		}
		var cardDraw = _playerDeck[0];
		_playerDeck.Remove(cardDraw);
		
		GD.Print("DrawCard");
		PackedScene cardScene = GD.Load<PackedScene>(Constant.CARD_SCENE_PATH);
		PlayerHand playerHand = GetNodeOrNull<PlayerHand>("/root/Main/PlayerHand");
		if (playerHand == null)
		{
			Utils.PrintErr(this,"获取不到playerHand实例！");
			return;
		}
		
		Card newCard = (Card)cardScene.Instantiate();
		GetNode("/root/Main/CardManager").AddChild(newCard);
		newCard.Name = "Card";
		playerHand.AddToHand(newCard);
		
	}
}
