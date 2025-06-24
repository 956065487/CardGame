using Godot;
using System;
using System.Collections.Generic;

public partial class Deck : Node2D
{
	#region 变量

	private List<String> _playerDeck = ["Knight", "Knight", "Knight"];

	#endregion
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	public void DrawCard()
	{
		GD.Print("DrawCard");
		/*PackedScene cardScene = GD.Load<PackedScene>(CARD_SCENE_PATH);

		for (int i = 0; i < CARD_COUNT_IN_HAND; i++)
		{
			Card card = (Card)cardScene.Instantiate();
			GetNode("/root/Main/CardManager").AddChild(card);
			card.Name = "Card";
			AddToHand(card);
            
		}*/
	}
}
