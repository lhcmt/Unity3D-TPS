using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*
 * 父类，使用时集成此类
 * 使用E键
 * 拾取子弹，武器
 * 开门
 */
public class PickUpItems : Photon.PunBehaviour {
    [SerializeField]
    public ContainerItem itemInfo;

    //算了不做名字显示和按E拾取了
    //E键操作留给开门
    void OnTriggerEnter(Collider collider)
    {
        if(collider.tag !="Player")
        {
            return;
        }

        PickUp(collider);
    }

    void PickUp(Collider collider)
    {
        OnPickup(collider);
    }

    //虚函数
    public virtual void OnPickup(Collider collider)
    {
        
    }


}
