namespace CardGame.script.cardSlots.Impl;

public partial class MagicCardSlot : CardSlot
{
    public override void _Ready()
    {
        base._Ready();
        CardSlotType = "Magic";
    }
}