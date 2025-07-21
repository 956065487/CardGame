using System;
using System.Diagnostics;
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
    
    
}