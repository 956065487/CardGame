namespace CardGame.script.cardSlots.Impl;

public partial class EnemyCardSlot : CardSlot
{
    public override void _Ready()
    {
        base._Ready();
        CardSlotType = "Enemy";
    }
}