using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CardGame.script;
using CardGame.script.constant;
using CardGame.script.pojo;

[GlobalClass]
public partial class Deck : Node2D
{
	#region 变量

	private List<String> _playerDeck = ["Knight", "Archer", "Demon", "Knight","Knight","Demon","Archer"];

	private RichTextLabel _numLabel;	// 显示卡组数量用

	private PlayerHand _playerHand;
	
	private bool _thisTurnDrawCard = false; // 本回合是否抽卡

	private int _startCardsNum = 3;
		
		
	#endregion
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Area2D area2D = GetNode<Area2D>("Area2D");
		area2D.CollisionLayer = Constant.LAYER_DECK;
		_numLabel = GetNode<RichTextLabel>("NumberLabel");
		_numLabel.Text = _playerDeck.Count.ToString();
		_playerHand = GetNodeOrNull<PlayerHand>("/root/Main/PlayerHand");
		
		for (int i = 0; i < _startCardsNum; i++)
		{
			DrawCard();
			_thisTurnDrawCard = false;
		}
		
	}

	/**
	 * 生成新的卡片
	 */
	public void DrawCard()
	{
		if (_thisTurnDrawCard)
		{
			return;
		}
		
		_thisTurnDrawCard = true;
		// 当牌堆没牌时
		if (_playerDeck.Count == 0)
		{
			// 此时是最后一张被发出去，重置隐藏等
			CollisionShape2D deckCollisionShape2D = 
				GetNodeOrNull<CollisionShape2D>("Area2D/CollisionShape2D");
			deckCollisionShape2D.Disabled = true;
			return;
		}
		else if (_playerDeck.Count == 1)
		{
			Sprite2D deckSprite2D = GetNodeOrNull<Sprite2D>("Sprite2D");
			deckSprite2D.Visible = false;
			_numLabel.Visible = false;
		}

		int randomCardIndex = GD.RandRange(0, _playerDeck.Count-1);
		var cardDraw = _playerDeck[randomCardIndex];
		_playerDeck.Remove(cardDraw);
		_numLabel.Text = _playerDeck.Count.ToString();
		
		//GD.Print("DrawCard");
		PackedScene cardScene = GD.Load<PackedScene>(Constant.CARD_SCENE_PATH);
		
		if (_playerHand == null)
		{
			Utils.PrintErr(this,"获取不到playerHand实例！");
			return;
		}
		
		Card newCard = (Card)cardScene.Instantiate();
		GetNode("/root/Main/CardManager").AddChild(newCard);
		newCard.Name = "Card";
		Sprite2D CardImg = newCard.GetNodeOrNull<Sprite2D>("CardImg");
		CardImg.Texture = GD.Load<Texture2D>($"res://asset/CardImg/" + cardDraw +"Card.png");
		
		CardInfo cardInfo = CardDataLoader.GetCardInfo(cardDraw);
		newCard.CardInfo = cardInfo;
		newCard.UpdateCardInfo();
		
		_playerHand.AddToHand(newCard);
		// 播放翻转动画
		newCard.GetNode<AnimationPlayer>("AnimationPlayer").Play("CardSlip");
		
		
	}
}
