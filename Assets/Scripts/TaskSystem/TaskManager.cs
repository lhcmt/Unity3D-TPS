using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System; 
using UnityEngine.UI;
using System.Xml.Linq;


/*
 * 
 * 2018/6/10
 * 任务系统 
 *  * 挂载，GameController中
 */
public class TaskManager : MonoSingletion<TaskManager>
{

    #region public variable
    public Text TaskUIText;

    public List<Task> taskList = new List<Task>();
    public XElement rootElement;


    #endregion

    #region private region
    //用来接收各种任务事件
    private List<TaskEventArgs> m_EventList;
    private List<TaskEventArgs> EventList
    {
        get
        {
            if (m_EventList == null)
                m_EventList = new List<TaskEventArgs>();
            return m_EventList;
        }
    }
    #endregion


    #region MonoCallBack
    void Start()
    {
        TaskUIText = FindObjectOfType<PlayerUI>().TaskUI;
        rootElement = XElement.Load(Application.dataPath + "/Scripts/TaskSystem/tasks.xml");//得到根元素
        Level1();
        UpdateTaskOnUI();
    }

    void Update()
    {
        //从EventBus获取想要的事件
        CheckTaskEvent();
        UpdateTaskOnUI();
    }
    #endregion

    #region public method

    //在UI上显示所有任务
    public void UpdateTaskOnUI()
    {
        if (TaskUIText != null)
        {
            TaskUIText.text = "";
            foreach (Task task in taskList)
            {
                TaskUIText.text +=
                    task.taskName + ":"+ 
                    "\n" +
                    task.caption;
                foreach (TaskCondition taskCondition in task.taskConditions)
                {
                    if (taskCondition.id !=001)
                        TaskUIText.text += "\n" + taskCondition.nowAmount + "//" + taskCondition.targetAmount;
                }
                TaskUIText.text += "\n\n";
            }
        }
    }

    //创建任务
    public void CreateTask(string taskID)
    {
        Task newTask = new Task(taskID);
        taskList.Add(newTask);
    }
    //从NPC接受任务
    public void AcceptTask(string taskID)
    {
        foreach (Task task in taskList)
        {
            if (task.taskID == taskID)
                return;
        }
        CreateTask(taskID);
    }

    //任务完成或者取消后删除某个任务
    public void DeleteTask(string taskIDtoDelete)
    {
        foreach(Task task in taskList)
        {
            if(task.taskID ==taskIDtoDelete)
            {
                taskList.Remove(task);
                return;
            }
        }
    }
    //暴露接口，给其他类调用，添加任务事件
    public void AddTaskEvent(TaskEventArgs eArg)
    {
        EventList.Add(eArg);
    }


    public void CheckTaskEvent()
    {
        //如果队列为空
        if (EventList.Count == 0)
            return;
        foreach (var e in EventList)
        {
            //倒叙遍历删除
            for (int i = taskList.Count - 1; i >= 0;i-- )
            {
                if (taskList[i].Check(e))
                {
                    taskList[i].Reward();
                    taskList.Remove(taskList[i]);
                }
            }
        }
        EventList.Clear();
    }

    #endregion


    #region private methods
    //test
    void Level1()
    {
        CreateTask("Task1");
        CreateTask("Task2");
    }

    #endregion

}
