using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace CardGame.script.pojo;

/**
 * 用于存储单张卡牌的属性值
 */
public partial class CardInfo : GodotObject
{
    public String Name {set; get;}
    public int Hp {set; get;}
    public int Attack {set; get;}
    public String Description {set; get;}

    public CardInfo()
    {
    }

    public CardInfo(Dictionary data)
    {
        Name = data.ContainsKey("Name") ? data["Name"].ToString() : "未知";
        Hp = data.ContainsKey("Hp") ? (int)data["Hp"] : 0;
        Attack = data.ContainsKey("Attack") ? (int)data["Attack"] : 0;
        Description = data.ContainsKey("Description") ? data["Description"].ToString() : "";
    }

    public override string ToString()
    {
        return $"Name = {Name} , Hp = {Hp} , Attack = {Attack} , Description = {Description}";
    }
}