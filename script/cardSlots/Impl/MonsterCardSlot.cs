namespace CardGame.script.cardSlots.Impl;

public partial class MonsterCardSlot : CardSlot
{

    public override void _Ready()
    {
        base._Ready();
        this.CardSlotType = "Monster";
    }
}