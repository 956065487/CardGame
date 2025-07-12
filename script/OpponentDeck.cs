using Godot;
using System;

public partial class OpponentDeck : Deck
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// base._Ready(); 不调用即可保证对手牌组玩家无法点击
		GD.Print("OpponentDeck");
		Area2D opponentArea2D = GetNodeOrNull<Area2D>("Area2D");
		opponentArea2D.Monitorable = false;
		opponentArea2D.Monitoring = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);
	}
}
