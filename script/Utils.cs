using System;
using System.Collections.Generic;
using System.Diagnostics;
using CardGame.script.constant;
using Godot;

namespace CardGame.script;

public static class Utils
{
    /**
     * node：需要反应错误的节点，一般可直接填 this ，表示当前节点
     * 打印错误信息，附带类型和节点名，
     */
    public static void PrintErr(Node node,string msg)
    {
        var stackTrace = new StackTrace();
        var callingMethod = stackTrace.GetFrame(1).GetMethod();
        // 获取类名和方法名
        String callingMethodInfo = $"{callingMethod.DeclaringType.Name}.{callingMethod.Name}";
        
        GD.PrintErr("[" + node.GetType().Name + "]" + node.GetName() + " 错误: " + msg + $"            ------调用自{callingMethodInfo}------");
    }
    
    /**
     * 方法重载，单纯在GD printErr基础上添加“错误：”
     */
    public static void PrintErr(string msg)
    {
        GD.PrintErr(" 错误： " + msg);
    }
    
    /**
     * GD.Print
     */
    public static void Print(string msg)
    {
        GD.Print(msg);
    }
    
    /**
     * GD.Print,附带打印节点类型，节点名，默认使用this就能调用当前节点
     */
    public static void Print(Node node,string msg)
    {
        var stackTrace = new StackTrace();
        var callingMethod = stackTrace.GetFrame(1).GetMethod();
        // 获取类名和方法名
        String callingMethodInfo = $"{callingMethod.DeclaringType.Name}.{callingMethod.Name}";
        GD.Print("[" + node.GetType().Name + "]" + node.GetName() + " : " + msg + $"            ------调用自{callingMethodInfo}------");
    }

    /**
     * 随机生成（begin ~ end）个卡到卡组中
     * begin : 范围初始值
     * end : 范围最大值
     *
     * return false : 随机生成失败
     *        true : 随机生成成功
     */
    public static bool RandomCardInList(List<String> cardList,int begin, int end)
    {
        if (begin < 0 && begin > end)
        {
            return false;
        }
        
        int randomInt = GD.RandRange(begin, end);
        int randomMagicCard = randomInt / 3;
        for (int i = 0; i < randomInt; i++)
        {
            int randomAddMagicCard = GD.RandRange(0, 1);
            if (randomAddMagicCard == 0 && randomMagicCard > 0)
            {
                // 随机到0，添加魔法卡
                int randomMagicCardNames = GD.RandRange(0,Constant.MAGIC_CARD_NAME_LIST.Count -1);
                cardList.Add(Constant.MAGIC_CARD_NAME_LIST[randomMagicCardNames]);
                randomMagicCard = randomMagicCard - 1;
                continue;
            }
            int randomCardNames = GD.RandRange(0,Constant.CARD_NAME_LIST.Count -1);
            cardList.Add(Constant.CARD_NAME_LIST[randomCardNames]);
        }
        return true;
    }
}