using System;
using CardGame.script;
using CardGame.script.pojo;
using Godot;
using Godot.Collections;

// 引用 Godot.Collections 命名空间

/**
 * 通过读取JSON对象，获取卡牌数据
 */
public partial class CardDataLoader : Node
{
    #region 变量

    // 用于存储所有卡牌数据的字典，键是卡牌ID（如"Knight"），值是CardStats对象
    private static Dictionary _allCardInfos = new Dictionary();

    // JSON文件路径
    private const string CARDS_DATA_PATH = "res://script/datebase/CardInfo.json";

    #endregion


    #region 生命周期中的方法

    public override void _Ready()
    {
        LoadCardData();
        //CardInfo cardInfo = GetCardInfo("Knight");
        //Utils.Print(cardInfo.ToString());
    }

    #endregion

    #region 自定义方法

    /**
     * 加载卡牌数据的方法
     */
    private void LoadCardData()
    {
        // 检查文件是否存在
        if (!FileAccess.FileExists(CARDS_DATA_PATH))
        {
            Utils.PrintErr(this,$"错误: JSON文件不存在于 '{CARDS_DATA_PATH}'");
            return;
        }

        // 打开文件
        using var file = FileAccess.Open(CARDS_DATA_PATH, FileAccess.ModeFlags.Read);
        if (file == null)
        {
            Utils.PrintErr(this,$"无法打开文件 '{CARDS_DATA_PATH}'. 错误码: {FileAccess.GetOpenError()}");
            return;
        }

        // 读取文件内容
        string jsonText = file.GetAsText();
        // GD.Print($"成功读取JSON文件内容:\n{jsonText}");

        Dictionary newDictionary = Json.ParseString(jsonText).AsGodotDictionary();
        Dictionary cardInfosDictionary = newDictionary["CardInfos"].AsGodotDictionary();
        _allCardInfos = cardInfosDictionary;
    }

    /**
     * 根据卡牌ID获取卡牌数据
     */
    public static CardInfo GetCardInfo(string cardId)
    {
        if (_allCardInfos.ContainsKey(cardId))
        {
            return new CardInfo(_allCardInfos[cardId].AsGodotDictionary());
        }

        Utils.PrintErr($"未找到ID为 '{cardId}' 的卡牌数据。");
        return null;
    }

    #endregion
}