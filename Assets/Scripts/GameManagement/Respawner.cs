using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * GameObject管理
 * 重新生成GameObject
 */

public class Respawner : Photon.PunBehaviour
{

    [System.Serializable]
    public class RespawnerSetting
    {
        public WaypointBase[] waypoints;

        public Transform respawnPlace;

        public GameObject[] zombiePrefebs;

        public int MaxZombieAmount = 5;

        public float RespawnRate = 15.0f;

    }
    public RespawnerSetting respawnerSetting;


    //public List object Respwaner place

    //Zombie pool 先暂时实现了
    int ZombieCurrentAmount =0;
    float lastRespawnTime;
    int Zoffest = 0;


    //object pool

    void Start()
    {
        for (int i = 0; i < respawnerSetting.waypoints.Length; i++)
        {
            respawnerSetting.waypoints[i].waitTime = Random.Range(1.1f, 5.0f);
        }
        lastRespawnTime = Time.time;
    }


    void Update()
    {
        if(Time.time - lastRespawnTime > respawnerSetting.RespawnRate)
        {
            if (!PhotonNetwork.connected)
            {
                RespawnZombie();
            }
            else if(PhotonNetwork.isMasterClient)
            {
                RespawnZombieOnNetwork();
            }
        }
    }
    //这个是旧版离线代码
    void RespawnZombie()
    {
        if (ZombieCurrentAmount <= respawnerSetting.MaxZombieAmount)
        {
            //随机一个僵尸
            GameObject newZombie = Instantiate<GameObject>(respawnerSetting.zombiePrefebs[Zoffest % respawnerSetting.zombiePrefebs.Length], 
                respawnerSetting.respawnPlace.position, 
                respawnerSetting.respawnPlace.rotation, transform);
            ZombieAI zAI = newZombie.GetComponent<ZombieAI>();


            for (int i = 0; i < zAI.patrolSettings.waypoints.Length; i++)
            {
                zAI.patrolSettings.waypoints[i] = respawnerSetting.waypoints[(i + Zoffest) % respawnerSetting.waypoints.Length];
            }

            ZombieStats zState = newZombie.GetComponent<ZombieStats>();

            zState.thisRespwaner = this;

            ZombieCurrentAmount++;

            lastRespawnTime = Time.time;
            
            Zoffest++;
            if(Zoffest >= 6)
                Zoffest = 0; 
        }
    }

    void RespawnZombieOnNetwork()
    {
        //debug
        if (respawnerSetting.zombiePrefebs == null )
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> respawnerSetting.zombiePrefebs is null", this);
            return;
        }

        if (ZombieCurrentAmount <= respawnerSetting.MaxZombieAmount)
        {
            GameObject  newZombie = PhotonNetwork.InstantiateSceneObject(this.respawnerSetting.zombiePrefebs[Zoffest % respawnerSetting.zombiePrefebs.Length].name,
                respawnerSetting.respawnPlace.position,
                respawnerSetting.respawnPlace.rotation, 0, null);

            PhotonView zombiephotonView = newZombie.GetComponent<PhotonView>();

            photonView.RPC("SetNewZombie", PhotonTargets.All, zombiephotonView.viewID);

            ZombieCurrentAmount++;
            lastRespawnTime = Time.time;

            Zoffest++;
            if (Zoffest >= 6)
                Zoffest = 0; 
        }


        //call RPC
    }
    [PunRPC]
    void SetNewZombie(int viewID)
    {
        //
        //Debug.Log("RPC SetNewZombie() called");

        PhotonView zombiephotonView = PhotonView.Find(viewID);
        if(zombiephotonView ==null)
        {
            Debug.Log("Can not find PhotonView with ID viewID , in SetNewZombie()");
            return;
        }
        ZombieAI zAI = zombiephotonView.gameObject.GetComponent<ZombieAI>();
        ZombieStats zState = zombiephotonView.gameObject.GetComponent<ZombieStats>();

        for (int i = 0; i < zAI.patrolSettings.waypoints.Length; i++)
        {
            zAI.patrolSettings.waypoints[i] = respawnerSetting.waypoints[(i + Zoffest) % respawnerSetting.waypoints.Length];
        }

        zState.thisRespwaner = this;

    }

    public void AmountOne()
    {
        ZombieCurrentAmount --;
        if (ZombieCurrentAmount < 0)
            ZombieCurrentAmount = 0;
    }

    //重新生成GameObject inSeconds
    public void Despawn(GameObject go, float inSeconds)
    {
        go.SetActive(false);
        GameController.gc.timer.Add(() =>{
            go.SetActive(true);
        }, inSeconds);
    }

}
