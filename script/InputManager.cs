using Godot;
using System;
using CardGame.script;
using CardGame.script.constant;
using Godot.Collections;

public partial class InputManager : Node2D
{
    #region 常量、变量、信号

    public CardManager CardManager;
    public Deck Deck;
    public CardSlot CardSlot;
    private BattleManager _battleManager;

    [Signal]
    public delegate void LeftMouseClickedEventHandler();

    [Signal]
    public delegate void LeftMouseReleasedEventHandler();

    #endregion

    #region 生命周期方法

    public override void _Ready()
    {
        GetNodes();
    }

    private void GetNodes()
    {
        CardManager = GetNode<CardManager>("/root/Main/CardManager");
        Deck = GetNode<Deck>("/root/Main/Deck");
        _battleManager = GetNode<BattleManager>("/root/Main/BattleManager");


        if (CardManager == null)
        {
            Utils.PrintErr(this,$"CardManager 没有获取到节点");
        }
        else if (Deck == null)
        {
            Utils.PrintErr(this,$"Deck 没有获取到节点");
        } 
        else if (_battleManager == null)
        {
            Utils.PrintErr(this,$"BattleManager 没有获取到节点");
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            if (mouseEvent.Pressed)
            {
                EmitSignalLeftMouseClicked();
                // 
                Node2D checkForCursor = CheckForCursor();
                // 调试用
                // GD.Print($"InputManager._Input (鼠标按下): CheckForCursor 返回: {checkForCursor?.Name} (类型: {checkForCursor?.GetType().Name})"); 
                if (checkForCursor is Card)
                {
                    Card draggedCard = (Card)checkForCursor;
                    if (draggedCard.GetCheckEnemyCard())
                    {
                        return;
                    }
                    CardManager.CardBeingDragged = draggedCard;
                    Utils.Print(this,$"被拖拽的卡片类型是：{CardManager.CardBeingDragged.CardInfo.CardType}");
                }
            }
            else
            {
                EmitSignalLeftMouseReleased();
                FinishedDragged();
            }
        }
    }

    #endregion

    #region 自定义方法

    /**
     * 获取光标下的东西
     * 如果不为 Card 对象，返回Null
     */
    public Node2D CheckForCursor()
    {
        // 获取被点击的Area2d的父级
        PhysicsDirectSpaceState2D spaceState2D = GetWorld2D().DirectSpaceState;
        var parameters2D = new PhysicsPointQueryParameters2D();
        parameters2D.Position = GetGlobalMousePosition();
        parameters2D.CollideWithAreas = true;
        Array<Dictionary> result = spaceState2D.IntersectPoint(parameters2D);
        if (result.Count > 0)
        {
            //判断 CollisionMask 是属于 什么类型的对象
            Node2D colliderNode2D = result[0]["collider"].As<Node2D>();
            if (colliderNode2D == null)
            {
                Utils.PrintErr(this, "colliderNode2D is null");
            }

            CollisionObject2D colliderObject = colliderNode2D as CollisionObject2D;
            if (colliderObject == null)
            {
                Utils.PrintErr(this, "colliderNode2D is not a CollisionObject2D");
            }

            var resultCollisionLayer = colliderObject.CollisionLayer;
            if (resultCollisionLayer == Constant.LAYER_CARD)
            {
                // Card对象 被 点击
                if (colliderObject.GetParent().GetType() == typeof(Card))
                {
                    return colliderObject.GetParent() as Card;
                }
            }
            else if (resultCollisionLayer == Constant.LAYER_DECK)
            {
                Deck.DrawCard();    // 生成卡牌
            }
            else if (resultCollisionLayer == Constant.LAYER_SLOT)
            {
                // 卡槽 被 点击
                if (colliderObject.GetParent() is CardSlot)
                {
                    CardSlot cardSlot = colliderObject.GetParent() as CardSlot;
                    Utils.Print(cardSlot,$"这里是卡槽,卡槽类型是 : {cardSlot.CardSlotType}");
                    return colliderObject.GetParent() as CardSlot;
                }
                
            }
        }

        return null;
    }

    /**
     * 检测2个矩形重叠面积
     */
    private float CalculateOverlapArea(Rect2 rect1, Rect2 rect2)
    {
        Rect2 intersection = rect1.Intersection(rect2);
        if (intersection.Size.X <= 0 || intersection.Size.Y <= 0)
        {
            return 0.0f; //没有重叠
        }
        return intersection.Size.X * intersection.Size.Y;
    }
    

    /**
     * 鼠标放开，不再拖拽时
     */
    private void FinishedDragged()
    {
        if (CardManager.CardBeingDragged == null)
        {
            return;
        }

        Area2D draggedCardArea2d = CardManager.CardBeingDragged.GetNodeOrNull<Area2D>("Area2D");

        if (draggedCardArea2d == null)
        {
            Utils.PrintErr(this, "被拖拽卡牌没有名为 'Area2D' 的 Area2D 子节点。无法检测重叠");
            // 无法检测，移回手牌
            CardManager.PlayerHandNode2d.AddToHand(CardManager.CardBeingDragged);
            CardManager.CardBeingDragged = null;
            return;
        }

        Array<Area2D> overlappingAreas = draggedCardArea2d.GetOverlappingAreas(); //获取所有与拖拽卡片重叠的区域
        
        CardSlot bestCardSlot = null;   // 储存最优卡槽
        float maxOverlap = 0.0f;        //储存最大重叠面积
        
        Utils.Print(this,$"检测到{overlappingAreas.Count}个重叠区域");

        foreach (Area2D overlappingArea in overlappingAreas)
        {
            Utils.Print(this,$"重叠区域: {overlappingArea.Name} (类型: {overlappingArea.GetType().Name})");
            
            if (overlappingArea.GetParent() is CardSlot currentCardSlot && !currentCardSlot.CardInSlot)
            {
                if (!currentCardSlot.CardSlotType.Equals(CardManager.CardBeingDragged.CardInfo.CardType))
                {
                    // 若卡槽和卡牌不是相同类型，跳过当前循环
                    continue;
                }
                // 重叠区域为卡槽，并且卡槽为空
                Utils.Print(this,$"找到一个空卡槽！{currentCardSlot.Name}");

                // 获取卡牌矩形全局矩形
                Rect2 cardRect2 = CardManager.CardBeingDragged.GetViewportRect();
                cardRect2.Position += CardManager.CardBeingDragged.GlobalPosition;
                
                // 获取卡槽全局矩形
                Rect2 currentCardSlotRect2 = currentCardSlot.GetViewportRect();
                currentCardSlotRect2.Position += currentCardSlot.GlobalPosition;

                float calculateOverlapArea = CalculateOverlapArea(cardRect2, currentCardSlotRect2);
                // Utils.Print(this,$"重叠面积 = {calculateOverlapArea}");

                if (calculateOverlapArea > maxOverlap)
                {
                    maxOverlap = calculateOverlapArea;
                    bestCardSlot = currentCardSlot;
                }
            }
        }
        
        // 判断是否获得了最好的卡槽
        if (bestCardSlot != null && maxOverlap > 0)
        {
            CardManager.CardBeingDragged.Position = bestCardSlot.Position;
            bestCardSlot.CardInSlot = true; // 标记卡槽被占用
            CardManager.PlayerHandNode2d.RemoveCardFromHand(CardManager.CardBeingDragged);
            _battleManager.AddToPlayerBattleCards(CardManager.CardBeingDragged);
            // 无论吸附成功与否，都要重新启用卡牌的碰撞体
            CollisionShape2D cardCollisionShape = CardManager.CardBeingDragged
                .GetNodeOrNull<CollisionShape2D>("Area2D/CollisionShape2D");
            if (cardCollisionShape != null)
            {
                cardCollisionShape.Disabled = true;
            }
            else
            {
                Utils.PrintErr(this, "FinishedDragged: 被拖拽卡牌的 Area2D 下未找到 CollisionShape2D。请检查路径。");
            }
        }
        else
        {
            CardManager.PlayerHandNode2d.AddToHand(CardManager.CardBeingDragged);
        }
        
        
        
        CardManager.CardBeingDragged = null;
    }
    
    

    #endregion
}