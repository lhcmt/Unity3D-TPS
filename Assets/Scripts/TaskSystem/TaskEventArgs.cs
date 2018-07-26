using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//任务事件 通信参数
public class TaskEventArgs
{
    //public string taskID;//任务ID
    public int conditionID;//条件ID 
    public int amount;//条件需求数量

    public TaskEventArgs()
    {

    }
    public TaskEventArgs(int conditionid,int n)
    {
        //taskID = taskid;
        conditionID = conditionid;
        amount = n;
    }

}
