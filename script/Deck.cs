using Godot;
using System;
using System.Collections.Generic;
using CardGame.script;
using CardGame.script.constant;

public partial class Deck : Node2D
{
	#region 变量

	private List<String> _playerDeck = ["Knight", "Knight", "Knight"];

	private RichTextLabel _numLabel;	// 显示卡组数量用

	#endregion
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Area2D area2D = GetNode<Area2D>("Area2D");
		area2D.CollisionLayer = Constant.LAYER_DECK;
		_numLabel = GetNode<RichTextLabel>("NumberLabel");
		_numLabel.Text = _playerDeck.Count.ToString();
	}

	public void DrawCard()
	{
		if (_playerDeck.Count == 0)
		{
			// 重置隐藏等
			CollisionShape2D deckCollisionShape2D = 
				GetNodeOrNull<CollisionShape2D>("Area2D/CollisionShape2D");
			deckCollisionShape2D.Disabled = true;
			Sprite2D deckSprite2D = GetNodeOrNull<Sprite2D>("Sprite2D");
			deckSprite2D.Visible = false;
			_numLabel.Visible = false;
			return;
		}
		var cardDraw = _playerDeck[0];
		_playerDeck.Remove(cardDraw);
		
		_numLabel.Text = _playerDeck.Count.ToString();
		
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
