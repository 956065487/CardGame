using Godot;
using System;

public partial class CardSlot : Node2D
{
	public bool CardInSlot { get; set; }

	public override void _Ready()
	{
		Area2D area2D = GetNode<Area2D>("Area2D");
		area2D.CollisionMask = InputManager.COLLISION_MASK_CARD_SLOT;
		
	}
}
