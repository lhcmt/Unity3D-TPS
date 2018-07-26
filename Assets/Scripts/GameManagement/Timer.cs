using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour {

    private class TimedEvent
    {
        public float TimeToExecute;
        public Callback Method;
    }
    private List<TimedEvent> events;

    public delegate void Callback();

    void Awake()
    {
        events = new List<TimedEvent>();
    }
    //像list中添加
    public void Add(Callback method,float inSeconds)
    {
        events.Add(new TimedEvent
        {
            Method = method,
            TimeToExecute = Time.time + inSeconds
        });
    }

    void Update()
    {
        if (events.Count == 0)
            return;
        //检查List中每一个时间事件，如果到了要执行的事件，则运行托管事件，并从list删除
        for(int i = 0;i<events.Count;i++)
        {
            var timeEvent = events[i];
            if (timeEvent.TimeToExecute <= Time.time)
            {
                timeEvent.Method();
                events.Remove(timeEvent);
            }
        }
    }
}
