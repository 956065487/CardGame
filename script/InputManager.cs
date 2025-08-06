using Godot;
using System;
using CardGame.script;
using CardGame.script.constant;
using Godot.Collections;
using Tornado = CardGame.script.cardAbility.Tornado;

public partial class InputManager : Node2D
{
    #region 常量、变量、信号

    public CardManager CardManager;
    public Deck Deck;
    public CardSlot CardSlot;
    private BattleManager _battleManager;
    private Vector2 _positionOnMouseClick;

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
                Vector2 mousePosition = mouseEvent.Position;
                _positionOnMouseClick = mousePosition;
                // 发射信号
                //检查卡牌拖动等逻辑
                EmitSignalLeftMouseClicked();
                Node2D clickedNode2D = GetClickedNode2D(mousePosition);
                if (clickedNode2D is Card card)
                {
                    Card draggedCard = card;
                    if (draggedCard.GetCheckEnemyCard())
                    {
                        return;
                    }
                    CardManager.CardBeingDragged = draggedCard;
                    // Utils.Print(this,$"被拖拽的卡片类型是：{CardManager.CardBeingDragged.CardInfo.CardType}");
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
     * 获取鼠标下的Node节点
     */
    public Node2D GetClickedNode2D(Vector2 mousePosition)
    {
        var spaceState = GetWorld2D().DirectSpaceState;
        var query = new PhysicsPointQueryParameters2D();
        query.Position = mousePosition;
        query.CollideWithAreas = true;  // 如果卡牌使用area2d
        query.CollideWithBodies = true; // 如果使用RigidBody2d
        
        // 执行查询
        var results = spaceState.IntersectPoint(query,32);  //限制最多返回数量

        foreach (var result in results)
        {
            var collider = result["collider"].AsGodotObject();
            
            // 获取碰撞体的父节点
            Node2D clickedNode2D = null;
            if (collider is Node2D node2D)
            {
                clickedNode2D = node2D;
            }
            else if (collider is Node node)
            {
                clickedNode2D = node.GetParent<Node2D>();
            }

            if (collider is Area2D area2D)
            {
                // 如果是Area2D对象，尝试获取父节点
                clickedNode2D = area2D.GetParent<Node2D>();
            }

            if (clickedNode2D != null)
            {
                // 按继承层次判断之类，然后父类
                if (clickedNode2D is Tornado tornado)
                {
                    Utils.Print(this,$"Tornado 点击到了");
                    return tornado;
                }
                else if (clickedNode2D is MagicCard magicCard)
                {
                    Utils.Print(this,$"MagicCard 点击到了。类型 = {magicCard.CardInfo.CardType}");
                    return magicCard;
                }
                else if (clickedNode2D is Card card)
                {
                    Utils.Print(this,$"Card <UNK> = {card.CardInfo.CardType}");
                    return card;
                }
                else if (clickedNode2D is OpponentDeck opponentDeck)
                {
                    Utils.Print(this, $"获取到了敌方卡牌堆");
                    return opponentDeck;
                }
                else if (clickedNode2D is Deck deck)
                {
                    Utils.Print(this,"获取到了玩家卡牌堆");
                    return deck;
                }
                else if (clickedNode2D is CardSlot cardSlot)
                {
                    Utils.Print(this,"获取到了卡槽");
                    return cardSlot;
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
        
        //Utils.Print(this,$"检测到{overlappingAreas.Count}个重叠区域");

        foreach (Area2D overlappingArea in overlappingAreas)
        {
            // Utils.Print(this,$"重叠区域: {overlappingArea.Name} (类型: {overlappingArea.GetType().Name})");
            
            if (overlappingArea.GetParent() is CardSlot currentCardSlot && !currentCardSlot.CardInSlot)
            {
                if (!currentCardSlot.CardSlotType.Equals(CardManager.CardBeingDragged.CardInfo.CardType))
                {
                    // 若卡槽和卡牌不是相同类型，跳过当前循环
                    continue;
                }
                // 重叠区域为卡槽，并且卡槽为空
                // Utils.Print(this,$"找到一个空卡槽！{currentCardSlot.Name}");

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
            CardManager.CardBeingDragged.SetCardSlot(bestCardSlot);
            CardManager.CardBeingDragged.PositionInCardSlot = true; // 标记卡牌在卡槽中
            CardManager.PlayerHandNode2d.RemoveCardFromHand(CardManager.CardBeingDragged);

            if ("Monster".Equals(CardManager.CardBeingDragged.CardInfo.CardType))
            {
                _battleManager.AddToPlayerBattleCards(CardManager.CardBeingDragged);
            }
            else if ("Magic".Equals(CardManager.CardBeingDragged.CardInfo.CardType))
            {
                _battleManager.AddTOPlayerBattleMagicCards(CardManager.CardBeingDragged);
                _battleManager.UsingMagicCard((MagicCard)CardManager.CardBeingDragged);
            }
            // 无论吸附成功与否，都要重新启用卡牌的碰撞体
            CollisionShape2D cardCollisionShape = CardManager.CardBeingDragged
                .GetNodeOrNull<CollisionShape2D>("Area2D/CollisionShape2D");
            if (cardCollisionShape != null)
            {
                cardCollisionShape.Disabled = false;
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