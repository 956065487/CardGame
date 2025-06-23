using Godot;

namespace CardGame.script;

public static class Utils
{
    public static void PrintErr(Node node,string msg)
    {
        GD.PrintErr("[" + node.GetType().Name + "]" + node.GetName() + " 错误: " + msg);
    }
    
    public static void PrintErr(string msg)
    {
        GD.PrintErr(" 错误： " + msg);
    }
}