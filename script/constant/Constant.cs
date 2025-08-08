using System;
using System.Collections.Generic;

namespace CardGame.script.constant;

public static class Constant
{
    public const uint LAYER_CARD = 1 << 0;        // 第 1 层
    public const uint LAYER_DECK = 1 << 1;        // 第 2 层
    public const uint LAYER_SLOT = 1 << 2;   // 第 3 层
    
    
    public const String CARD_SCENE_PATH = "res://scene/Card.tscn";
    public const String MAGIC_CARD_SCENE_PATH = "res://scene/MagicCard.tscn";
    public const String ENEMY_CARD_SCENE_PATH = "res://scene/EnemyCard.tscn";
    public const String TORNADO_CARD_SCENE_PATH = "res://scene/Tornado.tscn";

    public const int DEFAULT_CARD_WIDTH = 120;  // 默认卡牌宽度100，用于调整手牌中间距

    public const float DEFAULT_CARD_SCALE_SIZE = 0.8f;
    // 卡牌高亮大小
    public const float CARD_HIGH_LIGHT_SIZE = 1f;
    
    private const int CARD_WIDTH = 200;

    // 生命值设置
    public const int STARTING_HP = 30;

    public static readonly List<string> CARD_NAME_LIST = ["Knight", "Archer", "Demon","Tornado"];
}