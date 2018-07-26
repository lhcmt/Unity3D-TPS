using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
    * 背包中每条物品的属性
    * ID,
    * 名字
    * 最大数量
    * 当前数量
    */
[System.Serializable]
public class ContainerItem
{
    public int Id;
    public string Name;
    public int Maximum;
    public int currentNum;

    public ContainerItem(int id, string name, int max, int cur)
    {
        this.Id = id;
        this.Name = name;
        Maximum = max;
        currentNum = cur;
    }


    public int Get(int value)
    {
        if (currentNum - value < 0)
        {
            int toMuch = currentNum;//超出数量
            currentNum = 0;
            return toMuch;
            //list delete
        }
        currentNum -= value;
        return value;
    }

    public void Set(int amount)
    {
        currentNum += amount;
        //限制最大数量
        if (currentNum > Maximum)
            currentNum = Maximum;
    }
}


