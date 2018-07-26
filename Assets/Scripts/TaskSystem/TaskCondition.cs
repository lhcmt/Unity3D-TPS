using System.Collections;
using System;
/*
 * 任务完成条件
 */
public class TaskCondition
{
    public int id;//条件id 由于item的ID用了int ，所以这里也用Int,和ItemID相同
    public int nowAmount;//条件id的当前进度
    public int targetAmount;//条件id的目标进度
    public bool isFinish = false;//记录是否满足条件

    public TaskCondition(int id, int nowAmount, int targetAmount)
    {
        this.id = id;
        this.nowAmount = nowAmount;
        this.targetAmount = targetAmount;
    }

    public void CheckCondition()
    {
        if (nowAmount < 0) nowAmount = 0;
        if(nowAmount >= targetAmount)
        {
            isFinish = true;
        }
        else
        {
            isFinish = false;
        }
    }
}
