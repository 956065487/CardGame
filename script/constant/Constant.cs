using System;

namespace CardGame.script.constant;

public static class Constant
{
    public const uint LAYER_CARD = 1 << 0;        // 第 1 层
    public const uint LAYER_DECK = 1 << 1;        // 第 2 层
    public const uint LAYER_SLOT = 1 << 2;   // 第 3 层
    
    
    public const String CARD_SCENE_PATH = "res://scene/Card.tscn"; 
}