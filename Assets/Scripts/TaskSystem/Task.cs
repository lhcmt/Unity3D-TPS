/*
 * 2017/11/26
 * 任务类
 * 用于创建单个任务
 * 被观察对象
 */

using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System;
using UnityEngine;

public class Task{


    public string taskID;           //任务ID  
    public string taskName;         //任务名字  
    public string caption;          //任务描述   

    public List<TaskCondition> taskConditions = new List<TaskCondition>(); //任务条件
    public List<ContainerItem> taskRewards = new List<ContainerItem>();
    //构造函数
    public Task(string taskID)
    {
        this.taskID = taskID;
        //从XML文件中文本读入任务信息
        XElement xe = TaskManager.Instance.rootElement.Element(taskID);
        taskName = xe.Element("taskName").Value;
        caption = xe.Element("caption").Value;
        //获取完成条件元素
        IEnumerable<XElement> a = xe.Elements("conditionID");
        IEnumerator<XElement> b = xe.Elements("conditionTargetAmount").GetEnumerator();

        IEnumerable<XElement> c = xe.Elements("rewardItemID");
        IEnumerator<XElement> d = xe.Elements("rewardAmount").GetEnumerator();
        IEnumerator<XElement> e = xe.Elements("rewardItemName").GetEnumerator();
        IEnumerator<XElement> f = xe.Elements("ItemMaxNum").GetEnumerator();

        foreach (var s in a)
        {
            b.MoveNext();
            TaskCondition tc = new TaskCondition(int.Parse(s.Value), 0, int.Parse(b.Current.Value));
            taskConditions.Add(tc);
        }
        foreach (var s in c)
        {
            d.MoveNext();
            e.MoveNext();
            f.MoveNext();
            ContainerItem cIt = new ContainerItem(int.Parse(s.Value), e.Current.Value, int.Parse(d.Current.Value), int.Parse(f.Current.Value));
            taskRewards.Add(cIt);
        }

    }


    //任务物品发生变化时调用
    //检查任务的每个完成条件
    public bool Check(TaskEventArgs taskEventArgs)
    {
        TaskCondition tc;
        for (int i = 0; i < taskConditions.Count; i++)
        {
            tc = taskConditions[i];
            if (tc.id == taskEventArgs.conditionID)
            {
                tc.nowAmount += taskEventArgs.amount;
                tc.CheckCondition();
            }
        }

        //如果所有条件都完成，返回true
        for (int i = 0; i < taskConditions.Count; i++)
        {
            if (taskConditions[i].isFinish == false)
            {
                return false; 
            }
        }
        return true;

    }
    //奖励物品，数据从xml读入
    public void Reward()
    {
        Debug.Log(taskID + ":Reward()");
    }

}

