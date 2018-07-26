using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : Photon.PunBehaviour,IPunObservable
{

    [Range(0, 100)]
    public float health =100f;
    //队伍
    public int faction; //what time he is on

    //ID
    public int ID;
    //name etc
    public string playerName;


    private PlayerUI playerUI;

    void Start()
    {
        playerUI = FindObjectOfType<PlayerUI>();
    }
    void Update()
    {
        //health = Mathf.Clamp(health, 0, 100);
    }

    public void ApplyDamage(float number)
    {
        if(!PhotonNetwork.connected || photonView.isMine)
            playerUI.damage_react.GetComponent<CanvasGroup>().alpha = 1;

        health -= number;
        if(health <0)
        {
            health = 0;
            //gameover();
        }
        if(health >100)
        {
            health = 100;
        }
    }

    //只有本机可以控制的PhotonView的 stream.isWriting 才等于True;
    //需要双向更改
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        if (stream.isWriting)
        {
            stream.SendNext(health);
        }
        else
        {
            this.health = (float)stream.ReceiveNext();
        }
    }
}
