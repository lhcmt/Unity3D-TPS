/*
 * 暂时没用这个脚本
 * 2017/11/26
 * 事件队列 
 * 
 * 不挂载，直接载GameController中创建实例
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventBus  {

    public class EventListener
    {
        //定义委托（函数指针），返回值为void 参数 void
        public delegate void Callback();
        public bool IsSingleShot;//一次性的事件
        public  Callback Method;
        //构造函数
        public EventListener()
        {
            IsSingleShot = true;
        }
    }
    //事件队列，存储不同类型的事件队列
    private Dictionary<string, IList<EventListener>> m_EventTable;
    private Dictionary<string, IList<EventListener>> EventTable
    {
        get
        {
            if (m_EventTable == null)
                m_EventTable = new Dictionary<string, IList<EventListener>>();
            return m_EventTable;
        }
    }

    //对外接口，向EventTable添加事件listener
    public void AddListener(string name,EventListener listener)
    {
        if(!EventTable.ContainsKey(name))
            EventTable.Add(name, new List<EventListener>());
        if (EventTable[name].Contains(listener))
            return;
        EventTable[name].Add(listener);
    }
    //触发指定类型事件的方法
    public void RaiseEvent(string name)
    {
        if (!EventTable.ContainsKey(name))
            return;
        for(int i =0; i < EventTable[name].Count; i++)
        {
            EventListener listener = EventTable[name][i];
            listener.Method();
            if (listener.IsSingleShot)
                EventTable[name].Remove(listener);
        }
    }

    
}
