using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace CardGame.script.cardAbility;

public partial class Tornado : MagicCard
{
    #region 属性
    
    public const int MAGIC_DAMAGE = 2;
    private BattleManager _battleManager;
    
    #endregion
    
    #region 声明周期方法

    public override void _Ready()
    {
        try
        {
            base._Ready();
        }
        catch (Exception e)
        {
            Utils.PrintErr(e.Message);
        }

        GetNodes();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    #endregion

    #region 自定义方法

    private void GetNodes()
    {
        AttackLabel = GetNodeOrNull<RichTextLabel>("Attack");
        HealthLabel = GetNodeOrNull<RichTextLabel>("Health");
        _battleManager = GetNodeOrNull<BattleManager>("/root/Main/BattleManager");
        
    }
    
    /**
     * 龙卷风能力
     * 对所有敌方单位造成2点伤害
     */
    private async Task BaseStormAbility<T>(List<T> cards) where T : Card
    {
        bool checkEnemyCard = CheckEnemyCard;
        if (checkEnemyCard)
        {
            SetGlobalPosition(new Vector2(514,617));
            await AnimateCardToPosition(new Vector2(1255, 617),1.2);
        }
        else
        {
            SetGlobalPosition(new Vector2(514,387));
            await AnimateCardToPosition(new Vector2(1255, 387),1.2);
        }
        
        for (var i = 0; i < cards.Count; i++)
        {
            cards[i].CardInfo.Hp = Mathf.Max(cards[i].CardInfo.Hp - MAGIC_DAMAGE, 0);
            cards[i].UpdateCardInfoToLabel();
            if (cards[i].CardInfo.Hp <= 0)
            {
                await _battleManager.DestroyCard(cards[i]);
                i = i - 1;
            }
        }
        
        _battleManager.WaitTimerBySecond(1);
        await ToSignal(_battleManager.GetBattleTimer(), "timeout");
    }
    
    public async Task StormAbility(List<EnemyCard> monsterCards)
    {
        await BaseStormAbility(monsterCards);
    }
    
    public async Task StormAbility(List<Card> monsterCards)
    {
        await BaseStormAbility(monsterCards);
    }

    #endregion


}